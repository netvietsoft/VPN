using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using VpnResidentialHub.Models;

namespace VpnResidentialHub.Services;

/// <summary>
/// [VI] Giao diện dịch vụ cấp phát và quản trị cổng SOCKS5 riêng biệt (Dedicated Ports 10001-10500)
/// [EN] Interface for allocating and managing dedicated client proxy ports (10001-10500)
/// </summary>
public interface IDedicatedPortService
{
    /// <summary>
    /// [VI] Cấp phát một cổng riêng cố định cho khách hàng theo quốc gia/thành phố
    /// [EN] Allocates a fixed dedicated port for tenant targeting country/city
    /// </summary>
    int AllocateDedicatedPort(string tenantId, string? country, string? city);

    /// <summary>
    /// [VI] Giải phóng cổng riêng đã cấp phát
    /// [EN] Releases and stops listener for a dedicated port
    /// </summary>
    bool ReleaseDedicatedPort(int port);

    /// <summary>
    /// [VI] Lấy danh sách toàn bộ các cổng riêng đang hoạt động
    /// [EN] Retrieves list of all currently active dedicated ports
    /// </summary>
    IReadOnlyList<DedicatedPortInfo> GetActivePorts();
}

public class DedicatedPortInfo
{
    public int Port { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public ResidentialNode Node { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DedicatedPortService : IDedicatedPortService
{
    private readonly INodePoolService _nodePool;
    private readonly ITenantService _tenantService;
    private readonly ILogger<DedicatedPortService> _logger;
    private readonly ConcurrentDictionary<int, (DedicatedPortInfo Info, TcpListener Listener, CancellationTokenSource Cts)> _activePorts = new();
    private int _portCounter = 10000;
    private readonly object _lock = new();

    public DedicatedPortService(
        INodePoolService nodePool,
        ITenantService tenantService,
        ILogger<DedicatedPortService> logger)
    {
        _nodePool = nodePool;
        _tenantService = tenantService;
        _logger = logger;
    }

    public int AllocateDedicatedPort(string tenantId, string? country, string? city)
    {
        var node = _nodePool.GetBestNode(country, city);
        int port = GetNextPort();

        var cts = new CancellationTokenSource();
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start(50);

        var info = new DedicatedPortInfo
        {
            Port = port,
            TenantId = tenantId,
            Node = node,
            CreatedAt = DateTime.UtcNow
        };

        _activePorts[port] = (info, listener, cts);
        _logger.LogInformation("[DedicatedPort] Đã mở cổng riêng {Port} cho Tenant '{TenantId}' -> IP: {NodeIp} ({Country})",
            port, tenantId, node.Ip, node.Country);

        _ = AcceptLoopAsync(listener, info, cts.Token);
        return port;
    }

    public bool ReleaseDedicatedPort(int port)
    {
        if (_activePorts.TryRemove(port, out var tuple))
        {
            tuple.Cts.Cancel();
            tuple.Listener.Stop();
            tuple.Cts.Dispose();
            _logger.LogInformation("[DedicatedPort] Đã đóng cổng riêng {Port}", port);
            return true;
        }
        return false;
    }

    public IReadOnlyList<DedicatedPortInfo> GetActivePorts() => _activePorts.Values.Select(v => v.Info).ToList();

    private int GetNextPort()
    {
        lock (_lock)
        {
            for (int i = 0; i < 500; i++)
            {
                _portCounter++;
                if (_portCounter > 10500) _portCounter = 10001;

                if (!_activePorts.ContainsKey(_portCounter))
                {
                    return _portCounter;
                }
            }
            throw new InvalidOperationException("Hết cổng dedicated trống!");
        }
    }

    private async Task AcceptLoopAsync(TcpListener listener, DedicatedPortInfo info, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync(ct);
                _ = HandleConnectionAsync(client, info, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (ObjectDisposedException) { break; }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[DedicatedPort] Lỗi accept connection");
            }
        }
    }

    private async Task HandleConnectionAsync(TcpClient client, DedicatedPortInfo info, CancellationToken ct)
    {
        using (client)
        {
            try
            {
                using var stream = client.GetStream();

                // SOCKS5 Handshake đơn giản không cần xác thực cho cổng riêng
                byte[] buf = new byte[257];
                await stream.ReadExactlyAsync(buf.AsMemory(0, 2), ct);
                if (buf[0] != 0x05) return;

                int nMethods = buf[1];
                await stream.ReadExactlyAsync(buf.AsMemory(0, nMethods), ct);

                // Trả lời 0x05, 0x00 (NO AUTH)
                await stream.WriteAsync(new byte[] { 0x05, 0x00 }, ct);

                // Đọc request CONNECT
                byte[] reqHeader = new byte[4];
                await stream.ReadExactlyAsync(reqHeader, ct);
                if (reqHeader[0] != 0x05 || reqHeader[1] != 0x01) return;

                byte atyp = reqHeader[3];
                string targetHost = string.Empty;

                if (atyp == 0x01)
                {
                    byte[] ipBytes = new byte[4];
                    await stream.ReadExactlyAsync(ipBytes, ct);
                    targetHost = new IPAddress(ipBytes).ToString();
                }
                else if (atyp == 0x03)
                {
                    int len = await ReadByteAsync(stream, ct);
                    if (len <= 0) return;
                    byte[] dBytes = new byte[len];
                    await stream.ReadExactlyAsync(dBytes, ct);
                    targetHost = Encoding.ASCII.GetString(dBytes);
                }

                byte[] pBytes = new byte[2];
                await stream.ReadExactlyAsync(pBytes, ct);
                int targetPort = (pBytes[0] << 8) | pBytes[1];

                using var upstream = new TcpClient();
                await upstream.ConnectAsync(targetHost, targetPort, ct);

                byte[] reply = new byte[] { 0x05, 0x00, 0x00, 0x01, 127, 0, 0, 1, (byte)(targetPort >> 8), (byte)(targetPort & 0xFF) };
                await stream.WriteAsync(reply, ct);

                var t1 = stream.CopyToAsync(upstream.GetStream(), ct);
                var t2 = upstream.GetStream().CopyToAsync(stream, ct);
                await Task.WhenAny(t1, t2);
            }
            catch { }
        }
    }

    private static async ValueTask<int> ReadByteAsync(Stream stream, CancellationToken ct)
    {
        byte[] b = new byte[1];
        int read = await stream.ReadAsync(b.AsMemory(0, 1), ct);
        return read == 0 ? -1 : b[0];
    }
}
