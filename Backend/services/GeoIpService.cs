using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VpnResidentialHub.Services;

/// <summary>
/// [VI] Mô hình thông tin định vị địa lý của địa chỉ IP
/// [EN] GeoIP location information model
/// </summary>
public class GeoLocationInfo
{
    [JsonPropertyName("query")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string CountryName { get; set; } = "Vietnam";

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = "VN";

    [JsonPropertyName("city")]
    public string City { get; set; } = "Hanoi";

    [JsonPropertyName("isp")]
    public string Isp { get; set; } = "Residential ISP";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "success";
}

public interface IGeoIpService
{
    Task<GeoLocationInfo> ResolveIpAsync(string ip);
    Task<Dictionary<string, GeoLocationInfo>> ResolveBatchIpsAsync(IEnumerable<string> ips);
}

/// <summary>
/// [VI] Dịch vụ tra cứu GeoIP siêu tốc kết hợp Cache đĩa và Batch IP-API
/// [EN] High-performance GeoIP lookup service with disk cache & batch API
/// </summary>
public class GeoIpService : IGeoIpService
{
    private readonly ConcurrentDictionary<string, GeoLocationInfo> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _cacheFilePath;
    private readonly HttpClient _httpClient;
    private readonly object _fileLock = new();

    public GeoIpService(IHttpClientFactory? httpClientFactory = null)
    {
        string baseDir = AppContext.BaseDirectory;
        string dataDir = Path.Combine(baseDir, "data");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        _cacheFilePath = Path.Combine(dataDir, "geoip_cache.json");
        _httpClient = httpClientFactory?.CreateClient() ?? new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(5);

        LoadCache();
    }

