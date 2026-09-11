using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using VpnResidentialHub.Models;

namespace VpnResidentialHub.Services;

public interface IUniversalGatewayService
{
    void Start(int port, CancellationToken ct);
    void Stop();
    IReadOnlyList<ProxySession> GetActiveSessions();
    int ActiveConnectionsCount { get; }
    long TotalBytesServed { get; }
    long TotalBytesIn { get; }
    long TotalBytesOut { get; }
}

public class UniversalGatewayService : IUniversalGatewayService
{
    private readonly INodePoolService _nodePool;
    private readonly ITenantService _tenantService;
    private readonly IProxyManagerService _proxyManager;
    private readonly ISmartProxyRotationEngine _rotationEngine;
    private readonly IRelayPoolManagerService _relayPool;
    private readonly ILogger<UniversalGatewayService> _logger;
    private readonly ConcurrentDictionary<string, ProxySession> _activeSessions = new();
    private TcpListener? _listener;
    private long _totalBytesServed = 0;
    private long _totalBytesIn = 0;
    private long _totalBytesOut = 0;

    public int ActiveConnectionsCount => _activeSessions.Count;
    public long TotalBytesServed => _totalBytesServed;
    public long TotalBytesIn => _totalBytesIn;
    public long TotalBytesOut => _totalBytesOut;

    public UniversalGatewayService(
        INodePoolService nodePool,
        ITenantService tenantService,
        IProxyManagerService proxyManager,
        ISmartProxyRotationEngine rotationEngine,
        IRelayPoolManagerService relayPool,
        ILogger<UniversalGatewayService> logger)
    {
        _nodePool = nodePool;
        _tenantService = tenantService;
        _proxyManager = proxyManager;
        _rotationEngine = rotationEngine;
        _relayPool = relayPool;
        _logger = logger;
    }

    public void Start(int port, CancellationToken ct)
    {
        Stop();

        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start(200);

        _logger.LogInformation("=========================================================");
        _logger.LogInformation("  UNIVERSAL RESIDENTIAL PROXY GATEWAY ONLINE");
        _logger.LogInformation("  Listening on Port: {Port} (Supports SOCKS5 & HTTP CONNECT)", port);
        _logger.LogInformation("  Syntax: host:{Port}:username:password", port);
        _logger.LogInformation("=========================================================");

        _ = AcceptLoopAsync(_listener, ct);
    }

    public void Stop()
    {
        try
        {
            _listener?.Stop();
            _listener = null;
        }
        catch { }
    }

    public IReadOnlyList<ProxySession> GetActiveSessions() => _activeSessions.Values.ToList();

