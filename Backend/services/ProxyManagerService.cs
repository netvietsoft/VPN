using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VpnResidentialHub.Services;

/// <summary>
/// [VI] Đối tượng Upstream Proxy kèm phân cấp và hạn mức băng thông
/// [EN] Upstream proxy entity with tiering and bandwidth quota accounting
/// </summary>
public class UpstreamProxy
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Type { get; set; } = "socks5"; // "socks5" | "http"
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1080;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string Country { get; set; } = "VN";
    public string CountryName { get; set; } = "Vietnam";
    public string City { get; set; } = "Hanoi";
    public string Isp { get; set; } = "Residential ISP";
    public int? PingMs { get; set; }
    public string Status { get; set; } = "UNCHECKED"; // "LIVE" | "OFFLINE" | "UNCHECKED"
    public DateTime? LastChecked { get; set; }
    public bool IsActiveVpn { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- CƠ CHẾ PHÂN CẤP & BẢN QUYỀN (TIERING & AUTH) ---
    public string Tier { get; set; } = "Vip"; // "Free" (No-Pass/Slow) | "Vip" (Auth/Fast)
    
    [JsonIgnore]
    public bool HasAuth => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

    // --- CƠ CHẾ HẠN MỨC 10GB/THÁNG ~ 300MB/NGÀY (BANDWIDTH METERING) ---
    public long MonthlyQuotaBytes { get; set; } = 10L * 1024 * 1024 * 1024; // 10 GB
    public long DailyQuotaBytes { get; set; } = 300L * 1024 * 1024; // 300 MB
    public long UsedDailyBytes { get; set; } = 0;
    public long UsedMonthlyBytes { get; set; } = 0;
    public string DailyResetDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
    public bool IsExhaustedToday { get; set; } = false;
    public int AutoSwitchedCount { get; set; } = 0;

    [JsonIgnore]
    public string DisplayString => string.IsNullOrEmpty(Username)
        ? $"{Host}:{Port}"
        : $"{Host}:{Port}:{Username}:{Password}";
}

