using System.Collections.Concurrent;
using VpnResidentialHub.Models;

namespace VpnResidentialHub.Services;

public interface ITenantService
{
    IReadOnlyList<Tenant> GetAllTenants();
    Tenant? GetById(string id);
    Tenant? GetByApiKey(string apiKey);
    Tenant? Authenticate(string username, string password);
    Tenant CreateTenant(CreateTenantRequest request);
    bool DeleteTenant(string id);
    void RecordUsage(string tenantId, long bytes);
    (Tenant? Tenant, string? Country, string? City, string? SessionId, int? SessionMinutes) ParseProxyUsername(string fullUsername);
}

public class TenantService : ITenantService
{
    private readonly ConcurrentDictionary<string, Tenant> _tenants = new();
    private readonly ILogger<TenantService> _logger;

    public TenantService(ILogger<TenantService> logger)
    {
        _logger = logger;
        InitializeDefaultTenants();
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
        return _tenants.Values.FirstOrDefault(t => t.ApiKey.Equals(apiKey, StringComparison.OrdinalIgnoreCase) && t.IsActive);
    }

    public Tenant? Authenticate(string username, string password)
    {
        var tenant = _tenants.Values.FirstOrDefault(t => 
            t.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
            t.Password == password &&
            t.IsActive);

        if (tenant != null)
        {
            // Kiểm tra hạn sử dụng và quota
            if (tenant.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("[Auth] Tài khoản '{Username}' đã hết hạn thuê!", username);
                return null;
            }
            if (tenant.QuotaGb > 0 && tenant.UsedGb >= tenant.QuotaGb)
            {
                _logger.LogWarning("[Auth] Tài khoản '{Username}' đã dùng hết hạn mức ({Quota} GB)!", username, tenant.QuotaGb);
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
            Name = request.Name,
            Username = cleanUsername,
            Password = cleanPassword,
            ApiKey = "key_" + Guid.NewGuid().ToString("N"),
            QuotaGb = request.QuotaGb,
            MaxConcurrentConnections = request.MaxConnections,
            ExpiresAt = DateTime.UtcNow.AddDays(request.ExpiryDays > 0 ? request.ExpiryDays : 30),
            IsActive = true
        };

        _tenants[tenant.Id] = tenant;
        _logger.LogInformation("[TenantService] Đã tạo thành công Tenant mới: '{Name}' (User: {User}, Quota: {Quota}GB)",
            tenant.Name, tenant.Username, tenant.QuotaGb);

        return tenant;
    }

    public bool DeleteTenant(string id)
    {
        return _tenants.TryRemove(id, out _);
    }

    public void RecordUsage(string tenantId, long bytes)
    {
        if (_tenants.TryGetValue(tenantId, out var tenant))
        {
            Interlocked.Add(ref tenant.UsedBytes, bytes);
        }
    }

    /// <summary>
    /// Phân tích username theo chuẩn công nghiệp:
    /// Ví dụ: "kikilogin-country-us-city-losangeles-session-prof101-time-30"
    /// </summary>
    public (Tenant? Tenant, string? Country, string? City, string? SessionId, int? SessionMinutes) ParseProxyUsername(string fullUsername)
    {
        if (string.IsNullOrWhiteSpace(fullUsername))
        {
            return (null, null, null, null, null);
        }

        var parts = fullUsername.Split('-');
        string rawUsername = parts[0];

        string? country = null;
        string? city = null;
        string? sessionId = null;
        int? sessionMinutes = null;

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
            }
        }

        // Tìm tenant theo username gốc
        var tenant = _tenants.Values.FirstOrDefault(t => t.Username.Equals(rawUsername, StringComparison.OrdinalIgnoreCase) && t.IsActive);
        return (tenant, country, city, sessionId, sessionMinutes);
    }
}