    private async Task AcceptLoopAsync(TcpListener listener, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync(ct);
                _ = HandleClientConnectionAsync(client, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (ObjectDisposedException) { break; }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[Gateway] Accept connection error");
            }
        }
    }

    private async Task HandleClientConnectionAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        {
            try
            {
                client.NoDelay = true;
                using var stream = client.GetStream();

                // Đọc byte đầu tiên để nhận diện giao thức (SOCKS5 vs HTTP)
                byte[] peekBuffer = new byte[1];
                int read = await stream.ReadAsync(peekBuffer.AsMemory(0, 1), ct);
                if (read == 0) return;

                if (peekBuffer[0] == 0x05)
                {
                    // Giao thức SOCKS5 (RFC 1928)
                    await HandleSocks5Async(client, stream, ct);
                }
                else
                {
                    // Giao thức HTTP Proxy / CONNECT
                    await HandleHttpProxyAsync(client, stream, peekBuffer[0], ct);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogTrace(ex, "[Gateway] Session error");
            }
        }
    }

    #region SOCKS5 Protocol Handler (RFC 1928 & RFC 1929)
    private async Task HandleSocks5Async(TcpClient client, NetworkStream stream, CancellationToken ct)
    {
        // 1. Đọc số method xác thực
        int nMethods = stream.ReadByte();
        if (nMethods <= 0) return;

        byte[] methods = new byte[nMethods];
        await stream.ReadExactlyAsync(methods, ct);

        // Chấp nhận: 0x02 (Username/Password) hoặc 0x00 (No Auth)
        bool supportsAuth = methods.Contains((byte)0x02);
        if (supportsAuth)
        {
            // Yêu cầu xác thực Username/Password (0x02)
            await stream.WriteAsync(new byte[] { 0x05, 0x02 }, ct);
        }
        else
        {
            // Cho phép No Auth (0x00) cho môi trường nội bộ
            await stream.WriteAsync(new byte[] { 0x05, 0x00 }, ct);
        }

        Tenant? tenant = null;
        string? targetCountry = null;
        string? targetCity = null;
        string? sessionId = null;
        int? sessionMinutes = null;
        string? targetNodeId = null;

        if (supportsAuth)
        {
            // RFC 1929: Xác thực User/Pass
            int authVersion = stream.ReadByte();
            if (authVersion != 0x01) return;

            int uLen = stream.ReadByte();
            byte[] uBytes = new byte[uLen];
            await stream.ReadExactlyAsync(uBytes, ct);
            string fullUsername = Encoding.UTF8.GetString(uBytes);

            int pLen = stream.ReadByte();
            byte[] pBytes = new byte[pLen];
            await stream.ReadExactlyAsync(pBytes, ct);
            string password = Encoding.UTF8.GetString(pBytes);

            // Phân tích username & xác thực
            var parseRes = _tenantService.ParseProxyUsername(fullUsername);
            tenant = parseRes.Tenant;
            targetCountry = parseRes.Country;
            targetCity = parseRes.City;
            sessionId = parseRes.SessionId;
            sessionMinutes = parseRes.SessionMinutes;
            targetNodeId = parseRes.TargetNodeId;

            if (tenant == null || tenant.Password != password)
            {
                // Xác thực thất bại: 0x01, 0x01 (FAILURE)
                await stream.WriteAsync(new byte[] { 0x01, 0x01 }, ct);
                _logger.LogWarning("[SOCKS5] Xác thực thất bại cho username '{User}'", fullUsername);
                return;
            }

            // Xác thực thành công: 0x01, 0x00 (SUCCESS)
            await stream.WriteAsync(new byte[] { 0x01, 0x00 }, ct);
        }

        // 2. Client Request (CMD, DST.ADDR, DST.PORT)
        byte[] reqHeader = new byte[4];
        await stream.ReadExactlyAsync(reqHeader, ct);
        if (reqHeader[0] != 0x05 || reqHeader[1] != 0x01) return; // Chỉ hỗ trợ CONNECT

        byte atyp = reqHeader[3];
        string targetHost = string.Empty;
        int targetPort = 0;

        if (atyp == 0x01) // IPv4
        {
            byte[] ipBytes = new byte[4];
            await stream.ReadExactlyAsync(ipBytes, ct);
            targetHost = new IPAddress(ipBytes).ToString();
        }
        else if (atyp == 0x03) // Domain name
        {
            int domainLen = stream.ReadByte();
            byte[] domainBytes = new byte[domainLen];
            await stream.ReadExactlyAsync(domainBytes, ct);
            targetHost = Encoding.ASCII.GetString(domainBytes);
        }
        else if (atyp == 0x04) // IPv6
        {
            byte[] ip6Bytes = new byte[16];
            await stream.ReadExactlyAsync(ip6Bytes, ct);
            targetHost = new IPAddress(ip6Bytes).ToString();
        }

        byte[] portBytes = new byte[2];
        await stream.ReadExactlyAsync(portBytes, ct);
        targetPort = (portBytes[0] << 8) | portBytes[1];

        // 3. Chọn Residential Node phù hợp với yêu cầu
        ResidentialNode? assignedNode = null;
        if (!string.IsNullOrWhiteSpace(targetNodeId))
        {
            var clientNode = _nodePool.GetClientNodes().FirstOrDefault(n => n.DeviceId.Equals(targetNodeId, StringComparison.OrdinalIgnoreCase) && n.IsOnline);
            if (clientNode != null)
            {
                assignedNode = new ResidentialNode
                {
                    Id = clientNode.DeviceId,
                    Ip = clientNode.PublicIp,
                    Country = clientNode.Country,
                    CountryName = clientNode.CountryName,
                    City = clientNode.City,
                    Isp = clientNode.Isp,
                    Protocol = clientNode.Protocol,
                    IsActive = true
                };
            }
        }
        assignedNode ??= _nodePool.GetBestNode(targetCountry, targetCity);

        // 4. Kết nối ra ngoài Internet (qua Smart SaaS Rotation Engine & Upstream Proxy)
        TcpClient? upstreamClient = null;
        Stream? upstreamStream = null;
        string? activeProxyId = null;
        string? activeRelayId = null;
        try
        {
            (upstreamClient, upstreamStream, activeProxyId, activeRelayId) = await ConnectToUpstreamAsync(
                targetHost, targetPort, tenant, targetCountry, targetCity, sessionId, sessionMinutes, ct);

            // Báo thành công SOCKS5
            byte[] reply = new byte[] {
                0x05, 0x00, 0x00, 0x01,
                127, 0, 0, 1,
                (byte)(targetPort >> 8), (byte)(targetPort & 0xFF)
            };
            await stream.WriteAsync(reply, ct);

            // 5. Cầu nối chuyển tiếp & thống kê băng thông
            var sessId = Guid.NewGuid().ToString("N")[..8];
            var session = new ProxySession
            {
                SessionId = sessId,
                TenantId = tenant?.Id ?? "anonymous",
                ClientIp = client.Client.RemoteEndPoint?.ToString() ?? "unknown",
                Node = assignedNode,
                TargetHost = targetHost,
                TargetPort = targetPort,
                ConnectedAt = DateTime.UtcNow
            };
            _activeSessions[sessId] = session;

            try
            {
                await BridgeStreamsWithAccountingAsync(stream, upstreamStream, tenant?.Id, assignedNode, activeProxyId, activeRelayId, ct);
            }
            finally
            {
                _activeSessions.TryRemove(sessId, out _);
            }
        }
        catch
        {
            byte[] failReply = new byte[] { 0x05, 0x04, 0x00, 0x01, 0, 0, 0, 0, 0, 0 };
            try { await stream.WriteAsync(failReply, ct); } catch { }
        }
        finally
        {
            upstreamClient?.Dispose();
        }
    }
    #endregion

    #region HTTP Proxy / CONNECT Protocol Handler
    private async Task HandleHttpProxyAsync(TcpClient client, NetworkStream stream, byte firstByte, CancellationToken ct)
    {
        // Đọc toàn bộ HTTP Header ban đầu
        using var ms = new MemoryStream();
        ms.WriteByte(firstByte);

        byte[] lineBuffer = new byte[1024];
        bool headerComplete = false;

        while (!headerComplete && ms.Length < 16384)
        {
            int r = await stream.ReadAsync(lineBuffer.AsMemory(0, lineBuffer.Length), ct);
            if (r == 0) break;
            ms.Write(lineBuffer, 0, r);

            byte[] current = ms.ToArray();
            for (int i = 3; i < current.Length; i++)
            {
                if (current[i - 3] == '\r' && current[i - 2] == '\n' && current[i - 1] == '\r' && current[i] == '\n')
                {
                    headerComplete = true;
                    break;
                }
            }
        }

        string rawHeader = Encoding.UTF8.GetString(ms.ToArray());
        var lines = rawHeader.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return;

        string requestLine = lines[0]; // e.g. "CONNECT target.com:443 HTTP/1.1"
        var reqParts = requestLine.Split(' ');
        if (reqParts.Length < 2) return;

        string method = reqParts[0].ToUpperInvariant();
        string target = reqParts[1];

        // Xác thực Proxy-Authorization nếu có
        Tenant? tenant = null;
        string? targetCountry = null;
        string? targetCity = null;
        string? sessionId = null;
        int? sessionMinutes = null;
        string? targetNodeId = null;

        var authHeader = lines.FirstOrDefault(l => l.StartsWith("Proxy-Authorization:", StringComparison.OrdinalIgnoreCase));
        if (authHeader != null)
        {
            var authVal = authHeader.Substring("Proxy-Authorization:".Length).Trim();
            if (authVal.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                var base64 = authVal.Substring("Basic ".Length).Trim();
                try
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                    var uParts = decoded.Split(':');
                    if (uParts.Length >= 2)
                    {
                        var fullUser = uParts[0];
                        var pass = uParts.Skip(1).Aggregate((a, b) => a + ":" + b);

                        var pRes = _tenantService.ParseProxyUsername(fullUser);
                        tenant = pRes.Tenant;
                        targetCountry = pRes.Country;
                        targetCity = pRes.City;
                        sessionId = pRes.SessionId;
                        sessionMinutes = pRes.SessionMinutes;
                        targetNodeId = pRes.TargetNodeId;

                        if (tenant == null || tenant.Password != pass)
                        {
                            // Trả về 407 Proxy Authentication Required
                            byte[] authFail = Encoding.UTF8.GetBytes("HTTP/1.1 407 Proxy Authentication Required\r\nProxy-Authenticate: Basic realm=\"NextAI Residential Gateway\"\r\nContent-Length: 0\r\n\r\n");
                            await stream.WriteAsync(authFail, ct);
                            return;
                        }
                    }
                }
                catch { }
            }
        }

        string targetHost;
        int targetPort;

        if (method == "CONNECT")
        {
            var hostPort = target.Split(':');
            targetHost = hostPort[0];
            targetPort = hostPort.Length > 1 && int.TryParse(hostPort[1], out var p) ? p : 443;
        }
        else
        {
            // HTTP thông thường
            if (Uri.TryCreate(target, UriKind.Absolute, out var uri))
            {
                targetHost = uri.Host;
                targetPort = uri.Port;
            }
            else
            {
                return;
            }
        }

        ResidentialNode? assignedNode = null;
        if (!string.IsNullOrWhiteSpace(targetNodeId))
        {
            var clientNode = _nodePool.GetClientNodes().FirstOrDefault(n => n.DeviceId.Equals(targetNodeId, StringComparison.OrdinalIgnoreCase) && n.IsOnline);
            if (clientNode != null)
            {
                assignedNode = new ResidentialNode
                {
                    Id = clientNode.DeviceId,
                    Ip = clientNode.PublicIp,
                    Country = clientNode.Country,
                    CountryName = clientNode.CountryName,
                    City = clientNode.City,
                    Isp = clientNode.Isp,
                    Protocol = clientNode.Protocol,
                    IsActive = true
                };
            }
        }
        assignedNode ??= _nodePool.GetBestNode(targetCountry, targetCity);

        TcpClient? upstreamClient = null;
        Stream? upstreamStream = null;
        string? activeProxyId = null;
        string? activeRelayId = null;
        try
        {
            (upstreamClient, upstreamStream, activeProxyId, activeRelayId) = await ConnectToUpstreamAsync(
                targetHost, targetPort, tenant, targetCountry, targetCity, sessionId, sessionMinutes, ct);

            if (method == "CONNECT")
            {
                // Trả lời 200 Connection Established
                byte[] established = Encoding.UTF8.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n");
                await stream.WriteAsync(established, ct);
                await stream.FlushAsync(ct);
            }
            else
            {
                // Forward lại HTTP Request ban đầu cho upstream
                byte[] initialReq = ms.ToArray();
                await upstreamStream.WriteAsync(initialReq, ct);
                await upstreamStream.FlushAsync(ct);
            }

            var sessId = Guid.NewGuid().ToString("N")[..8];
            var session = new ProxySession
            {
                SessionId = sessId,
                TenantId = tenant?.Id ?? "anonymous",
                ClientIp = client.Client.RemoteEndPoint?.ToString() ?? "unknown",
                Node = assignedNode,
                TargetHost = targetHost,
                TargetPort = targetPort,
                ConnectedAt = DateTime.UtcNow
            };
            _activeSessions[sessId] = session;

            try
            {
                await BridgeStreamsWithAccountingAsync(stream, upstreamStream, tenant?.Id, assignedNode, activeProxyId, activeRelayId, ct);
            }
            finally
            {
                _activeSessions.TryRemove(sessId, out _);
            }
        }
        catch
        {
            byte[] errResp = Encoding.UTF8.GetBytes("HTTP/1.1 502 Bad Gateway\r\nContent-Length: 0\r\n\r\n");
            try { await stream.WriteAsync(errResp, ct); } catch { }
        }
        finally
        {
            upstreamClient?.Dispose();
        }
    }
    #endregion

    private async Task BridgeStreamsWithAccountingAsync(
        Stream clientStream, 
        Stream upstreamStream, 
        string? tenantId, 
        ResidentialNode node, 
        string? proxyId,
        string? relayId,
        CancellationToken ct)
    {
        long bytesIn = 0;
        long bytesOut = 0;
        using var bridgeCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var t1 = Task.Run(async () =>
        {
            try
            {
                byte[] buf = new byte[32768];
                while (!bridgeCts.IsCancellationRequested)
                {
                    int r = await clientStream.ReadAsync(buf.AsMemory(0, buf.Length), bridgeCts.Token);
                    if (r == 0) break;
                    await upstreamStream.WriteAsync(buf.AsMemory(0, r), bridgeCts.Token);
                    await upstreamStream.FlushAsync(bridgeCts.Token);
                    Interlocked.Add(ref bytesOut, r);
                    Interlocked.Add(ref _totalBytesOut, r);
                    Interlocked.Add(ref _totalBytesServed, r);
                }
            }
            catch { }
            finally
            {
                bridgeCts.Cancel();
            }
        }, ct);

        var t2 = Task.Run(async () =>
        {
            try
            {
                byte[] buf = new byte[32768];
                while (!bridgeCts.IsCancellationRequested)
                {
                    int r = await upstreamStream.ReadAsync(buf.AsMemory(0, buf.Length), bridgeCts.Token);
                    if (r == 0) break;
                    await clientStream.WriteAsync(buf.AsMemory(0, r), bridgeCts.Token);
                    await clientStream.FlushAsync(bridgeCts.Token);
                    Interlocked.Add(ref bytesIn, r);
                    Interlocked.Add(ref _totalBytesIn, r);
                    Interlocked.Add(ref _totalBytesServed, r);
                }
            }
            catch { }
            finally
            {
                bridgeCts.Cancel();
            }
        }, ct);

        await Task.WhenAll(t1, t2);

        long total = bytesIn + bytesOut;
        Interlocked.Add(ref node.TotalBytesServed, total);

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            _tenantService.RecordUsage(tenantId, total);
        }

        // [VI] Ghi nhận tiêu hao hạn mức 10GB/300MB cho con Proxy này
        // [EN] Record 10GB/300MB quota usage for this upstream proxy
        if (!string.IsNullOrWhiteSpace(proxyId))
        {
            _proxyManager.RecordBandwidthUsage(proxyId, total);
        }

        // [VI] Ghi nhận lưu lượng qua Relay VPS
        // [EN] Record relayed stream completion on Relay Node
        if (!string.IsNullOrWhiteSpace(relayId))
        {
            _relayPool.TrackStreamEnd(relayId, total);
        }
    }

    private async Task<(TcpClient client, Stream stream, string? proxyId, string? relayId)> ConnectToUpstreamAsync(
        string targetHost, 
        int targetPort,
        Tenant? tenant = null,
        string? targetCountry = null,
        string? targetCity = null,
        string? sessionId = null,
        int? sessionMinutes = null,
        CancellationToken ct = default)
    {
        // 1. Phân giải proxy: nếu có tenant/query thì dùng rotation engine; nếu là Desktop Client thì ưu tiên tuyệt đối ActiveVpnProxy
        UpstreamProxy? activeProxy = null;
        if (tenant != null || !string.IsNullOrWhiteSpace(targetCountry) || !string.IsNullOrWhiteSpace(sessionId))
        {
            activeProxy = _rotationEngine.ResolveProxy(tenant, targetCountry, targetCity, sessionId, sessionMinutes);
        }
        activeProxy ??= _proxyManager.GetActiveVpnProxy() ?? _rotationEngine.ResolveProxy(tenant, targetCountry, targetCity, sessionId, sessionMinutes);

        bool isExternalProxy = activeProxy != null
            && !string.IsNullOrWhiteSpace(activeProxy.Host)
            && !(activeProxy.Host == "127.0.0.1" && activeProxy.Port == 10000)
            && !(activeProxy.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) && activeProxy.Port == 10000);

        if (!isExternalProxy || activeProxy == null)
        {
            var tcpClient = new TcpClient { NoDelay = true };
            await tcpClient.ConnectAsync(targetHost, targetPort, ct);
            return (tcpClient, tcpClient.GetStream(), null, null);
        }

        // 2. Quyết định xem có đi qua dàn VPS Trung chuyển (Relay Shield) hay không
        bool isVipUser = tenant != null && (tenant.IsVip || !tenant.Id.Equals("free_tier", StringComparison.OrdinalIgnoreCase));
        RelayNode? relay = null;
        if (_relayPool.ShouldUseRelay(isVipUser, activeProxy.Country))
        {
            relay = _relayPool.ResolveRelayNode(activeProxy.Country);
            if (relay != null)
            {
                _relayPool.TrackStreamStart(relay.Id);
            }
        }

        // 3. Thử kết nối; nếu gặp sự cố -> Tự động Failover sang proxy LIVE cùng quốc gia hoặc quốc tế
        try
        {
            var (c, s) = await AttemptConnectToUpstreamProxyAsync(activeProxy, targetHost, targetPort, ct);
            return (c, s, activeProxy.Id, relay?.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Gateway Failover] Proxy {Host}:{Port} ({Country}) gặp sự cố ({Err}). Đang tự động chuyển sang node cứu hộ...",
                activeProxy.Host, activeProxy.Port, activeProxy.Country, ex.Message);

            _rotationEngine.ReportFailure(activeProxy.Id, $"{tenant?.Id}_{sessionId}");

            // Lấy danh sách các node LIVE khác trong kho
            var allLive = _proxyManager.GetAll()
                .Where(p => p.Id != activeProxy.Id && (p.Type == "socks5" || p.Type == "http") && !string.IsNullOrWhiteSpace(p.Host) && p.Host != "127.0.0.1" && !p.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                .ToList();

            UpstreamProxy? failoverProxy = null;
            bool isForeignTarget = !string.IsNullOrWhiteSpace(activeProxy.Country) && !activeProxy.Country.Equals("VN", StringComparison.OrdinalIgnoreCase);

            if (isForeignTarget)
            {
                failoverProxy = allLive.FirstOrDefault(p => p.Country.Equals(activeProxy.Country, StringComparison.OrdinalIgnoreCase))
                             ?? allLive.FirstOrDefault(p => !p.Country.Equals("VN", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                failoverProxy = allLive.FirstOrDefault(p => p.Country.Equals("VN", StringComparison.OrdinalIgnoreCase))
                             ?? allLive.FirstOrDefault();
            }

            if (failoverProxy != null)
            {
                _logger.LogInformation("[Gateway Failover] Đã kích hoạt node thay thế: {Host}:{Port} ({Country})",
                    failoverProxy.Host, failoverProxy.Port, failoverProxy.Country);
                var (fc, fs) = await AttemptConnectToUpstreamProxyAsync(failoverProxy, targetHost, targetPort, ct);
                return (fc, fs, failoverProxy.Id, relay?.Id);
            }

            throw;
        }
    }

    private async Task<(TcpClient client, Stream stream)> AttemptConnectToUpstreamProxyAsync(
        UpstreamProxy proxy, 
        string targetHost, 
        int targetPort, 
        CancellationToken ct)
    {
        // Nếu là node VLESS: kiểm tra nếu có local Xray SOCKS5 bridge (cổng 20808) thì chuyển tiếp
        if (proxy.Type.Equals("vless", StringComparison.OrdinalIgnoreCase))
        {
            var xrayProxy = new UpstreamProxy { Type = "socks5", Host = "127.0.0.1", Port = 20808, Country = proxy.Country };
            return await AttemptConnectToUpstreamProxyAsync(xrayProxy, targetHost, targetPort, ct);
        }

        var tcpClient = new TcpClient { NoDelay = true };
        using var connectCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, connectCts.Token);
        await tcpClient.ConnectAsync(proxy.Host, proxy.Port, linkedCts.Token);
        var proxyStream = tcpClient.GetStream();

        if (proxy.Type.Equals("socks5", StringComparison.OrdinalIgnoreCase))
        {
            bool hasAuth = !string.IsNullOrEmpty(proxy.Username);
            if (hasAuth)
            {
                await proxyStream.WriteAsync(new byte[] { 0x05, 0x01, 0x02 }, ct);
                byte[] authResp = new byte[2];
                await proxyStream.ReadExactlyAsync(authResp, ct);
                if (authResp[0] != 0x05 || authResp[1] != 0x02)
                    throw new IOException("Upstream SOCKS5 auth method rejected");

                byte[] uBytes = Encoding.UTF8.GetBytes(proxy.Username ?? "");
                byte[] pBytes = Encoding.UTF8.GetBytes(proxy.Password ?? "");
                byte[] authReq = new byte[3 + uBytes.Length + pBytes.Length];
                authReq[0] = 0x01;
                authReq[1] = (byte)uBytes.Length;
                Buffer.BlockCopy(uBytes, 0, authReq, 2, uBytes.Length);
                authReq[2 + uBytes.Length] = (byte)pBytes.Length;
                Buffer.BlockCopy(pBytes, 0, authReq, 3 + uBytes.Length, pBytes.Length);
                await proxyStream.WriteAsync(authReq, ct);

                byte[] authStatus = new byte[2];
                await proxyStream.ReadExactlyAsync(authStatus, ct);
                if (authStatus[1] != 0x00)
                    throw new IOException("Upstream SOCKS5 authentication failed");
            }
            else
            {
                await proxyStream.WriteAsync(new byte[] { 0x05, 0x01, 0x00 }, ct);
                byte[] noAuthResp = new byte[2];
                await proxyStream.ReadExactlyAsync(noAuthResp, ct);
                if (noAuthResp[0] != 0x05 || noAuthResp[1] != 0x00)
                    throw new IOException("Upstream SOCKS5 handshake failed");
            }

            // SOCKS5 CONNECT tới targetHost:targetPort
            byte[] targetBytes = Encoding.ASCII.GetBytes(targetHost);
            byte[] cmd = new byte[4 + 1 + targetBytes.Length + 2];
            cmd[0] = 0x05;
            cmd[1] = 0x01; // CONNECT
            cmd[2] = 0x00; // RSV
            cmd[3] = 0x03; // DOMAIN
            cmd[4] = (byte)targetBytes.Length;
            Buffer.BlockCopy(targetBytes, 0, cmd, 5, targetBytes.Length);
            cmd[5 + targetBytes.Length] = (byte)(targetPort >> 8);
            cmd[6 + targetBytes.Length] = (byte)(targetPort & 0xFF);
            await proxyStream.WriteAsync(cmd, ct);

            byte[] cmdResp = new byte[4];
            await proxyStream.ReadExactlyAsync(cmdResp, ct);
            if (cmdResp[1] != 0x00)
                throw new IOException($"Upstream SOCKS5 connect failed with status 0x{cmdResp[1]:X2}");

            if (cmdResp[3] == 0x01) { byte[] b = new byte[6]; await proxyStream.ReadExactlyAsync(b, ct); }
            else if (cmdResp[3] == 0x03) 
            { 
                byte[] lenBuf = new byte[1];
                await proxyStream.ReadExactlyAsync(lenBuf, ct);
                byte[] b = new byte[lenBuf[0] + 2]; 
                await proxyStream.ReadExactlyAsync(b, ct); 
            }
            else if (cmdResp[3] == 0x04) { byte[] b = new byte[18]; await proxyStream.ReadExactlyAsync(b, ct); }
        }
        else
        {
            // HTTP CONNECT
            var connectReq = new StringBuilder();
            connectReq.Append($"CONNECT {targetHost}:{targetPort} HTTP/1.1\r\n");
            connectReq.Append($"Host: {targetHost}:{targetPort}\r\n");
            if (!string.IsNullOrEmpty(proxy.Username))
            {
                string cred = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{proxy.Username}:{proxy.Password}"));
                connectReq.Append($"Proxy-Authorization: Basic {cred}\r\n");
            }
            connectReq.Append("User-Agent: NextAiVPN/2.1\r\n\r\n");
            byte[] reqBytes = Encoding.ASCII.GetBytes(connectReq.ToString());
            await proxyStream.WriteAsync(reqBytes, ct);

            var respSb = new StringBuilder();
            byte[] single = new byte[1];
            while (true)
            {
                int r = await proxyStream.ReadAsync(single.AsMemory(0, 1), ct);
                if (r <= 0) break;
                respSb.Append((char)single[0]);
                if (respSb.ToString().EndsWith("\r\n\r\n")) break;
            }

            string respStr = respSb.ToString();
            if (!respStr.Contains("200"))
                throw new IOException($"Upstream HTTP CONNECT failed: {respStr.Split("\r\n").FirstOrDefault()}");
        }

        return (tcpClient, proxyStream);
    }
}