    private void LoadCache()
    {
        lock (_fileLock)
        {
            if (File.Exists(_cacheFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_cacheFilePath);
                    var list = JsonSerializer.Deserialize<List<GeoLocationInfo>>(json);
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Ip))
                            {
                                _cache[item.Ip] = item;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GeoIpService] Load cache error: {ex.Message}");
                }
            }
        }
    }

    private void SaveCache()
    {
        lock (_fileLock)
        {
            try
            {
                string json = JsonSerializer.Serialize(_cache.Values.ToList());
                File.WriteAllText(_cacheFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeoIpService] Save cache error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// [VI] Tra cứu nhanh 1 IP (từ Cache hoặc phân tích Fallback)
    /// [EN] Fast single IP lookup from cache or fallback
    /// </summary>
    public async Task<GeoLocationInfo> ResolveIpAsync(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return GetFallback(ip);
        }

        if (_cache.TryGetValue(ip, out var cached))
        {
            return cached;
        }

        var batchResult = await ResolveBatchIpsAsync(new[] { ip });
        if (batchResult.TryGetValue(ip, out var resolved))
        {
            return resolved;
        }

        return GetFallback(ip);
    }

    /// <summary>
    /// [VI] Tra cứu hàng loạt IP với Batch 100 IP/lần để đạt tốc độ tối đa
    /// [EN] Batch resolve IPs (100 IPs per request) for maximum speed
    /// </summary>
    public async Task<Dictionary<string, GeoLocationInfo>> ResolveBatchIpsAsync(IEnumerable<string> ips)
    {
        var result = new Dictionary<string, GeoLocationInfo>(StringComparer.OrdinalIgnoreCase);
        var uniqueIps = ips.Where(ip => !string.IsNullOrWhiteSpace(ip)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var missingIps = new List<string>();

        foreach (var ip in uniqueIps)
        {
            if (_cache.TryGetValue(ip, out var cached))
            {
                result[ip] = cached;
            }
            else
            {
                missingIps.Add(ip);
            }
        }

        if (missingIps.Count == 0)
        {
            return result;
        }

        bool cacheUpdated = false;

        // Chia nhỏ thành các batch 100 IP (giới hạn của ip-api.com batch)
        var batches = missingIps.Chunk(100);
        foreach (var batch in batches)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("http://ip-api.com/batch?fields=query,status,country,countryCode,city,isp", batch);
                if (response.IsSuccessStatusCode)
                {
                    var items = await response.Content.ReadFromJsonAsync<List<GeoLocationInfo>>();
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Ip))
                            {
                                if (string.IsNullOrWhiteSpace(item.CountryCode) || item.Status != "success")
                                {
                                    // Fallback heuristic nếu API không nhận diện được
                                    var fb = GetFallback(item.Ip);
                                    item.CountryCode = fb.CountryCode;
                                    item.CountryName = fb.CountryName;
                                    item.City = fb.City;
                                    item.Isp = fb.Isp;
                                }

                                _cache[item.Ip] = item;
                                result[item.Ip] = item;
                                cacheUpdated = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeoIpService] Batch query failed: {ex.Message}");
                // Fallback nhanh cho batch này
                foreach (var ip in batch)
                {
                    var fb = GetFallback(ip);
                    _cache[ip] = fb;
                    result[ip] = fb;
                    cacheUpdated = true;
                }
            }
        }

        // Kiểm tra xem còn IP nào sót chưa có kết quả
        foreach (var ip in missingIps)
        {
            if (!result.ContainsKey(ip))
            {
                var fb = GetFallback(ip);
                _cache[ip] = fb;
                result[ip] = fb;
                cacheUpdated = true;
            }
        }

        if (cacheUpdated)
        {
            SaveCache();
        }

        return result;
    }

    /// <summary>
    /// [VI] Nhận diện Heuristic nhanh các dải IP phổ biến khi offline
    /// [EN] Fast heuristic lookup for common subnets when offline
    /// </summary>
    public static GeoLocationInfo GetFallback(string ip)
    {
        string countryCode = "VN";
        string countryName = "Vietnam";
        string city = "Hanoi";
        string isp = "Viettel Telecom";

        if (string.IsNullOrWhiteSpace(ip))
        {
            return new GeoLocationInfo { Ip = ip, CountryCode = countryCode, CountryName = countryName, City = city, Isp = isp };
        }

        // Dải IP Việt Nam phổ biến
        if (ip.StartsWith("103.") || ip.StartsWith("14.") || ip.StartsWith("113.") ||
            ip.StartsWith("115.") || ip.StartsWith("116.") || ip.StartsWith("117.") ||
            ip.StartsWith("118.") || ip.StartsWith("123.") || ip.StartsWith("125.") ||
            ip.StartsWith("171.") || ip.StartsWith("222.") || ip.StartsWith("42.") ||
            ip.StartsWith("1.52.") || ip.StartsWith("27.") || ip.StartsWith("58.186."))
        {
            countryCode = "VN";
            countryName = "Vietnam";
            city = ip.StartsWith("103.166") ? "Da Nang" : (ip.StartsWith("103.82") ? "Hanoi" : "Ho Chi Minh City");
            isp = ip.StartsWith("103.166") ? "CLOUDFLY Residential" : "VNPT / Viettel Residential";
        }
        else if (ip.StartsWith("8.") || ip.StartsWith("23.") || ip.StartsWith("64.") ||
                 ip.StartsWith("66.") || ip.StartsWith("98.") || ip.StartsWith("104.") ||
                 ip.StartsWith("107.") || ip.StartsWith("172.") || ip.StartsWith("198."))
        {
            countryCode = "US";
            countryName = "United States";
            city = "Los Angeles";
            isp = "Verizon / AT&T Residential";
        }
        else if (ip.StartsWith("133.") || ip.StartsWith("150.") || ip.StartsWith("160.") || ip.StartsWith("210."))
        {
            countryCode = "JP";
            countryName = "Japan";
            city = "Tokyo";
            isp = "NTT Docomo";
        }
        else if (ip.StartsWith("165.") || ip.StartsWith("175.") || ip.StartsWith("203."))
        {
            countryCode = "SG";
            countryName = "Singapore";
            city = "Singapore";
            isp = "Singtel";
        }

        return new GeoLocationInfo
        {
            Ip = ip,
            CountryCode = countryCode,
            CountryName = countryName,
            City = city,
            Isp = isp,
            Status = "fallback"
        };
    }
}
