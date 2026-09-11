using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VpnResidentialHub.Models;

namespace VpnResidentialHub.Services;

/// <summary>
/// [VI] Giao thức quản lý thông tin khách hàng thuê cổng proxy (Tenants)
/// [EN] Interface for managing proxy tenants, authentication, and quotas
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// [VI] Lấy danh sách tất cả các tenant trong hệ thống
    /// [EN] Retrieves all registered tenants
    /// </summary>
    IReadOnlyList<Tenant> GetAllTenants();

    /// <summary>
    /// [VI] Tìm tenant theo định danh Id
    /// [EN] Finds tenant by unique ID
    /// </summary>
    Tenant? GetById(string id);

    /// <summary>
    /// [VI] Tìm tenant theo API Key
    /// [EN] Finds tenant by API Key
    /// </summary>
    Tenant? GetByApiKey(string apiKey);

    /// <summary>
    /// [VI] Xác thực thông tin đăng nhập của tenant (hỗ trợ so sánh an toàn constant-time)
    /// [EN] Authenticates tenant credentials (supports constant-time comparison)
    /// </summary>
    Tenant? Authenticate(string username, string password);

    /// <summary>
    /// [VI] Tạo tenant mới và lưu xuống đĩa
    /// [EN] Creates a new tenant and persists to storage
    /// </summary>
    Tenant CreateTenant(CreateTenantRequest request);

    /// <summary>
    /// [VI] Xóa tenant theo Id và cập nhật lưu trữ
    /// [EN] Deletes tenant by ID and updates storage
    /// </summary>
    bool DeleteTenant(string id);

    /// <summary>
    /// [VI] Ghi nhận lưu lượng băng thông tiêu thụ
    /// [EN] Records bandwidth consumption for a tenant
    /// </summary>
    void RecordUsage(string tenantId, long bytes);

    /// <summary>
    /// [VI] Phân tích cú pháp username định dạng mở rộng SOCKS5/HTTP
    /// [EN] Parses extended username formatting for target routing rules
    /// </summary>
    (Tenant? Tenant, string? Country, string? City, string? SessionId, int? SessionMinutes, string? TargetNodeId) ParseProxyUsername(string fullUsername);
}

/// <summary>
/// [VI] Dịch vụ quản lý tài khoản khách hàng thuê (Tenants) có lưu trữ bền vững (Persistence)
/// [EN] Tenant management service with persistent JSON storage and thread-safe quota accounting
/// </summary>
public class TenantService : ITenantService
{
    private readonly ConcurrentDictionary<string, Tenant> _tenants = new();
    private readonly ILogger<TenantService> _logger;
    private readonly string _storagePath;
    private readonly object _lock = new();
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    public TenantService(ILogger<TenantService> logger)
    {
        _logger = logger;
        string baseDir = AppContext.BaseDirectory;
        string dataDir = Path.Combine(baseDir, "data");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        _storagePath = Path.Combine(dataDir, "tenants.json");
        Load();
    }

    /// <summary>
    /// [VI] Nạp danh sách tenant từ đĩa hoặc khởi tạo mặc định nếu chưa tồn tại
    /// [EN] Loads tenants from disk or initializes defaults if not present
    /// </summary>
    private void Load()
    {
        lock (_lock)
        {
            if (File.Exists(_storagePath))
            {
                try
                {
                    string json = File.ReadAllText(_storagePath);
                    var list = JsonSerializer.Deserialize<List<Tenant>>(json, _jsonOptions);
                    if (list != null && list.Count > 0)
                    {
                        _tenants.Clear();
                        foreach (var item in list)
                        {
                            _tenants[item.Id] = item;
                        }
                        _logger.LogInformation("[TenantService] Đã nạp {Count} tenant từ file lưu trữ {Path}.", _tenants.Count, _storagePath);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[TenantService] Lỗi khi nạp file tenants.json, khởi tạo lại mặc định.");
                }
            }

            InitializeDefaultTenants();
            Save();
        }
    }

    /// <summary>
    /// [VI] Ghi danh sách tenant hiện tại xuống đĩa bền vững
    /// [EN] Saves current tenant registry to persistent disk
    /// </summary>
    private void Save()
    {
        lock (_lock)
        {
            try
            {
                var list = _tenants.Values.ToList();
                string json = JsonSerializer.Serialize(list, _jsonOptions);
                File.WriteAllText(_storagePath, json);

                // Đồng bộ vào workspace source data nếu có
                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "tenants.json");
                if (!string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(_storagePath), StringComparison.OrdinalIgnoreCase))
                {
                    var sDir = Path.GetDirectoryName(sourcePath);
                    if (!string.IsNullOrEmpty(sDir) && !Directory.Exists(sDir)) Directory.CreateDirectory(sDir);
                    File.WriteAllText(sourcePath, json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TenantService] Lỗi khi lưu danh sách tenant xuống đĩa.");
            }
        }
    }

