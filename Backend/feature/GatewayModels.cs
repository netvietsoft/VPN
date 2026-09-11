namespace VpnResidentialHub.Models;

/// <summary>
/// Đại diện cho một khách hàng / đối tác thuê băng thông hoặc cổng kết nối (Tenant)
/// Represents a customer or system tenant renting bandwidth or proxy ports
/// </summary>
public class Tenant
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = Guid.NewGuid().ToString("N");
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    /// <summary>Hạn mức dung lượng tính theo Gigabytes (0 = Không giới hạn)</summary>
    public double QuotaGb { get; set; } = 50.0;
    
    /// <summary>Dung lượng đã sử dụng tính theo Bytes</summary>
    public long UsedBytes;
    
    /// <summary>Số luồng kết nối đồng thời tối đa</summary>
    public int MaxConcurrentConnections { get; set; } = 100;
    
    /// <summary>Trạng thái hoạt động</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Tài khoản VIP / Băng thông ưu tiên qua cụm Relay</summary>
    public bool IsVip { get; set; } = false;
    
    /// <summary>Thời hạn gói thuê</summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMonths(1);
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public double UsedGb => Math.Round((double)UsedBytes / (1024 * 1024 * 1024), 3);
    public double RemainingGb => QuotaGb <= 0 ? 999999 : Math.Max(0, QuotaGb - UsedGb);
}

/// <summary>
/// Đại diện cho một node IP cư dân trong kho
/// Represents a residential IP node in the pool
/// </summary>
public class ResidentialNode
{
    public string Id { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string Country { get; set; } = "US";
    public string CountryName { get; set; } = "United States";
    public string State { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Timezone { get; set; } = "America/New_York";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Isp { get; set; } = "Residential ISP";
    public string Protocol { get; set; } = "socks5"; // socks5, http, wireguard
    public int FraudScore { get; set; } = 0; // 0-100 (càng thấp càng sạch)
    public bool IsActive { get; set; } = true;
    public long TotalBytesServed;
    public DateTime LastRotatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Phiên kết nối proxy đang hoạt động qua Gateway
/// Active proxy session routed through the gateway
/// </summary>
public class ProxySession
{
    public string SessionId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientIp { get; set; } = string.Empty;
    public ResidentialNode Node { get; set; } = new();
    public string TargetHost { get; set; } = string.Empty;
    public int TargetPort { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public long BytesTransferred { get; set; }
    public bool IsAlive { get; set; } = true;
}

/// <summary>
/// Yêu cầu sinh proxy xuất danh sách
/// </summary>
public class GenerateProxyRequest
{
    public string ApiKey { get; set; } = string.Empty;
    public string Country { get; set; } = "ALL"; // ALL hoặc mã ISO (US, VN, JP, DE...)
    public string? City { get; set; }
    public string Mode { get; set; } = "rotating"; // rotating, sticky
    public int SessionMinutes { get; set; } = 30; // Dùng cho sticky
    public int Quantity { get; set; } = 10;
    public string Protocol { get; set; } = "socks5"; // socks5, http
    public string Format { get; set; } = "host:port:user:pass"; // host:port:user:pass, json
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public double QuotaGb { get; set; } = 10.0;
    public int MaxConnections { get; set; } = 50;
    public int ExpiryDays { get; set; } = 30;
}

public class BulkImportRequest
{
    public string Text { get; set; } = string.Empty;
    public string? DefaultType { get; set; } = "socks5";
    public bool ReplaceExisting { get; set; } = false;
    public bool AutoGeoIp { get; set; } = true;
}

public class BatchIdsRequest
{
    public List<string> Ids { get; set; } = new();
}

/// <summary>
/// [VI] Thiết bị khách hàng cài NextAI VPN đóng góp IP cư dân vào Mesh Pool
/// [EN] Client device with NextAI VPN installed contributing residential IP to Mesh Pool
/// </summary>
public class ClientDeviceNode
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string PublicIp { get; set; } = string.Empty;
    public int Port { get; set; } = 10000;
    public string Protocol { get; set; } = "socks5";
    public string Country { get; set; } = "VN";
    public string CountryName { get; set; } = "Vietnam";
    public string City { get; set; } = string.Empty;
    public string Isp { get; set; } = "Residential ISP";
    public string OsVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = "8.0.0.0";
    public string DeviceType { get; set; } = "Desktop"; // "Desktop" | "Mobile" | "Tablet"
    public int FraudScore { get; set; } = 0;
    public bool IsOnline { get; set; } = true;
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
    public DateTime LastHeartbeatAt { get; set; } = DateTime.UtcNow;
    public long TotalBytesServed { get; set; } = 0;
}

public class RegisterClientNodeRequest
{
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? PublicIp { get; set; }
    public int? Port { get; set; } = 10000;
    public string? Protocol { get; set; } = "socks5";
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Isp { get; set; }
    public string? OsVersion { get; set; }
    public string? AppVersion { get; set; }
    public string? DeviceType { get; set; } = "Desktop";
}

public class ClientNodeHeartbeatRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public long? BytesServedDelta { get; set; }
    public ushort? CurrentPingMs { get; set; }
}


public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
