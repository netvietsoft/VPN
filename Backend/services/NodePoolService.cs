using System.Collections.Concurrent;
using VpnResidentialHub.Models;

namespace VpnResidentialHub.Services;

/// <summary>
/// [VI] Giao diện quản lý kho tài nguyên IP Dân cư (Mesh Residential Node Pool)
/// [EN] Interface for managing the residential IP pool and client contributor devices
/// </summary>
public interface INodePoolService
{
    /// <summary>
    /// [VI] Lấy danh sách toàn bộ các node IP dân cư
    /// [EN] Retrieves all residential nodes
    /// </summary>
    IReadOnlyList<ResidentialNode> GetAllNodes();

    /// <summary>
    /// [VI] Lấy node tối ưu nhất theo quốc gia và thành phố
    /// [EN] Selects best available residential node matching country and city
    /// </summary>
    ResidentialNode GetBestNode(string? country = null, string? city = null);

    /// <summary>
    /// [VI] Xoay sang IP mới cho một profile trình duyệt
    /// [EN] Rotates to a fresh IP for a specific browser profile
    /// </summary>
    ResidentialNode RotateNode(string profileId, string? country = null, string? city = null);

    /// <summary>
    /// [VI] Đăng ký một node upstream mới vào kho
    /// [EN] Registers a new upstream node to the pool
    /// </summary>
    void RegisterUpstreamNode(ResidentialNode node);

    /// <summary>
    /// [VI] Tổng số node đang hoạt động
    /// [EN] Total count of active online nodes
    /// </summary>
    int TotalActiveNodes { get; }

    /// <summary>
    /// [VI] Đăng ký thiết bị người dùng cài đặt NextAI VPN đóng góp IP
    /// [EN] Registers a client device contributing residential IP
    /// </summary>
    Task<ClientDeviceNode> RegisterClientNodeAsync(RegisterClientNodeRequest req, string detectedRemoteIp);

    /// <summary>
    /// [VI] Ghi nhận nhịp tim duy trì kết nối của thiết bị
    /// [EN] Records keep-alive heartbeat from client device
    /// </summary>
    bool RecordClientHeartbeat(ClientNodeHeartbeatRequest req);

    /// <summary>
    /// [VI] Lấy danh sách các thiết bị khách hàng đã đăng ký
    /// [EN] Retrieves list of registered client devices
    /// </summary>
    IReadOnlyList<ClientDeviceNode> GetClientNodes();
}

public class NodePoolService : INodePoolService
{
    private readonly ConcurrentBag<ResidentialNode> _nodes = new();
    private readonly ConcurrentDictionary<string, string> _profileLastIp = new();
    private readonly ConcurrentDictionary<string, ClientDeviceNode> _clientNodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Random _random = new();
    private readonly IGeoIpService _geoIpService;
    private readonly string _clientNodesFilePath;
    private readonly object _saveLock = new();

    public int TotalActiveNodes => _nodes.Count(n => n.IsActive);

    public NodePoolService(IGeoIpService? geoIpService = null)
    {
        _geoIpService = geoIpService ?? new GeoIpService();
        _clientNodesFilePath = Path.Combine(AppContext.BaseDirectory, "data", "client_nodes.json");
        InitializeDefaultPool();
        LoadClientNodesFromDisk();
    }

