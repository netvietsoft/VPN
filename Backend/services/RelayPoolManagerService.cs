using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VpnResidentialHub.Services;

/// <summary>
/// [VI] Chế độ chuyển mạch trung chuyển (Relay Routing Mode)
/// [EN] Relay Routing Modes: Full Relay (100%), Semi-Relay (Hybrid/Partial), Direct Bypass
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RelayRoutingMode
{
    /// <summary>
    /// [VI] 100% lưu lượng bắt buộc đi qua dàn VPS Trung chuyển (Ẩn danh tối đa, giấu IP Core tuyệt đối)
    /// [EN] 100% traffic routed through Relay VPS fleet (Maximum anonymity, zero core IP exposure)
    /// </summary>
    FullRelay = 0,

    /// <summary>
    /// [VI] Bán phần / Cân bằng tải: VIP đi qua Relay, Free đi Direct hoặc chia 50-50 để giảm tải khi VPS bận
    /// [EN] Semi-Relay / Hybrid: VIP traffic goes through Relay, Free goes direct or 50-50 to balance load
    /// </summary>
    SemiRelay = 1,

    /// <summary>
    /// [VI] Bỏ qua Relay hoàn toàn (Core nối thẳng tới Proxy ngoài khi toàn bộ dàn VPS quá tải hoặc gặp sự cố)
    /// [EN] Direct Bypass: Core directly connects to Upstream Proxy (when relays are overloaded or offline)
    /// </summary>
    DirectBypass = 2
}

/// <summary>
/// [VI] Thực thể Máy chủ VPS Trung chuyển (Relay Node Entity)
/// [EN] Intermediate Relay VPS Node Entity
/// </summary>
public class RelayNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "Relay-Singapore-01";
    public string Ip { get; set; } = "103.150.12.88";
    public int Port { get; set; } = 10000;
    public string Country { get; set; } = "SG";
    public string CountryName { get; set; } = "Singapore";
    public string Region { get; set; } = "Asia";
    public int Weight { get; set; } = 100; // Trọng số chia tải (1-100)
    public int ActiveStreams;
    public int MaxStreams { get; set; } = 5000; // Ngưỡng quá tải
    public int? PingMs { get; set; } = 22;
    public string Status { get; set; } = "ONLINE"; // "ONLINE" | "DEGRADED" | "OFFLINE"
    public bool IsEnabled { get; set; } = true;
    public long TotalBytesRelayed;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastCheckedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// [VI] Cấu trúc dữ liệu lưu trữ dàn Relay xuống đĩa
/// [EN] Persistent storage structure for Relay configuration
/// </summary>
public class RelayStorageState
{
    public RelayRoutingMode ActiveMode { get; set; } = RelayRoutingMode.SemiRelay;
    public List<RelayNode> Relays { get; set; } = new();
}

/// <summary>
/// [VI] Giao diện dịch vụ quản trị dàn VPS Trung chuyển (Relay Pool Manager)
/// [EN] Interface for managing intermediate Relay VPS nodes and routing policies
/// </summary>
public interface IRelayPoolManagerService
{
    /// <summary>
    /// [VI] Chế độ định tuyến chuyển tiếp hiện tại của hệ thống
    /// [EN] Current relay routing mode of the system
    /// </summary>
    RelayRoutingMode ActiveMode { get; set; }

    /// <summary>
    /// [VI] Lấy danh sách toàn bộ các VPS Trung chuyển
    /// [EN] Retrieves all relay VPS nodes
    /// </summary>
    IReadOnlyList<RelayNode> GetAllRelays();

    /// <summary>
    /// [VI] Lấy thông tin relay theo mã Id
    /// [EN] Retrieves relay node by Id
    /// </summary>
    RelayNode? GetById(string id);

    /// <summary>
    /// [VI] Thêm mới một VPS Trung chuyển vào cụm
    /// [EN] Adds a new relay node to the fleet
    /// </summary>
    RelayNode AddRelay(RelayNode node);

