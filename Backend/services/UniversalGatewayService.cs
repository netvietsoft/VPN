using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VpnResidentialHub.Models;

namespace VpnResidentialHub.Services;

/// <summary>
/// [VI] Giao diện cổng Universal Proxy Gateway (SOCKS5 & HTTP CONNECT trên cổng 10000)
/// [EN] Interface for the Universal Proxy Gateway (supporting SOCKS5 & HTTP CONNECT on port 10000)
/// </summary>
public interface IUniversalGatewayService
{
    /// <summary>
    /// [VI] Khởi động Gateway lắng nghe trên cổng chỉ định
    /// [EN] Starts the Gateway listening on the specified port
    /// </summary>
    void Start(int port, CancellationToken ct);

    /// <summary>
    /// [VI] Dừng Gateway và đóng toàn bộ listener
    /// [EN] Stops the Gateway and releases all socket listeners
    /// </summary>
    void Stop();

    /// <summary>
    /// [VI] Lấy danh sách các phiên proxy đang hoạt động
    /// [EN] Retrieves list of active proxy sessions
    /// </summary>
    IReadOnlyList<ProxySession> GetActiveSessions();

    /// <summary>
    /// [VI] Số lượng kết nối đồng thời đang xử lý
    /// [EN] Current number of concurrent active connections
    /// </summary>
    int ActiveConnectionsCount { get; }

    /// <summary>
    /// [VI] Tổng lưu lượng đã phục vụ tính bằng Bytes
    /// [EN] Total bytes served across all sessions
    /// </summary>
    long TotalBytesServed { get; }

    /// <summary>
    /// [VI] Tổng lưu lượng tải lên (Inbound) tính bằng Bytes
    /// [EN] Total incoming bytes received
    /// </summary>
    long TotalBytesIn { get; }

    /// <summary>
    /// [VI] Tổng lưu lượng tải xuống (Outbound) tính bằng Bytes
    /// [EN] Total outgoing bytes sent
    /// </summary>
    long TotalBytesOut { get; }
}

/// <summary>
/// [VI] Dịch vụ Universal Proxy Gateway hỗ trợ cả SOCKS5 và HTTP Proxy với tính năng xoay IP dân cư tự động
/// [EN] Universal Proxy Gateway service supporting SOCKS5 & HTTP CONNECT with automated residential IP rotation
/// </summary>
public class UniversalGatewayService : IUniversalGatewayService
{
    private readonly INodePoolService _nodePool;
    private readonly ITenantService _tenantService;
    private readonly IProxyManagerService _proxyManager;
    private readonly ISmartProxyRotationEngine _rotationEngine;
    private readonly IRelayPoolManagerService _relayPool;
    private readonly ILogger<UniversalGatewayService> _logger;
    private readonly ConcurrentDictionary<string, ProxySession> _activeSessions = new();
    private readonly ConcurrentDictionary<string, (int FailCount, DateTime LockoutUntil)> _authFailures = new();
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

    #region Helper Methods (Async I/O, Constant-Time Auth & Rate Limiting)
    /// <summary>
    /// [VI] Đọc bất đồng bộ 1 byte từ luồng socket kèm hỗ trợ CancellationToken
    /// [EN] Asynchronously reads 1 byte from stream with CancellationToken support
    /// </summary>
    private static async ValueTask<int> ReadByteAsync(Stream stream, CancellationToken ct)
    {
        byte[] b = new byte[1];
        int read = await stream.ReadAsync(b.AsMemory(0, 1), ct);
        return read == 0 ? -1 : b[0];
    }

    /// <summary>
    /// [VI] So sánh mật khẩu an toàn theo thời gian cố định (Constant-Time) chống tấn công Timing
    /// [EN] Constant-time password comparison to prevent timing side-channel attacks
    /// </summary>
    private static bool CheckPasswordConstantTime(Tenant? tenant, string password)
    {
        if (tenant == null || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tenant.Password))
            return false;

        byte[] expected = Encoding.UTF8.GetBytes(tenant.Password);
        byte[] actual = Encoding.UTF8.GetBytes(password);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    /// <summary>
    /// [VI] Kiểm tra địa chỉ IP client có đang bị khóa tạm thời do nhập sai mật khẩu nhiều lần
    /// [EN] Checks if client IP is temporarily locked out due to repeated authentication failures
    /// </summary>
    private bool IsClientLockedOut(string clientIp)
    {
        if (_authFailures.TryGetValue(clientIp, out var state))
        {
            if (DateTime.UtcNow < state.LockoutUntil) return true;
            if (DateTime.UtcNow > state.LockoutUntil && state.FailCount >= 10)
            {
                _authFailures.TryRemove(clientIp, out _);
            }
        }
        return false;
    }