    private void InitializeDefaultTenants()
    {
        // 1. Tenant mặc định dành riêng cho KikiLogin Antidetect
        var kikiTenant = new Tenant
        {
            Id = "tenant_kiki",
            Name = "KikiLogin Antidetect Ecosystem",
            Username = "kikilogin",
            Password = "kiki_resident_pass_2026",
            ApiKey = "kiki_live_key_998877665544332211",
            QuotaGb = 100.0,
            UsedBytes = 0,
            MaxConcurrentConnections = 200,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        };
        _tenants[kikiTenant.Id] = kikiTenant;

        // 2. Tenant mẫu cho Agency thuê ngoài
        var agencyTenant = new Tenant
        {
            Id = "tenant_agency",
            Name = "Global Marketing Agency",
            Username = "agency_vip",
            Password = "agency_pass_secure",
            ApiKey = "agency_key_112233445566778899",
            QuotaGb = 50.0,
            UsedBytes = 0,
            MaxConcurrentConnections = 100,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddMonths(3)
        };
        _tenants[agencyTenant.Id] = agencyTenant;
    }

    public IReadOnlyList<Tenant> GetAllTenants() => _tenants.Values.ToList();

    public Tenant? GetById(string id)
    {
        _tenants.TryGetValue(id, out var tenant);
        return tenant;
    }

    public Tenant? GetByApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return null;
        return _tenants.Values.FirstOrDefault(t => t.ApiKey == apiKey && t.IsActive);
    }

    public Tenant? Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var userBytes = Encoding.UTF8.GetBytes(password);

        var tenant = _tenants.Values.FirstOrDefault(t => 
            t.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
            CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(t.Password), userBytes) && 
            t.IsActive);

        if (tenant != null)
        {
            // Kiểm tra hạn sử dụng và Quota
            if (tenant.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("[Auth] Tenant '{Name}' đã hết hạn sử dụng.", tenant.Name);
                return null;
            }

            if (tenant.QuotaGb > 0 && tenant.UsedGb >= tenant.QuotaGb)
            {
                _logger.LogWarning("[Auth] Tenant '{Name}' đã dùng hết Quota băng thông ({Quota} GB).", tenant.Name, tenant.QuotaGb);
                return null;
            }
        }

        return tenant;
    }

    public Tenant CreateTenant(CreateTenantRequest request)
    {
        var cleanUsername = "usr_" + Guid.NewGuid().ToString("N")[..6];
        var cleanPassword = "pwd_" + Guid.NewGuid().ToString("N")[..8];

        var tenant = new Tenant
        {
            Id = "tenant_" + Guid.NewGuid().ToString("N")[..8],
            Name = request.Name,
            Username = cleanUsername,
            Password = cleanPassword,
            ApiKey = "kiki_live_key_" + Guid.NewGuid().ToString("N"),
            QuotaGb = request.QuotaGb,
            UsedBytes = 0,
            MaxConcurrentConnections = request.MaxConnections,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(request.ExpiryDays > 0 ? request.ExpiryDays : 30)
        };

        _tenants[tenant.Id] = tenant;
        _logger.LogInformation("[Tenant] Đã tạo Tenant mới: {Name} (Username: {User}, Quota: {Quota}GB)", 
            tenant.Name, tenant.Username, tenant.QuotaGb);

        Save();
        return tenant;
    }

    public bool DeleteTenant(string id)
    {
        var removed = _tenants.TryRemove(id, out var t);
        if (removed && t != null)
        {
            _logger.LogInformation("[Tenant] Đã xóa Tenant: {Name}", t.Name);
            Save();
        }
        return removed;
    }

    private long _unsavedBytes = 0;

    public void RecordUsage(string tenantId, long bytes)
    {
        if (_tenants.TryGetValue(tenantId, out var tenant))
        {
            Interlocked.Add(ref tenant.UsedBytes, bytes);
            long acc = Interlocked.Add(ref _unsavedBytes, bytes);
            // Định kỳ ghi xuống đĩa mỗi khi tích lũy 5MB hoặc có thể flush
            if (acc > 5 * 1024 * 1024)
            {
                Interlocked.Exchange(ref _unsavedBytes, 0);
                Save();
            }
        }
    }

    /// <summary>
    /// Phân tích username theo chuẩn công nghiệp:
    /// Ví dụ: "kikilogin-country-us-city-losangeles-session-prof101-time-30-node-win_da26056e4dfe"
    /// </summary>
    public (Tenant? Tenant, string? Country, string? City, string? SessionId, int? SessionMinutes, string? TargetNodeId) ParseProxyUsername(string fullUsername)
    {
        if (string.IsNullOrWhiteSpace(fullUsername))
        {
            return (null, null, null, null, null, null);
        }

        var parts = fullUsername.Split('-');
        string rawUsername = parts[0];

        string? country = null;
        string? city = null;
        string? sessionId = null;
        int? sessionMinutes = null;
        string? targetNodeId = null;

        for (int i = 1; i < parts.Length; i++)
        {
            var key = parts[i].ToLowerInvariant();
            if (i + 1 < parts.Length)
            {
                var val = parts[i + 1];
                if (key == "country" || key == "zone")
                {
                    country = val.ToUpperInvariant();
                    i++;
                }
                else if (key == "city")
                {
                    city = val;
                    i++;
                }
                else if (key == "session" || key == "sess")
                {
                    sessionId = val;
                    i++;
                }
                else if (key == "time" || key == "ttl")
                {
                    if (int.TryParse(val, out var m)) sessionMinutes = m;
                    i++;
                }
                else if (key == "node" || key == "device")
                {
                    targetNodeId = val;
                    i++;
                }
            }
        }

        // Tìm tenant theo username gốc
        var tenant = _tenants.Values.FirstOrDefault(t => t.Username.Equals(rawUsername, StringComparison.OrdinalIgnoreCase) && t.IsActive);
        return (tenant, country, city, sessionId, sessionMinutes, targetNodeId);
    }
}