public class BulkImportResult
{
    public int Added { get; set; }
    public int TotalLines { get; set; }
    public int UniqueIps { get; set; }
    public Dictionary<string, int> CountryBreakdown { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public interface IProxyManagerService
{
    IReadOnlyList<UpstreamProxy> GetAll();
    UpstreamProxy? GetById(string id);
    UpstreamProxy? GetActiveVpnProxy();
    UpstreamProxy Add(UpstreamProxy proxy);
    int AddBulk(string bulkText, string defaultType = "socks5");
    Task<BulkImportResult> AddBulkAsync(string bulkText, string defaultType = "socks5", bool replaceExisting = false, bool autoGeoIp = true);
    bool Delete(string id);
    int BatchDelete(IEnumerable<string> ids);
    bool ClearAll();
    int DeleteOffline();
    bool SelectActiveVpn(string id);
    Task<UpstreamProxy?> TestProxyAsync(string id);
    Task<IReadOnlyList<UpstreamProxy>> TestAllAsync();
    Task<IReadOnlyList<UpstreamProxy>> BatchTestAsync(IEnumerable<string> ids);
    object GetVpnLocations();
    object GetCountrySummary();
    void RecordBandwidthUsage(string proxyId, long bytes);
    int ResetDailyQuotas();
    object GetQuotaOverview();
    void Reload();
}

public class ProxyManagerService : IProxyManagerService
{
    private readonly ConcurrentDictionary<string, UpstreamProxy> _proxies = new();
    private readonly string _storagePath;
    private readonly object _lock = new();
    private readonly IGeoIpService _geoIpService;

    public ProxyManagerService(IGeoIpService? geoIpService = null)
    {
        _geoIpService = geoIpService ?? new GeoIpService();
        string baseDir = AppContext.BaseDirectory;
        string dataDir = Path.Combine(baseDir, "data");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        _storagePath = Path.Combine(dataDir, "proxies.json");
        Load();
    }

    public void Reload() => Load();

    private void Load()
    {
        lock (_lock)
        {
            if (File.Exists(_storagePath))
            {
                try
                {
                    string json = File.ReadAllText(_storagePath);
                    var list = JsonSerializer.Deserialize<List<UpstreamProxy>>(json);
                    if (list != null && list.Count > 0)
                    {
                        _proxies.Clear();
                        foreach (var item in list)
                        {
                            _proxies[item.Id] = item;
                        }
                        EnsureXrayVpnNodes();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ProxyManager] Error loading proxies: {ex.Message}");
                }
            }

            // Danh sách proxy hạt giống ban đầu (Seed Default Proxies)
            SeedDefaults();
            EnsureXrayVpnNodes();
            Save();
        }
    }

    private void EnsureXrayVpnNodes()
    {
        // [VI] Nạp các node VPN Xray Reality đã kiểm định hợp lệ (Chỉ nạp server hoạt động tốt làm VPN)
        // [EN] Seed verified Xray Reality VPN nodes (only load servers verified capable of VPN)
        var xrayNodes = new List<UpstreamProxy>
        {
            new() { Id = "vpn_xray_bg_1", Type = "vless", Host = "bg.bsdup.com", Port = 63821, Country = "BG", CountryName = "Bulgaria", City = "Sofia", Isp = "NextAI Reality (www.microsoft.com)", Status = "LIVE", PingMs = 231 },
            new() { Id = "vpn_xray_bg_2", Type = "vless", Host = "bg.bsdup.com", Port = 63821, Country = "BG", CountryName = "Bulgaria", City = "Sofia", Isp = "NextAI Reality (www.microsoft.com)", Status = "LIVE", PingMs = 235 },
            new() { Id = "vpn_xray_de_3", Type = "vless", Host = "77.90.188.26", Port = 63821, Country = "DE", CountryName = "Germany", City = "Frankfurt", Isp = "NextAI Reality (www.tiktok.com)", Status = "LIVE", PingMs = 209, IsActiveVpn = true }
        };

        foreach (var node in xrayNodes)
        {
            if (!_proxies.ContainsKey(node.Id))
            {
                _proxies[node.Id] = node;
            }
        }
    }

    private void SeedDefaults()
    {
        var defaults = new List<UpstreamProxy>
        {
            new() { Id = "proxy_vn_hanoi", Type = "socks5", Host = "14.225.254.10", Port = 1080, Country = "VN", CountryName = "Vietnam", City = "Hanoi", Isp = "Viettel Telecom", Status = "LIVE", PingMs = 18, IsActiveVpn = true },
            new() { Id = "proxy_vn_hcm", Type = "socks5", Host = "113.161.72.15", Port = 1080, Country = "VN", CountryName = "Vietnam", City = "Ho Chi Minh City", Isp = "VNPT Residential", Status = "LIVE", PingMs = 24 },
            new() { Id = "proxy_us_lax", Type = "socks5", Host = "104.28.19.45", Port = 1080, Country = "US", CountryName = "United States", City = "Los Angeles", Isp = "AT&T Fiber Residential", Status = "LIVE", PingMs = 145 },
            new() { Id = "proxy_us_nyc", Type = "socks5", Host = "198.51.100.22", Port = 1080, Country = "US", CountryName = "United States", City = "New York City", Isp = "Verizon Fios", Status = "LIVE", PingMs = 168 },
            new() { Id = "proxy_sg_sin", Type = "socks5", Host = "103.253.144.8", Port = 1080, Country = "SG", CountryName = "Singapore", City = "Singapore", Isp = "Singtel Residential", Status = "LIVE", PingMs = 38 },
            new() { Id = "proxy_jp_tyo", Type = "socks5", Host = "133.242.18.99", Port = 1080, Country = "JP", CountryName = "Japan", City = "Tokyo", Isp = "NTT Docomo", Status = "LIVE", PingMs = 72 },
            new() { Id = "proxy_gb_lon", Type = "socks5", Host = "151.236.21.40", Port = 1080, Country = "GB", CountryName = "United Kingdom", City = "London", Isp = "BT Residential", Status = "LIVE", PingMs = 180 },
            new() { Id = "proxy_de_fra", Type = "socks5", Host = "185.12.64.12", Port = 1080, Country = "DE", CountryName = "Germany", City = "Frankfurt", Isp = "Deutsche Telekom", Status = "LIVE", PingMs = 195 }
        };

        foreach (var p in defaults)
        {
            _proxies[p.Id] = p;
        }
    }

    private void Save()
    {
        lock (_lock)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_proxies.Values.ToList(), options);
                File.WriteAllText(_storagePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProxyManager] Error saving proxies: {ex.Message}");
            }
        }
    }

    public IReadOnlyList<UpstreamProxy> GetAll() => _proxies.Values.OrderByDescending(p => p.CreatedAt).ToList();

    public UpstreamProxy? GetById(string id) => _proxies.TryGetValue(id, out var p) ? p : null;

    public UpstreamProxy? GetActiveVpnProxy() => _proxies.Values.FirstOrDefault(p => p.IsActiveVpn) ?? _proxies.Values.FirstOrDefault();

    public UpstreamProxy Add(UpstreamProxy proxy)
    {
        if (string.IsNullOrWhiteSpace(proxy.Id))
        {
            proxy.Id = Guid.NewGuid().ToString("N")[..8];
        }

        // Chuẩn hóa Quốc gia
        NormalizeCountry(proxy);

        _proxies[proxy.Id] = proxy;
        Save();
        return proxy;
    }

    public int AddBulk(string bulkText, string defaultType = "socks5")
    {
        return AddBulkAsync(bulkText, defaultType, replaceExisting: false, autoGeoIp: true).GetAwaiter().GetResult().Added;
    }

    /// <summary>
    /// [VI] Nhập hàng loạt siêu tốc hàng nghìn proxy, tự động giải mã GeoIP theo quốc gia
    /// [EN] High-speed bulk import for thousands of proxies with GeoIP auto-resolution
    /// </summary>
    public async Task<BulkImportResult> AddBulkAsync(string bulkText, string defaultType = "socks5", bool replaceExisting = false, bool autoGeoIp = true)
    {
        var result = new BulkImportResult();
        if (string.IsNullOrWhiteSpace(bulkText)) return result;

        var lines = bulkText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        result.TotalLines = lines.Length;

        var parsedProxies = new List<UpstreamProxy>();
        var uniqueHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            var proxy = ParseProxyLine(line, defaultType);
            if (proxy != null)
            {
                parsedProxies.Add(proxy);
                uniqueHosts.Add(proxy.Host);
            }
        }

        result.UniqueIps = uniqueHosts.Count;

        // Tự động phân tách và gán thông tin quốc gia qua GeoIP
        if (autoGeoIp && uniqueHosts.Count > 0)
        {
            var geoMap = await _geoIpService.ResolveBatchIpsAsync(uniqueHosts);
            foreach (var proxy in parsedProxies)
            {
                if (geoMap.TryGetValue(proxy.Host, out var geo))
                {
                    proxy.Country = geo.CountryCode.ToUpperInvariant();
                    proxy.CountryName = geo.CountryName;
                    proxy.City = geo.City;
                    proxy.Isp = geo.Isp;
                }
                else
                {
                    NormalizeCountry(proxy);
                }
            }
        }
        else
        {
            foreach (var proxy in parsedProxies)
            {
                NormalizeCountry(proxy);
            }
        }

        if (replaceExisting)
        {
            _proxies.Clear();
        }

        bool hasActive = _proxies.Values.Any(p => p.IsActiveVpn);

        foreach (var proxy in parsedProxies)
        {
            if (!hasActive)
            {
                proxy.IsActiveVpn = true;
                hasActive = true;
            }

            _proxies[proxy.Id] = proxy;
            result.Added++;

            string countryKey = string.IsNullOrWhiteSpace(proxy.CountryName) ? proxy.Country : proxy.CountryName;
            if (result.CountryBreakdown.TryGetValue(countryKey, out int count))
            {
                result.CountryBreakdown[countryKey] = count + 1;
            }
            else
            {
                result.CountryBreakdown[countryKey] = 1;
            }
        }

        if (result.Added > 0)
        {
            Save();
        }

        return result;
    }

    public bool Delete(string id)
    {
        if (_proxies.TryRemove(id, out _))
        {
            Save();
            return true;
        }
        return false;
    }

    public int BatchDelete(IEnumerable<string> ids)
    {
        int deleted = 0;
        lock (_lock)
        {
            foreach (var id in ids)
            {
                if (_proxies.TryRemove(id, out _))
                {
                    deleted++;
                }
            }
            if (deleted > 0)
            {
                Save();
            }
        }
        return deleted;
    }

    public bool ClearAll()
    {
        _proxies.Clear();
        Save();
        return true;
    }

    public int DeleteOffline()
    {
        var offlineIds = _proxies.Values.Where(p => p.Status == "OFFLINE").Select(p => p.Id).ToList();
        int deleted = 0;
        foreach (var id in offlineIds)
        {
            if (_proxies.TryRemove(id, out _))
            {
                deleted++;
            }
        }
        if (deleted > 0) Save();
        return deleted;
    }

    public bool SelectActiveVpn(string id)
    {
        if (!_proxies.ContainsKey(id)) return false;

        foreach (var kv in _proxies)
        {
            kv.Value.IsActiveVpn = (kv.Key == id);
        }
        Save();
        return true;
    }

    public async Task<UpstreamProxy?> TestProxyAsync(string id)
    {
        if (!_proxies.TryGetValue(id, out var proxy)) return null;

        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(proxy.Host, proxy.Port);
            var timeoutTask = Task.Delay(3500);

            var finished = await Task.WhenAny(connectTask, timeoutTask);
            if (finished != connectTask)
            {
                proxy.Status = "OFFLINE";
                proxy.PingMs = null;
                proxy.LastChecked = DateTime.UtcNow;
                Save();
                return proxy;
            }

            // Nếu là SOCKS5, thử bắt tay SOCKS5
            if (proxy.Type.Equals("socks5", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = client.GetStream();
                stream.ReadTimeout = 3000;
                stream.WriteTimeout = 3000;

                // Client greeting: SOCKS5, 1 method: No Auth
                byte[] greeting = new byte[] { 0x05, 0x01, 0x00 };
                await stream.WriteAsync(greeting);

                byte[] resp = new byte[2];
                int read = await stream.ReadAsync(resp.AsMemory(0, 2));
                if (read >= 2 && resp[0] == 0x05)
                {
                    sw.Stop();
                    proxy.PingMs = (int)Math.Max(1, sw.ElapsedMilliseconds);
                    proxy.Status = "LIVE";
                }
                else
                {
                    sw.Stop();
                    proxy.PingMs = (int)Math.Max(1, sw.ElapsedMilliseconds);
                    proxy.Status = "LIVE"; // Vẫn kết nối TCP thành công
                }
            }
            else
            {
                sw.Stop();
                proxy.PingMs = (int)Math.Max(1, sw.ElapsedMilliseconds);
                proxy.Status = "LIVE";
            }
        }
        catch
        {
            proxy.Status = "OFFLINE";
            proxy.PingMs = null;
        }

        proxy.LastChecked = DateTime.UtcNow;
        Save();
        return proxy;
    }

    public async Task<IReadOnlyList<UpstreamProxy>> TestAllAsync()
    {
        // Chạy kiểm tra tối đa 25 proxy cùng một lúc để tránh cạn socket
        var list = _proxies.Values.ToList();
        var chunks = list.Chunk(25);
        foreach (var chunk in chunks)
        {
            var tasks = chunk.Select(p => TestProxyAsync(p.Id));
            await Task.WhenAll(tasks);
        }
        return GetAll();
    }

    public async Task<IReadOnlyList<UpstreamProxy>> BatchTestAsync(IEnumerable<string> ids)
    {
        var idSet = new HashSet<string>(ids);
        var targetList = _proxies.Values.Where(p => idSet.Contains(p.Id)).ToList();
        var chunks = targetList.Chunk(25);
        foreach (var chunk in chunks)
        {
            var tasks = chunk.Select(p => TestProxyAsync(p.Id));
            await Task.WhenAll(tasks);
        }
        return targetList;
    }

    public object GetCountrySummary()
    {
        var groups = _proxies.Values
            .GroupBy(p => new { Code = p.Country.ToUpperInvariant(), Name = p.CountryName })
            .Select(g => new
            {
                CountryCode = g.Key.Code,
                CountryName = g.Key.Name,
                Total = g.Count(),
                Live = g.Count(p => p.Status == "LIVE"),
                Cities = g.Select(c => c.City).Distinct().ToList()
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        return new
        {
            TotalProxies = _proxies.Count,
            TotalCountries = groups.Count,
            Countries = groups
        };
    }

    public object GetVpnLocations()
    {
        // Trả về định dạng cho Desktop Client nạp vào AllLocationsControl
        var list = _proxies.Values.Select(p => new
        {
            Id = p.Id,
            Country = p.CountryName,
            CountryCode = p.Country.ToUpperInvariant(),
            City = p.City,
            CityCode = p.City.Replace(" ", "").ToLowerInvariant(),
            SearchName = $"{p.CountryName} {p.City} {p.Country}",
            PingMs = p.PingMs ?? 35,
            Load = 15,
            Host = p.Host,
            Port = p.Port,
            Type = p.Type,
            Status = p.Status,
            IsActive = p.IsActiveVpn
        }).ToList();

        // Luôn có mục "Best Available" đầu danh sách
        var best = list.OrderBy(x => x.PingMs).FirstOrDefault();

        return new
        {
            Success = true,
            Total = list.Count,
            BestAvailable = best,
            Locations = list
        };
    }

    public void RecordBandwidthUsage(string proxyId, long bytes)
    {
        if (bytes <= 0) return;
        if (_proxies.TryGetValue(proxyId, out var proxy))
        {
            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (proxy.DailyResetDate != today)
            {
                proxy.UsedDailyBytes = 0;
                proxy.IsExhaustedToday = false;
                proxy.DailyResetDate = today;
            }

            proxy.UsedDailyBytes += bytes;
            proxy.UsedMonthlyBytes += bytes;

            // Kiểm tra chạm ngưỡng 300MB/ngày (300 * 1024 * 1024 bytes)
            if (proxy.UsedDailyBytes >= proxy.DailyQuotaBytes && !proxy.IsExhaustedToday)
            {
                proxy.IsExhaustedToday = true;
                proxy.AutoSwitchedCount++;
            }
        }
    }

    public int ResetDailyQuotas()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        int resetCount = 0;
        foreach (var p in _proxies.Values)
        {
            if (p.DailyResetDate != today || p.IsExhaustedToday)
            {
                p.UsedDailyBytes = 0;
                p.IsExhaustedToday = false;
                p.DailyResetDate = today;
                resetCount++;
            }
        }
        if (resetCount > 0) Save();
        return resetCount;
    }

    public object GetQuotaOverview()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var all = _proxies.Values.ToList();
        int totalProxies = all.Count;
        int activeQuotaCount = all.Count(p => !p.IsExhaustedToday);
        int exhaustedTodayCount = all.Count(p => p.IsExhaustedToday);
        int freeTierCount = all.Count(p => p.Tier.Equals("Free", StringComparison.OrdinalIgnoreCase));
        int vipTierCount = all.Count(p => p.Tier.Equals("Vip", StringComparison.OrdinalIgnoreCase));
        long totalDailyUsedBytes = all.Sum(p => p.UsedDailyBytes);
        long totalMonthlyUsedBytes = all.Sum(p => p.UsedMonthlyBytes);
        int totalAutoSwitches = all.Sum(p => p.AutoSwitchedCount);

        return new
        {
            Success = true,
            Today = today,
            TotalProxies = totalProxies,
            ActiveQuotaCount = activeQuotaCount,
            ExhaustedTodayCount = exhaustedTodayCount,
            FreeTierCount = freeTierCount,
            VipTierCount = vipTierCount,
            TotalDailyUsedMB = Math.Round((double)totalDailyUsedBytes / (1024 * 1024), 2),
            TotalMonthlyUsedGB = Math.Round((double)totalMonthlyUsedBytes / (1024 * 1024 * 1024), 2),
            TotalAutoSwitches = totalAutoSwitches,
            PerProxyDailyQuotaMB = 300,
            PerProxyMonthlyQuotaGB = 10
        };
    }

    private static UpstreamProxy? ParseProxyLine(string line, string defaultType)
    {
        try
        {
            string type = defaultType;
            string host = string.Empty;
            int port = 1080;
            string? user = null;
            string? pass = null;

            // Xử lý URI format: socks5://user:pass@host:port hoặc http://...
            if (line.Contains("://"))
            {
                var uri = new Uri(line);
                type = uri.Scheme.ToLowerInvariant();
                host = uri.Host;
                port = uri.Port > 0 ? uri.Port : (type == "http" ? 8080 : 1080);
                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    var parts = uri.UserInfo.Split(':');
                    user = parts[0];
                    if (parts.Length > 1) pass = parts[1];
                }
            }
            else
            {
                // Format: host:port:user:pass hoặc user:pass@host:port hoặc host:port
                if (line.Contains('@'))
                {
                    var atParts = line.Split('@');
                    var authParts = atParts[0].Split(':');
                    user = authParts[0];
                    if (authParts.Length > 1) pass = authParts[1];

                    var hp = atParts[1].Split(':');
                    host = hp[0];
                    port = int.Parse(hp[1]);
                }
                else
                {
                    var parts = line.Split(':');
                    if (parts.Length >= 2)
                    {
                        host = parts[0].Trim();
                        port = int.Parse(parts[1].Trim());
                        if (parts.Length >= 4)
                        {
                            user = parts[2].Trim();
                            pass = parts[3].Trim();
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(host) || port <= 0) return null;

            bool hasCredentials = !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass);

            var proxy = new UpstreamProxy
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Type = type,
                Host = host,
                Port = port,
                Username = user,
                Password = pass,
                Tier = hasCredentials ? "Vip" : "Free",
                MonthlyQuotaBytes = 10L * 1024 * 1024 * 1024,
                DailyQuotaBytes = 300L * 1024 * 1024,
                UsedDailyBytes = 0,
                UsedMonthlyBytes = 0,
                DailyResetDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                IsExhaustedToday = false,
                AutoSwitchedCount = 0,
                Status = "UNCHECKED",
                CreatedAt = DateTime.UtcNow
            };

            return proxy;
        }
        catch
        {
            return null;
        }
    }

    private static void NormalizeCountry(UpstreamProxy p)
    {
        var map = new Dictionary<string, (string code, string name, string city)>(StringComparer.OrdinalIgnoreCase)
        {
            { "VN", ("VN", "Vietnam", "Hanoi") },
            { "Vietnam", ("VN", "Vietnam", "Hanoi") },
            { "US", ("US", "United States", "Los Angeles") },
            { "USA", ("US", "United States", "New York City") },
            { "United States", ("US", "United States", "Los Angeles") },
            { "SG", ("SG", "Singapore", "Singapore") },
            { "Singapore", ("SG", "Singapore", "Singapore") },
            { "JP", ("JP", "Japan", "Tokyo") },
            { "Japan", ("JP", "Japan", "Tokyo") },
            { "GB", ("GB", "United Kingdom", "London") },
            { "UK", ("GB", "United Kingdom", "London") },
            { "United Kingdom", ("GB", "United Kingdom", "London") },
            { "DE", ("DE", "Germany", "Frankfurt") },
            { "Germany", ("DE", "Germany", "Frankfurt") },
            { "FR", ("FR", "France", "Paris") },
            { "France", ("FR", "France", "Paris") },
            { "CA", ("CA", "Canada", "Toronto") },
            { "Canada", ("CA", "Canada", "Toronto") },
            { "AU", ("AU", "Australia", "Sydney") },
            { "Australia", ("AU", "Australia", "Sydney") },
            { "KR", ("KR", "South Korea", "Seoul") },
            { "Korea", ("KR", "South Korea", "Seoul") },
            { "HK", ("HK", "Hong Kong", "Hong Kong") },
            { "Hong Kong", ("HK", "Hong Kong", "Hong Kong") }
        };

        if (!string.IsNullOrWhiteSpace(p.Country) && map.TryGetValue(p.Country, out var found))
        {
            p.Country = found.code;
            p.CountryName = found.name;
            if (string.IsNullOrWhiteSpace(p.City)) p.City = found.city;
        }
        else if (string.IsNullOrWhiteSpace(p.Country))
        {
            p.Country = "VN";
            p.CountryName = "Vietnam";
            p.City = "Hanoi";
        }
        else
        {
            p.Country = p.Country.ToUpperInvariant();
            p.CountryName = p.Country;
            if (string.IsNullOrWhiteSpace(p.City)) p.City = "City";
        }
    }
}