    private void RecordAuthFailure(string clientIp)
    {
        _authFailures.AddOrUpdate(clientIp,
            (1, DateTime.UtcNow.AddMinutes(1)),
            (_, old) =>
            {
                int newCount = old.FailCount + 1;
                var lockout = newCount >= 10 ? DateTime.UtcNow.AddMinutes(5) : DateTime.UtcNow.AddMinutes(1);
                return (newCount, lockout);
            });
    }

    private void RecordAuthSuccess(string clientIp)
    {
        _authFailures.TryRemove(clientIp, out _);
    }
    #endregion

    #region SOCKS5 Protocol Handler (RFC 1928 & RFC 1929)
    private async Task HandleSocks5Async(TcpClient client, NetworkStream stream, CancellationToken ct)
    {
        string clientIp = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        if (IsClientLockedOut(clientIp))
        {
            _logger.LogWarning("[SOCKS5] Từ chối kết nối do IP {Ip} đang bị khóa tạm thời vì thử sai mật khẩu.", clientIp);
            return;
        }

        // 1. Bắt tay lựa chọn phương thức xác thực
        int nMethods = await ReadByteAsync(stream, ct);
        if (nMethods <= 0) return;

        byte[] methods = new byte[nMethods];
        await stream.ReadExactlyAsync(methods, ct);

        bool supportsAuth = methods.Contains((byte)0x02);
        await stream.WriteAsync(supportsAuth ? new byte[] { 0x05, 0x02 } : new byte[] { 0x05, 0x00 }, ct);

        Tenant? tenant = null;
        string? targetCountry = null;
        string? targetCity = null;
        string? sessionId = null;
        int? sessionMinutes = null;
        string? targetNodeId = null;

        if (supportsAuth)
        {
            var authResult = await AuthenticateSocks5Async(stream, clientIp, ct);
            if (!authResult.Success) return;

            tenant = authResult.Tenant;
            targetCountry = authResult.Country;
            targetCity = authResult.City;
            sessionId = authResult.SessionId;
            sessionMinutes = authResult.SessionMinutes;
            targetNodeId = authResult.TargetNodeId;
        }

        // 2. Đọc Client Request (CMD, DST.ADDR, DST.PORT)
        var targetInfo = await ParseSocks5TargetAsync(stream, ct);
        if (!targetInfo.Success) return;

        // 3. Chọn Residential Node phù hợp
        var assignedNode = ResolveAssignedNode(targetNodeId, targetCountry, targetCity);

        // 4. Kết nối ra ngoài Internet
        TcpClient? upstreamClient = null;
        Stream? upstreamStream = null;
        string? activeProxyId = null;
        string? activeRelayId = null;
        try
        {
            (upstreamClient, upstreamStream, activeProxyId, activeRelayId) = await ConnectToUpstreamAsync(
                targetInfo.Host, targetInfo.Port, tenant, targetCountry, targetCity, sessionId, sessionMinutes, ct);

            // Báo thành công SOCKS5
            byte[] reply = new byte[] {
                0x05, 0x00, 0x00, 0x01,
                127, 0, 0, 1,
                (byte)(targetInfo.Port >> 8), (byte)(targetInfo.Port & 0xFF)
            };
            await stream.WriteAsync(reply, ct);

            // 5. Cầu nối chuyển tiếp & thống kê băng thông
            var sessId = Guid.NewGuid().ToString("N")[..8];
            var session = new ProxySession
            {
                SessionId = sessId,
                TenantId = tenant?.Id ?? "anonymous",
                ClientIp = clientIp,
                Node = assignedNode,
                TargetHost = targetInfo.Host,
                TargetPort = targetInfo.Port,
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

    private async Task<(bool Success, Tenant? Tenant, string? Country, string? City, string? SessionId, int? SessionMinutes, string? TargetNodeId)>
        AuthenticateSocks5Async(NetworkStream stream, string clientIp, CancellationToken ct)
    {
        int authVersion = await ReadByteAsync(stream, ct);
        if (authVersion != 0x01) return (false, null, null, null, null, null, null);

        int uLen = await ReadByteAsync(stream, ct);
        if (uLen <= 0) return (false, null, null, null, null, null, null);
        byte[] uBytes = new byte[uLen];
        await stream.ReadExactlyAsync(uBytes, ct);
        string fullUsername = Encoding.UTF8.GetString(uBytes);

        int pLen = await ReadByteAsync(stream, ct);
        if (pLen < 0) return (false, null, null, null, null, null, null);
        byte[] pBytes = new byte[pLen];
        if (pLen > 0) await stream.ReadExactlyAsync(pBytes, ct);
        string password = Encoding.UTF8.GetString(pBytes);

        var parseRes = _tenantService.ParseProxyUsername(fullUsername);
        var tenant = parseRes.Tenant;

        if (tenant == null || !CheckPasswordConstantTime(tenant, password))
        {
            RecordAuthFailure(clientIp);
            await stream.WriteAsync(new byte[] { 0x01, 0x01 }, ct);
            _logger.LogWarning("[SOCKS5] Xác thực thất bại cho username '{User}' từ IP {Ip}", fullUsername, clientIp);
            return (false, null, null, null, null, null, null);
        }

        RecordAuthSuccess(clientIp);
        await stream.WriteAsync(new byte[] { 0x01, 0x00 }, ct);
        return (true, tenant, parseRes.Country, parseRes.City, parseRes.SessionId, parseRes.SessionMinutes, parseRes.TargetNodeId);
    }

    private static async Task<(bool Success, string Host, int Port)> ParseSocks5TargetAsync(NetworkStream stream, CancellationToken ct)
    {
        byte[] reqHeader = new byte[4];
        await stream.ReadExactlyAsync(reqHeader, ct);
        if (reqHeader[0] != 0x05 || reqHeader[1] != 0x01) return (false, string.Empty, 0);

        byte atyp = reqHeader[3];
        string targetHost = string.Empty;

        if (atyp == 0x01) // IPv4
        {
            byte[] ipBytes = new byte[4];
            await stream.ReadExactlyAsync(ipBytes, ct);
            targetHost = new IPAddress(ipBytes).ToString();
        }
        else if (atyp == 0x03) // Domain
        {
            int domainLen = await ReadByteAsync(stream, ct);
            if (domainLen <= 0) return (false, string.Empty, 0);
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
        int targetPort = (portBytes[0] << 8) | portBytes[1];

        return (true, targetHost, targetPort);
    }
    #endregion

    #region HTTP Proxy / CONNECT Protocol Handler
    private async Task HandleHttpProxyAsync(TcpClient client, NetworkStream stream, byte firstByte, CancellationToken ct)
    {
        string clientIp = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        if (IsClientLockedOut(clientIp))
        {
            _logger.LogWarning("[HTTP Proxy] Từ chối kết nối do IP {Ip} đang bị khóa tạm thời vì thử sai mật khẩu.", clientIp);
            return;
        }

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

        string requestLine = lines[0];
        var reqParts = requestLine.Split(' ');
        if (reqParts.Length < 2) return;

        string method = reqParts[0].ToUpperInvariant();
        string target = reqParts[1];

        var authResult = AuthenticateHttp(lines, clientIp);
        if (!authResult.Success)
        {
            byte[] authFail = Encoding.UTF8.GetBytes("HTTP/1.1 407 Proxy Authentication Required\r\nProxy-Authenticate: Basic realm=\"NextAI Residential Gateway\"\r\nContent-Length: 0\r\n\r\n");
            await stream.WriteAsync(authFail, ct);
            return;
        }

        var (targetHost, targetPort) = ParseHttpTarget(method, target);
        if (string.IsNullOrEmpty(targetHost) || targetPort == 0) return;

        var assignedNode = ResolveAssignedNode(authResult.TargetNodeId, authResult.Country, authResult.City);

        TcpClient? upstreamClient = null;
        Stream? upstreamStream = null;
        string? activeProxyId = null;
        string? activeRelayId = null;
        try
        {
            (upstreamClient, upstreamStream, activeProxyId, activeRelayId) = await ConnectToUpstreamAsync(
                targetHost, targetPort, authResult.Tenant, authResult.Country, authResult.City, authResult.SessionId, authResult.SessionMinutes, ct);

            if (method == "CONNECT")
            {
                byte[] established = Encoding.UTF8.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n");
                await stream.WriteAsync(established, ct);
                await stream.FlushAsync(ct);
            }
            else
            {
                byte[] initialReq = ms.ToArray();
                await upstreamStream.WriteAsync(initialReq, ct);
                await upstreamStream.FlushAsync(ct);
            }

            var sessId = Guid.NewGuid().ToString("N")[..8];
            var session = new ProxySession
            {
                SessionId = sessId,
                TenantId = authResult.Tenant?.Id ?? "anonymous",
                ClientIp = clientIp,
                Node = assignedNode,
                TargetHost = targetHost,
                TargetPort = targetPort,
                ConnectedAt = DateTime.UtcNow
            };
            _activeSessions[sessId] = session;

            try
            {
                await BridgeStreamsWithAccountingAsync(stream, upstreamStream, authResult.Tenant?.Id, assignedNode, activeProxyId, activeRelayId, ct);
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

    private (bool Success, Tenant? Tenant, string? Country, string? City, string? SessionId, int? SessionMinutes, string? TargetNodeId)
        AuthenticateHttp(string[] lines, string clientIp)
    {
        var authHeader = lines.FirstOrDefault(l => l.StartsWith("Proxy-Authorization:", StringComparison.OrdinalIgnoreCase));
        if (authHeader == null)
        {
            // Môi trường nội bộ hoặc chưa truyền header
            return (true, null, null, null, null, null, null);
        }

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
                    var tenant = pRes.Tenant;

                    if (tenant == null || !CheckPasswordConstantTime(tenant, pass))
                    {
                        RecordAuthFailure(clientIp);
                        return (false, null, null, null, null, null, null);
                    }

                    RecordAuthSuccess(clientIp);
                    return (true, tenant, pRes.Country, pRes.City, pRes.SessionId, pRes.SessionMinutes, pRes.TargetNodeId);
                }
            }
            catch { }
        }

        return (false, null, null, null, null, null, null);
    }

    private static (string Host, int Port) ParseHttpTarget(string method, string target)
    {
        if (method == "CONNECT")
        {
            var hostPort = target.Split(':');
            string host = hostPort[0];
            int port = hostPort.Length > 1 && int.TryParse(hostPort[1], out var p) ? p : 443;
            return (host, port);
        }

        if (Uri.TryCreate(target, UriKind.Absolute, out var uri))
        {
            return (uri.Host, uri.Port);
        }

        return (string.Empty, 0);
    }

    private ResidentialNode ResolveAssignedNode(string? targetNodeId, string? targetCountry, string? targetCity)
    {
        if (!string.IsNullOrWhiteSpace(targetNodeId))
        {
            var clientNode = _nodePool.GetClientNodes().FirstOrDefault(n => n.DeviceId.Equals(targetNodeId, StringComparison.OrdinalIgnoreCase) && n.IsOnline);
            if (clientNode != null)
            {
                return new ResidentialNode
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
        return _nodePool.GetBestNode(targetCountry, targetCity);
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

        // [VI] Ghi nhận tiêu hao hạn mức 10GB/300MB cho con Proxy này và giảm bộ đếm kết nối đang phục vụ
        // [EN] Record 10GB/300MB quota usage and decrement active connection count for load balancing
        if (!string.IsNullOrWhiteSpace(proxyId))
        {
            _proxyManager.RecordBandwidthUsage(proxyId, total);
            _rotationEngine.ReportSuccess(proxyId);
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
        Tenant? tenant,
        string? targetCountry,
        string? targetCity,
        string? sessionId,
        int? sessionMinutes,
        CancellationToken ct)
    {
        // 1. Dùng Smart SaaS Rotation Engine để chọn Proxy tối ưu nhất theo Quốc gia, Session, Ping và Tải
        var activeProxy = _rotationEngine.ResolveProxy(tenant, targetCountry, targetCity, sessionId, sessionMinutes);

        // Fallback: nếu rotation engine không tìm thấy, lấy active VPN node
        activeProxy ??= _proxyManager.GetActiveVpnProxy();

        bool isExternalProxy = activeProxy != null && 
            !string.IsNullOrWhiteSpace(activeProxy.Host) && 
            activeProxy.Host != "127.0.0.1" && 
            !activeProxy.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);

        // Nhánh Direct Bypass giữ nguyên theo yêu cầu không sửa BUG-02
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
                try
                {
                    var (fc, fs) = await AttemptConnectToUpstreamProxyAsync(failoverProxy, targetHost, targetPort, ct);
                    return (fc, fs, failoverProxy.Id, relay?.Id);
                }
                catch
                {
                    // [VI] BUG-15: Giải phóng slot relay nếu kết nối failover cũng thất bại
                    // [EN] BUG-15: Release relay slot if failover connection also fails
                    if (relay != null)
                    {
                        _relayPool.TrackStreamEnd(relay.Id, 0);
                    }
                    throw;
                }
            }

            // [VI] BUG-15: Giải phóng slot relay nếu không có failover proxy
            // [EN] BUG-15: Release relay slot if no failover proxy is available
            if (relay != null)
            {
                _relayPool.TrackStreamEnd(relay.Id, 0);
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
        try
        {
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
        catch
        {
            // [VI] BUG-09: Đóng socket descriptor ngay khi bắt tay upstream thất bại, chống rò rỉ file handle
            // [EN] BUG-09: Close socket descriptor immediately on upstream handshake failure to prevent handle leak
            tcpClient.Dispose();
            throw;
        }
    }
}