    /// <summary>
    /// [VI] Cập nhật thông số trọng số, trạng thái kích hoạt và ngưỡng tải
    /// [EN] Updates relay weight, enabled status, and stream limit
    /// </summary>
    bool UpdateRelay(string id, int weight, bool isEnabled, int maxStreams);

    /// <summary>
    /// [VI] Xóa VPS Trung chuyển khỏi danh sách
    /// [EN] Deletes a relay node from the fleet
    /// </summary>
    bool DeleteRelay(string id);
    
    /// <summary>
    /// [VI] Quyết định xem kết nối này có cần đi qua Relay hay không theo chế độ chuyển mạch và loại tài khoản
    /// [EN] Determines if the connection should be routed through a relay based on mode and tier
    /// </summary>
    bool ShouldUseRelay(bool isVipUser, string? targetCountry);

    /// <summary>
    /// [VI] Chọn VPS Trung chuyển tối ưu theo Trọng số (Weighted Round-Robin) và Vùng địa lý
    /// [EN] Selects optimal Relay Node based on weights and geo-proximity
    /// </summary>
    RelayNode? ResolveRelayNode(string? targetCountry);

    /// <summary>
    /// [VI] Ghi nhận bắt đầu luồng kết nối trên Relay Node
    /// [EN] Tracks stream start on a relay node
    /// </summary>
    void TrackStreamStart(string relayId);

    /// <summary>
    /// [VI] Ghi nhận kết thúc luồng kết nối và cộng dồn lưu lượng trên Relay Node
    /// [EN] Tracks stream end and aggregates transferred bytes on a relay node
    /// </summary>
    void TrackStreamEnd(string relayId, long bytesTransferred);

    /// <summary>
    /// [VI] Đo sức khỏe định kỳ cho các VPS Trung chuyển
    /// [EN] Performs health check on all relay nodes
    /// </summary>
    Task HealthCheckAllAsync();
}

/// <summary>
/// [VI] Dịch vụ quản lý cụm VPS Trung chuyển phân tán với bộ chuyển mạch linh hoạt và lưu trữ bền vững
/// [EN] Service managing distributed Relay VPS fleet with dynamic master switch, weighted load balancing and JSON persistence
/// </summary>
public class RelayPoolManagerService : IRelayPoolManagerService
{
    private readonly ConcurrentDictionary<string, RelayNode> _relays = new();
    private readonly ILogger<RelayPoolManagerService> _logger;
    private readonly Random _random = new();
    private readonly string _storagePath;
    private readonly object _lock = new();
    private RelayRoutingMode _activeMode = RelayRoutingMode.SemiRelay;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    public RelayRoutingMode ActiveMode
    {
        get => _activeMode;
        set
        {
            if (_activeMode != value)
            {
                _activeMode = value;
                Save();
            }
        }
    }

    public RelayPoolManagerService(ILogger<RelayPoolManagerService> logger)
    {
        _logger = logger;
        string baseDir = AppContext.BaseDirectory;
        string dataDir = Path.Combine(baseDir, "data");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        _storagePath = Path.Combine(dataDir, "relays.json");
        Load();
    }