    private void InitializeDefaultPool()
    {
        // Danh sách mẫu các node Residential sạch chuẩn mực đa quốc gia
        var seedNodes = new List<ResidentialNode>
        {
            // US Residential
            new() {
                Id = "res_us_lax_01",
                Ip = "172.56.21.84",
                Country = "US",
                CountryName = "United States",
                State = "California",
                City = "Los Angeles",
                Timezone = "America/Los_Angeles",
                Latitude = 34.0522,
                Longitude = -118.2437,
                Isp = "AT&T Residential Fiber",
                FraudScore = 2,
                Protocol = "socks5"
            },
            new() {
                Id = "res_us_nyc_02",
                Ip = "68.195.142.11",
                Country = "US",
                CountryName = "United States",
                State = "New York",
                City = "New York City",
                Timezone = "America/New_York",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Isp = "Verizon Fios Residential",
                FraudScore = 1,
                Protocol = "socks5"
            },
            new() {
                Id = "res_us_dal_03",
                Ip = "76.184.205.99",
                Country = "US",
                CountryName = "United States",
                State = "Texas",
                City = "Dallas",
                Timezone = "America/Chicago",
                Latitude = 32.7767,
                Longitude = -96.7970,
                Isp = "Spectrum Charter Communications",
                FraudScore = 0,
                Protocol = "socks5"
            },
            new() {
                Id = "res_us_mia_04",
                Ip = "73.139.77.45",
                Country = "US",
                CountryName = "United States",
                State = "Florida",
                City = "Miami",
                Timezone = "America/New_York",
                Latitude = 25.7617,
                Longitude = -80.1918,
                Isp = "Comcast Cable Residential",
                FraudScore = 3,
                Protocol = "socks5"
            },
            
            // Vietnam Residential & 4G
            new() {
                Id = "res_vn_sgn_01",
                Ip = "14.169.112.45",
                Country = "VN",
                CountryName = "Vietnam",
                State = "Ho Chi Minh",
                City = "Ho Chi Minh City",
                Timezone = "Asia/Ho_Chi_Minh",
                Latitude = 10.8231,
                Longitude = 106.6297,
                Isp = "VNPT Residential Broadband",
                FraudScore = 0,
                Protocol = "socks5"
            },
            new() {
                Id = "res_vn_han_02",
                Ip = "113.190.234.18",
                Country = "VN",
                CountryName = "Vietnam",
                State = "Hanoi",
                City = "Hanoi",
                Timezone = "Asia/Ho_Chi_Minh",
                Latitude = 21.0285,
                Longitude = 105.8542,
                Isp = "Viettel Telecom 4G/FTTH",
                FraudScore = 0,
                Protocol = "socks5"
            },
            new() {
                Id = "res_vn_sgn_03",
                Ip = "42.112.89.201",
                Country = "VN",
                CountryName = "Vietnam",
                State = "Ho Chi Minh",
                City = "Ho Chi Minh City",
                Timezone = "Asia/Ho_Chi_Minh",
                Latitude = 10.8231,
                Longitude = 106.6297,
                Isp = "FPT Telecom Residential",
                FraudScore = 1,
                Protocol = "socks5"
            },

            // Japan Residential
            new() {
                Id = "res_jp_tyo_01",
                Ip = "126.158.82.130",
                Country = "JP",
                CountryName = "Japan",
                State = "Tokyo",
                City = "Tokyo",
                Timezone = "Asia/Tokyo",
                Latitude = 35.6762,
                Longitude = 139.6503,
                Isp = "Softbank BB Corp",
                FraudScore = 1,
                Protocol = "socks5"
            },
            new() {
                Id = "res_jp_osa_02",
                Ip = "133.204.65.19",
                Country = "JP",
                CountryName = "Japan",
                State = "Osaka",
                City = "Osaka",
                Timezone = "Asia/Tokyo",
                Latitude = 34.6937,
                Longitude = 135.5023,
                Isp = "NTT Communications Hikari",
                FraudScore = 0,
                Protocol = "socks5"
            },

            // Germany Residential
            new() {
                Id = "res_de_fra_01",
                Ip = "84.142.190.73",
                Country = "DE",
                CountryName = "Germany",
                State = "Hesse",
                City = "Frankfurt",
                Timezone = "Europe/Berlin",
                Latitude = 50.1109,
                Longitude = 8.6821,
                Isp = "Deutsche Telekom AG",
                FraudScore = 0,
                Protocol = "socks5"
            },

            // UK Residential
            new() {
                Id = "res_gb_lon_01",
                Ip = "82.34.118.90",
                Country = "GB",
                CountryName = "United Kingdom",
                State = "England",
                City = "London",
                Timezone = "Europe/London",
                Latitude = 51.5074,
                Longitude = -0.1278,
                Isp = "Virgin Media Residential",
                FraudScore = 2,
                Protocol = "socks5"
            },

            // Singapore Residential
            new() {
                Id = "res_sg_sin_01",
                Ip = "118.200.145.62",
                Country = "SG",
                CountryName = "Singapore",
                State = "Singapore",
                City = "Singapore",
                Timezone = "Asia/Singapore",
                Latitude = 1.3521,
                Longitude = 103.8198,
                Isp = "Singtel Residential Broadband",
                FraudScore = 0,
                Protocol = "socks5"
            }
        };

        foreach (var node in seedNodes)
        {
            _nodes.Add(node);
        }
    }

    public IReadOnlyList<ResidentialNode> GetAllNodes() => _nodes.ToList();

