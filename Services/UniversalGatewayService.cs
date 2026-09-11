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
}

public class UniversalGatewayService : IUniversalGatewayService
{
    private readonly INodePoolService _nodePool;
    private readonly ITenantService _tenantService;
    private readonly ILogger<UniversalGatewayService> _logger;
    private readonly ConcurrentDictionary<string, ProxySession> _activeSessions = new();
    private TcpListener? _listener;
    private long _totalBytesServed = 0;

    public int ActiveConnectionsCount => _activeSessions.Count;
    public long TotalBytesServed => _totalBytesServed;

    public UniversalGatewayService(
        INodePoolService nodePool,
        ITenantService tenantService,
        ILogger<UniversalGatewayService> logger)
    {
        _nodePool = nodePool;
        _tenantService = tenantService;
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
        var assignedNode = _nodePool.GetBestNode(targetCountry, targetCity);

        // 4. Kết nối ra ngoài Internet
        using var upstreamClient = new TcpClient();
        try
        {
            await upstreamClient.ConnectAsync(targetHost, targetPort, ct);

            // Báo thành công SOCKS5
            byte[] reply = new byte[] {
                0x05, 0x00, 0x00, 0x01,
                127, 0, 0, 1,
                (byte)(targetPort >> 8), (byte)(targetPort & 0xFF)
            };
            await stream.WriteAsync(reply, ct);

            using var upstreamStream = upstreamClient.GetStream();

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
                await BridgeStreamsWithAccountingAsync(stream, upstreamStream, tenant?.Id, assignedNode, ct);
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

        var assignedNode = _nodePool.GetBestNode(targetCountry, targetCity);

        using var upstreamClient = new TcpClient();
        try
        {
            await upstreamClient.ConnectAsync(targetHost, targetPort, ct);

            if (method == "CONNECT")
            {
                // Trả lời 200 Connection Established
                byte[] established = Encoding.UTF8.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n");
                await stream.WriteAsync(established, ct);
            }
            else
            {
                // Forward lại HTTP Request ban đầu cho upstream
                byte[] initialReq = ms.ToArray();
                await upstreamClient.GetStream().WriteAsync(initialReq, ct);
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
                await BridgeStreamsWithAccountingAsync(stream, upstreamClient.GetStream(), tenant?.Id, assignedNode, ct);
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
    }
    #endregion

    private async Task BridgeStreamsWithAccountingAsync(
        Stream clientStream, 
        Stream upstreamStream, 
        string? tenantId, 
        ResidentialNode node, 
        CancellationToken ct)
    {
        long bytesIn = 0;
        long bytesOut = 0;

        var t1 = Task.Run(async () =>
        {
            byte[] buf = new byte[16384];
            while (!ct.IsCancellationRequested)
            {
                int r = await clientStream.ReadAsync(buf.AsMemory(0, buf.Length), ct);
                if (r == 0) break;
                await upstreamStream.WriteAsync(buf.AsMemory(0, r), ct);
                Interlocked.Add(ref bytesOut, r);
            }
        }, ct);

        var t2 = Task.Run(async () =>
        {
            byte[] buf = new byte[16384];
            while (!ct.IsCancellationRequested)
            {
                int r = await upstreamStream.ReadAsync(buf.AsMemory(0, buf.Length), ct);
                if (r == 0) break;
                await clientStream.WriteAsync(buf.AsMemory(0, r), ct);
                Interlocked.Add(ref bytesIn, r);
            }
        }, ct);

        await Task.WhenAny(t1, t2);

        long total = bytesIn + bytesOut;
        Interlocked.Add(ref _totalBytesServed, total);
        Interlocked.Add(ref node.TotalBytesServed, total);

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            _tenantService.RecordUsage(tenantId, total);
        }
    }
}