    private void Load()
    {
        lock (_lock)
        {
            if (File.Exists(_storagePath))
            {
                try
                {
                    string json = File.ReadAllText(_storagePath);
                    var state = JsonSerializer.Deserialize<RelayStorageState>(json, _jsonOptions);
                    if (state != null && state.Relays != null && state.Relays.Count > 0)
                    {
                        _activeMode = state.ActiveMode;
                        _relays.Clear();
                        foreach (var item in state.Relays)
                        {
                            _relays[item.Id] = item;
                        }
                        _logger.LogInformation("[RelayPool] Đã nạp {Count} relay nodes từ file {Path}. Chế độ: {Mode}",
                            _relays.Count, _storagePath, _activeMode);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[RelayPool] Lỗi đọc file relays.json, sử dụng cấu hình mặc định.");
                }
            }

            SeedDefaultRelays();
            Save();
        }
    }

    private void Save()
    {
        lock (_lock)
        {
            try
            {
                var state = new RelayStorageState
                {
                    ActiveMode = _activeMode,
                    Relays = _relays.Values.ToList()
                };
                string json = JsonSerializer.Serialize(state, _jsonOptions);
                File.WriteAllText(_storagePath, json);

                // Đồng bộ vào workspace source data nếu có
                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "relays.json");
                if (!string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(_storagePath), StringComparison.OrdinalIgnoreCase))
                {
                    var sDir = Path.GetDirectoryName(sourcePath);
                    if (!string.IsNullOrEmpty(sDir) && !Directory.Exists(sDir)) Directory.CreateDirectory(sDir);
                    File.WriteAllText(sourcePath, json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RelayPool] Lỗi khi lưu cấu hình relay xuống đĩa.");
            }
        }
    }

    private void SeedDefaultRelays()
    {
        var defaultRelays = new List<RelayNode>
        {
            new RelayNode
            {
                Id = "relay-sg-01",
                Name = "Relay-SG-Core (Singapore)",
                Ip = "103.150.12.88",
                Port = 10000,
                Country = "SG",
                CountryName = "Singapore",
                Region = "Asia",
                Weight = 40,
                MaxStreams = 5000,
                PingMs = 18,
                Status = "ONLINE",
                IsEnabled = true
            },
            new RelayNode
            {
                Id = "relay-ty-02",
                Name = "Relay-Tokyo-Equinix (Japan)",
                Ip = "45.76.108.92",
                Port = 10000,
                Country = "JP",
                CountryName = "Japan",
                Region = "Asia",
                Weight = 30,
                MaxStreams = 4000,
                PingMs = 38,
                Status = "ONLINE",
                IsEnabled = true
            },
            new RelayNode
            {
                Id = "relay-fra-03",
                Name = "Relay-Frankfurt-Hetzner (Germany)",
                Ip = "159.69.198.45",
                Port = 10000,
                Country = "DE",
                CountryName = "Germany",
                Region = "Europe",
                Weight = 30,
                MaxStreams = 6000,
                PingMs = 145,
                Status = "ONLINE",
                IsEnabled = true
            },
            new RelayNode
            {
                Id = "relay-lax-04",
                Name = "Relay-LosAngeles-Vultr (United States)",
                Ip = "108.61.219.14",
                Port = 10000,
                Country = "US",
                CountryName = "United States",
                Region = "Americas",
                Weight = 35,
                MaxStreams = 5000,
                PingMs = 180,
                Status = "ONLINE",
                IsEnabled = true
            }
        };

        foreach (var r in defaultRelays)
        {
            _relays[r.Id] = r;
        }
    }

    public IReadOnlyList<RelayNode> GetAllRelays() => _relays.Values.OrderBy(r => r.Country).ToList();

    public RelayNode? GetById(string id) => _relays.TryGetValue(id, out var r) ? r : null;

    public RelayNode AddRelay(RelayNode node)
    {
        if (string.IsNullOrWhiteSpace(node.Id)) node.Id = Guid.NewGuid().ToString("N")[..8];
        node.CreatedAt = DateTime.UtcNow;
        node.LastCheckedAt = DateTime.UtcNow;
        _relays[node.Id] = node;
        Save();
        return node;
    }

    public bool UpdateRelay(string id, int weight, bool isEnabled, int maxStreams)
    {
        if (_relays.TryGetValue(id, out var r))
        {
            r.Weight = Math.Clamp(weight, 1, 100);
            r.IsEnabled = isEnabled;
            r.MaxStreams = Math.Max(100, maxStreams);
            Save();
            return true;
        }
        return false;
    }

    public bool DeleteRelay(string id)
    {
        bool removed = _relays.TryRemove(id, out _);
        if (removed)
        {
            Save();
        }
        return removed;
    }