    public ResidentialNode GetBestNode(string? country = null, string? city = null)
    {
        var query = _nodes.Where(n => n.IsActive);
        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(n => n.Country.Equals(country, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(n => n.City.Equals(city, StringComparison.OrdinalIgnoreCase));
        }

        var candidates = query.ToList();
        if (candidates.Count == 0)
        {
            // Fallback sang node bất kỳ đang hoạt động
            candidates = _nodes.Where(n => n.IsActive).ToList();
        }

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("Không có node cư dân nào đang hoạt động trong pool!");
        }

        // Chọn node có FraudScore thấp nhất và ngẫu nhiên trong nhóm top
        var minScore = candidates.Min(c => c.FraudScore);
        var bestCandidates = candidates.Where(c => c.FraudScore <= minScore + 5).ToList();
        return bestCandidates[_random.Next(bestCandidates.Count)];
    }

    public ResidentialNode RotateNode(string profileId, string? country = null, string? city = null)
    {
        _profileLastIp.TryGetValue(profileId, out var lastIp);

        var query = _nodes.Where(n => n.IsActive);
        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(n => n.Country.Equals(country, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(n => n.City.Equals(city, StringComparison.OrdinalIgnoreCase));
        }

        var candidates = query.Where(n => n.Ip != lastIp).ToList();
        if (candidates.Count == 0)
        {
            candidates = _nodes.Where(n => n.IsActive).ToList();
        }

        var selected = candidates[_random.Next(candidates.Count)];
        
        // Nếu đây là dynamic pool, sinh biến thể IP cư dân giả lập cùng dải subnet / ISP
        var rotatedNode = new ResidentialNode
        {
            Id = $"{selected.Id}_rot_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            Ip = GenerateNextSubnetIp(selected.Ip),
            Country = selected.Country,
            CountryName = selected.CountryName,
            State = selected.State,
            City = selected.City,
            Timezone = selected.Timezone,
            Latitude = selected.Latitude + (_random.NextDouble() - 0.5) * 0.01,
            Longitude = selected.Longitude + (_random.NextDouble() - 0.5) * 0.01,
            Isp = selected.Isp,
            Protocol = selected.Protocol,
            FraudScore = _random.Next(0, 3),
            IsActive = true,
            LastRotatedAt = DateTime.UtcNow
        };

        _profileLastIp[profileId] = rotatedNode.Ip;
        return rotatedNode;
    }

    public void RegisterUpstreamNode(ResidentialNode node)
    {
        _nodes.Add(node);
    }

    #region Client Residential Node Harvesting (Thu Thập IP Cư Dân)
    public IReadOnlyList<ClientDeviceNode> GetClientNodes()
    {
        // Tự động kiểm tra timeout heartbeat (> 15 phút không có tín hiệu thì coi là OFFLINE)
        var now = DateTime.UtcNow;
        foreach (var node in _clientNodes.Values)
        {
            if (node.IsOnline && (now - node.LastHeartbeatAt).TotalMinutes > 15)
            {
                node.IsOnline = false;
            }
        }
        return _clientNodes.Values.OrderByDescending(n => n.IsOnline).ThenByDescending(n => n.LastHeartbeatAt).ToList();
    }

    public async Task<ClientDeviceNode> RegisterClientNodeAsync(RegisterClientNodeRequest req, string detectedRemoteIp)
    {
        string deviceId = string.IsNullOrWhiteSpace(req.DeviceId)
            ? "dev_" + Guid.NewGuid().ToString("N")[..8]
            : req.DeviceId.Trim();

        // Ưu tiên IP client gửi lên nếu là public IP thật, nếu không lấy detected IP từ connection
        string publicIp = !string.IsNullOrWhiteSpace(req.PublicIp) && req.PublicIp != "127.0.0.1" && req.PublicIp != "::1"
            ? req.PublicIp.Trim()
            : detectedRemoteIp;

        if (string.IsNullOrWhiteSpace(publicIp) || publicIp == "127.0.0.1" || publicIp == "::1")
        {
            publicIp = "14.169.112." + _random.Next(10, 250); // Giả lập IP dân cư nếu chạy local loopback
        }

        // Tự động tra cứu GeoIP để làm giàu dữ liệu (Quốc gia, Thành phố, ISP thật)
        string country = req.Country ?? "VN";
        string countryName = "Vietnam";
        string city = req.City ?? "Hanoi";
        string isp = req.Isp ?? "Residential ISP";

        try
        {
            var geo = await _geoIpService.ResolveIpAsync(publicIp);
            if (geo != null && geo.Status == "success")
            {
                country = geo.CountryCode;
                countryName = geo.CountryName;
                city = geo.City;
                isp = geo.Isp;
            }
        }
        catch { }

        var node = _clientNodes.AddOrUpdate(deviceId,
            addValueFactory: (id) => new ClientDeviceNode
            {
                DeviceId = id,
                DeviceName = string.IsNullOrWhiteSpace(req.DeviceName) ? "NextAi Client Device" : req.DeviceName.Trim(),
                PublicIp = publicIp,
                Port = req.Port ?? 10000,
                Protocol = req.Protocol ?? "socks5",
                Country = country,
                CountryName = countryName,
                City = city,
                Isp = isp,
                OsVersion = req.OsVersion ?? "Windows",
                AppVersion = req.AppVersion ?? "8.0.0.0",
                DeviceType = req.DeviceType ?? "Desktop",
                IsOnline = true,
                FirstSeenAt = DateTime.UtcNow,
                LastHeartbeatAt = DateTime.UtcNow
            },
            updateValueFactory: (id, existing) =>
            {
                existing.PublicIp = publicIp;
                existing.DeviceName = string.IsNullOrWhiteSpace(req.DeviceName) ? existing.DeviceName : req.DeviceName.Trim();
                existing.Country = country;
                existing.CountryName = countryName;
                existing.City = city;
                existing.Isp = isp;
                existing.OsVersion = req.OsVersion ?? existing.OsVersion;
                existing.AppVersion = req.AppVersion ?? existing.AppVersion;
                existing.IsOnline = true;
                existing.LastHeartbeatAt = DateTime.UtcNow;
                return existing;
            });

        // Đưa node vào Mesh Residential Node Pool để có thể xoay IP
        _nodes.Add(new ResidentialNode
        {
            Id = "node_" + deviceId,
            Ip = publicIp,
            Country = country,
            CountryName = countryName,
            City = city,
            Isp = isp,
            Protocol = node.Protocol,
            FraudScore = 0,
            IsActive = true,
            LastRotatedAt = DateTime.UtcNow
        });

        SaveClientNodesToDisk();
        return node;
    }

    public bool RecordClientHeartbeat(ClientNodeHeartbeatRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.DeviceId)) return false;
        if (_clientNodes.TryGetValue(req.DeviceId, out var node))
        {
            node.IsOnline = true;
            node.LastHeartbeatAt = DateTime.UtcNow;
            if (req.BytesServedDelta.HasValue && req.BytesServedDelta.Value > 0)
            {
                node.TotalBytesServed += req.BytesServedDelta.Value;
            }
            return true;
        }
        return false;
    }

    private void LoadClientNodesFromDisk()
    {
        try
        {
            if (File.Exists(_clientNodesFilePath))
            {
                string json = File.ReadAllText(_clientNodesFilePath);
                var list = System.Text.Json.JsonSerializer.Deserialize<List<ClientDeviceNode>>(json);
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        _clientNodes[item.DeviceId] = item;
                        // [VI] BUG-14: Đồng bộ ngược vào _nodes routing pool để sẵn sàng định tuyến ngay sau khi khởi động
                        // [EN] BUG-14: Sync back into _nodes routing pool for immediate routing availability after startup
                        _nodes.Add(new ResidentialNode
                        {
                            Id = "node_" + item.DeviceId,
                            Ip = item.PublicIp,
                            Country = item.Country,
                            CountryName = item.CountryName,
                            City = item.City,
                            Isp = item.Isp,
                            Protocol = item.Protocol,
                            FraudScore = 0,
                            IsActive = item.IsOnline,
                            LastRotatedAt = item.LastHeartbeatAt
                        });
                    }
                }
            }
        }
        catch { }
    }

    private void SaveClientNodesToDisk()
    {
        lock (_saveLock)
        {
            try
            {
                var dir = Path.GetDirectoryName(_clientNodesFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var list = _clientNodes.Values.ToList();
                string json = System.Text.Json.JsonSerializer.Serialize(list, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_clientNodesFilePath, json);

                // Đồng bộ vào thư mục Backend/data trong workspace
                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "client_nodes.json");
                if (!string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(_clientNodesFilePath), StringComparison.OrdinalIgnoreCase))
                {
                    var sDir = Path.GetDirectoryName(sourcePath);
                    if (!string.IsNullOrEmpty(sDir) && !Directory.Exists(sDir)) Directory.CreateDirectory(sDir);
                    File.WriteAllText(sourcePath, json);
                }
            }
            catch { }
        }
    }
    #endregion

    private string GenerateNextSubnetIp(string baseIp)
    {
        var parts = baseIp.Split('.');
        if (parts.Length == 4 && int.TryParse(parts[3], out var lastOctet))
        {
            var nextOctet = (lastOctet + _random.Next(1, 40)) % 250 + 2;
            return $"{parts[0]}.{parts[1]}.{parts[2]}.{nextOctet}";
        }
        return baseIp;
    }
}