    public bool ShouldUseRelay(bool isVipUser, string? targetCountry)
    {
        switch (ActiveMode)
        {
            case RelayRoutingMode.FullRelay:
                return true; // 100% bắt buộc qua Relay

            case RelayRoutingMode.DirectBypass:
                return false; // Bỏ qua hoàn toàn (quá tải hoặc sự cố)

            case RelayRoutingMode.SemiRelay:
            default:
                // VIP luôn được bảo vệ qua Relay; User Free chỉ qua Relay nếu dàn VPS còn nhiều tải trống
                if (isVipUser) return true;
                
                // Kiểm tra xem dàn Relay có bị quá tải không (Tổng stream < 60% capacity)
                int totalActive = _relays.Values.Sum(r => r.ActiveStreams);
                int totalCap = _relays.Values.Sum(r => r.MaxStreams);
                return totalCap > 0 && ((double)totalActive / totalCap) < 0.60;
        }
    }

    public RelayNode? ResolveRelayNode(string? targetCountry)
    {
        var activeNodes = _relays.Values
            .Where(r => r.IsEnabled && r.Status != "OFFLINE" && r.ActiveStreams < r.MaxStreams)
            .ToList();

        if (activeNodes.Count == 0)
        {
            // Fallback: Tìm node online bất kỳ kể cả sắp đầy
            activeNodes = _relays.Values.Where(r => r.IsEnabled && r.Status != "OFFLINE").ToList();
            if (activeNodes.Count == 0) return null;
        }

        // 1. Ưu tiên Geo-Matching (Ví dụ: Khách chọn US -> Ưu tiên Relay US; Khách chọn JP/SG -> Ưu tiên Relay Asia)
        if (!string.IsNullOrWhiteSpace(targetCountry))
        {
            var geoMatches = activeNodes
                .Where(r => r.Country.Equals(targetCountry, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (geoMatches.Count > 0)
            {
                return SelectByWeight(geoMatches);
            }
        }

        // 2. Chọn theo Trọng số tải (Weighted Random Selection)
        return SelectByWeight(activeNodes);
    }

    private RelayNode SelectByWeight(List<RelayNode> nodes)
    {
        int totalWeight = nodes.Sum(n => Math.Max(1, n.Weight));
        int randomRoll = _random.Next(1, totalWeight + 1);
        int currentSum = 0;

        foreach (var node in nodes)
        {
            currentSum += Math.Max(1, node.Weight);
            if (randomRoll <= currentSum)
            {
                return node;
            }
        }

        return nodes[0];
    }

    public void TrackStreamStart(string relayId)
    {
        if (_relays.TryGetValue(relayId, out var r))
        {
            Interlocked.Increment(ref r.ActiveStreams);
        }
    }

    public void TrackStreamEnd(string relayId, long bytesTransferred)
    {
        if (_relays.TryGetValue(relayId, out var r))
        {
            if (r.ActiveStreams > 0) Interlocked.Decrement(ref r.ActiveStreams);
            Interlocked.Add(ref r.TotalBytesRelayed, bytesTransferred);
        }
    }

    public async Task HealthCheckAllAsync()
    {
        var tasks = _relays.Values.Select(async relay =>
        {
            try
            {
                using var tcp = new TcpClient();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var connectTask = tcp.ConnectAsync(relay.Ip, relay.Port);
                var completed = await Task.WhenAny(connectTask, Task.Delay(3000));

                if (completed == connectTask && tcp.Connected)
                {
                    sw.Stop();
                    relay.PingMs = (int)sw.ElapsedMilliseconds;
                    relay.Status = relay.PingMs > 1500 ? "DEGRADED" : "ONLINE";
                }
                else
                {
                    relay.Status = "OFFLINE";
                    relay.PingMs = 9999;
                }
            }
            catch
            {
                relay.Status = "OFFLINE";
                relay.PingMs = 9999;
            }
            relay.LastCheckedAt = DateTime.UtcNow;
        });

        await Task.WhenAll(tasks);
    }
}
