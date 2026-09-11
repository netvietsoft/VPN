using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VpnResidentialHub.Models;

namespace VpnResidentialHub.Services;

/// <summary>
/// [VI] Bản ghi lưu vết phiên cố định (Sticky Session Entry)
/// [EN] Sticky session entry tracking assigned proxy and expiration
/// </summary>
public class StickySessionEntry
{
    public string SessionKey { get; set; } = string.Empty;
    public string ProxyId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    public int ActiveConnections { get; set; } = 0;
}

public interface ISmartProxyRotationEngine
{
    /// <summary>
    /// [VI] Điều phối và chọn proxy tối ưu theo tham số phiên và sức khỏe
    /// [EN] Resolves the optimal proxy based on session parameters and health
    /// </summary>
    UpstreamProxy? ResolveProxy(
        Tenant? tenant,
        string? targetCountry,
        string? targetCity,
        string? sessionId,
        int? sessionMinutes = null,
        bool forceRotate = false);

    /// <summary>
    /// [VI] Báo cáo proxy gặp sự cố kết nối (để kích hoạt Failover ngay lập tức)
    /// [EN] Reports connection failure to trigger immediate failover
    /// </summary>
    void ReportFailure(string proxyId, string? sessionKey = null);

    /// <summary>
    /// [VI] Báo cáo hoàn tất kết nối thành công
    /// [EN] Reports successful connection
    /// </summary>
    void ReportSuccess(string proxyId);

    /// <summary>
    /// [VI] Lấy danh sách các session đang gắn cố định (Sticky)
    /// [EN] Gets list of active sticky sessions
    /// </summary>
    IReadOnlyList<StickySessionEntry> GetActiveStickySessions();

    /// <summary>
    /// [VI] Xóa sạch các session đã hết hạn
    /// [EN] Cleans up expired sticky sessions
    /// </summary>
    void CleanupExpiredSessions();
}

/// <summary>
/// [VI] Động cơ xoay proxy SaaS thông minh học hỏi từ mã nguồn decompiler BestAvailableServerHelper
/// [EN] Smart SaaS Proxy Rotation Engine inspired by decompiled BestAvailableServerHelper logic
/// </summary>
public class SmartProxyRotationEngine : ISmartProxyRotationEngine
{
    private readonly IProxyManagerService _proxyManager;
    private readonly ILogger<SmartProxyRotationEngine> _logger;
    private readonly ConcurrentDictionary<string, StickySessionEntry> _stickySessions = new();
    private readonly ConcurrentDictionary<string, int> _proxyConnectionCounts = new();

    public SmartProxyRotationEngine(
        IProxyManagerService proxyManager,
        ILogger<SmartProxyRotationEngine> logger)
    {
        _proxyManager = proxyManager;
        _logger = logger;
    }

    public UpstreamProxy? ResolveProxy(
        Tenant? tenant,
        string? targetCountry,
        string? targetCity,
        string? sessionId,
        int? sessionMinutes = null,
        bool forceRotate = false)
    {
        CleanupExpiredSessions();

        string tenantId = tenant?.Id ?? "anonymous";
        bool isStickyRequested = !string.IsNullOrWhiteSpace(sessionId) && !forceRotate;
        string sessionKey = $"{tenantId}_{sessionId}";

        // 1. Kiểm tra Sticky Session hiện có
        if (isStickyRequested && _stickySessions.TryGetValue(sessionKey, out var existingSession))
        {
            if (DateTime.UtcNow <= existingSession.ExpiresAt)
            {
                var boundProxy = _proxyManager.GetById(existingSession.ProxyId);
                // Kiểm tra proxy còn sống VÀ chưa cạn kiệt 300MB quota ngày hôm nay
                if (boundProxy != null && boundProxy.Status.Equals("LIVE", StringComparison.OrdinalIgnoreCase) && !boundProxy.IsExhaustedToday)
                {
                    existingSession.LastActiveAt = DateTime.UtcNow;
                    _proxyConnectionCounts.AddOrUpdate(boundProxy.Id, 1, (_, c) => c + 1);
                    return boundProxy;
                }
                else
                {
                    // Proxy đã chết hoặc hết 300MB quota ngày -> Tự động chuyển vùng cứu hộ (Auto-Switching Failover)
                    string reason = (boundProxy != null && boundProxy.IsExhaustedToday) ? "ĐÃ HẾT 300MB QUOTA NGÀY" : "ĐÃ OFFLINE";
                    _logger.LogWarning("[RotationEngine] Proxy {Id} trong Sticky Session {Key} {Reason}. Đang tự động ĐẢO PROXY (Auto-Switching)...", 
                        existingSession.ProxyId, sessionKey, reason);
                    _stickySessions.TryRemove(sessionKey, out _);
                }
            }
            else
            {
                _stickySessions.TryRemove(sessionKey, out _);
            }
        }

        // 2. Lấy toàn bộ proxy trong kho và áp dụng Hàng Rào Sức Khỏe & Hạn Mức Quota 300MB/Ngày
        var allProxies = _proxyManager.GetAll();
        if (allProxies.Count == 0)
        {
            return _proxyManager.GetActiveVpnProxy();
        }

        // Lọc bỏ các proxy đã cạn kiệt quota 300MB trong ngày (Quota Gate)
        var nonExhaustedPool = allProxies.Where(p => !p.IsExhaustedToday).ToList();
        if (nonExhaustedPool.Count == 0)
        {
            // Nếu toàn bộ kho đều hết hạn mức ngày, mở lại tạm thời để không đứt mạng
            nonExhaustedPool = allProxies.ToList();
        }

        // Phân cấp Tầng Proxy theo loại tài khoản (Tier Gate):
        // User Free -> Ưu tiên dùng Free Pool (Không pass, tốc độ thường)
        // User VIP -> Ưu tiên dùng VIP Pool (Có User:Pass, tốc độ cao)
        bool isVip = tenant != null && (tenant.IsVip || !tenant.Id.Equals("free_tier", StringComparison.OrdinalIgnoreCase));
        List<UpstreamProxy> tieredPool;
        if (isVip)
        {
            var vipCandidates = nonExhaustedPool.Where(p => p.Tier.Equals("Vip", StringComparison.OrdinalIgnoreCase) || p.HasAuth).ToList();
            tieredPool = vipCandidates.Count > 0 ? vipCandidates : nonExhaustedPool;
        }
        else
        {
            var freeCandidates = nonExhaustedPool.Where(p => p.Tier.Equals("Free", StringComparison.OrdinalIgnoreCase) || !p.HasAuth).ToList();
            tieredPool = freeCandidates.Count > 0 ? freeCandidates : nonExhaustedPool;
        }

        // Ưu tiên các proxy có trạng thái LIVE
        var livePool = tieredPool
            .Where(p => p.Status.Equals("LIVE", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Fallback: Nếu kho chưa chạy health check hoặc không có node nào LIVE, lấy tạm UNCHECKED
        var candidatePool = livePool.Count > 0 
            ? livePool 
            : tieredPool.Where(p => !p.Status.Equals("OFFLINE", StringComparison.OrdinalIgnoreCase)).ToList();

        if (candidatePool.Count == 0)
        {
            candidatePool = tieredPool.Count > 0 ? tieredPool : allProxies.ToList();
        }

        // 3. Lọc theo Vị Trí Địa Lý (Geo Matching)
        List<UpstreamProxy> geoFiltered = candidatePool;

        if (!string.IsNullOrWhiteSpace(targetCountry))
        {
            var countryMatches = candidatePool.Where(p => 
                p.Country.Equals(targetCountry, StringComparison.OrdinalIgnoreCase) ||
                p.CountryName.Equals(targetCountry, StringComparison.OrdinalIgnoreCase)).ToList();

            if (countryMatches.Count > 0)
            {
                geoFiltered = countryMatches;

                // Nếu có chỉ định thành phố (City)
                if (!string.IsNullOrWhiteSpace(targetCity))
                {
                    var cityMatches = countryMatches.Where(p => 
                        p.City.Equals(targetCity, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (cityMatches.Count > 0)
                    {
                        geoFiltered = cityMatches;
                    }
                }
            }
        }

        // 4. Thuật toán Cân Bằng Tải & Chọn Node Tối Ưu (BestAvailableServerHelper Pattern)
        // Tiêu chí: (1) Số kết nối đang xử lý ít nhất -> (2) Độ trễ Ping thấp nhất
        var sortedCandidates = geoFiltered
            .OrderBy(p => _proxyConnectionCounts.GetValueOrDefault(p.Id, 0))
            .ThenBy(p => p.PingMs ?? 9999)
            .ToList();

        // Lấy top 3 node tốt nhất và xáo trộn ngẫu nhiên để chống dồn tải (Anti-Dogpiling)
        int topCount = Math.Min(3, sortedCandidates.Count);
        var topCandidates = sortedCandidates.Take(topCount).OrderBy(_ => Guid.NewGuid()).ToList();
        var selected = topCandidates.FirstOrDefault() ?? sortedCandidates.FirstOrDefault() ?? _proxyManager.GetActiveVpnProxy();

        if (selected == null)
        {
            return null;
        }

        // 5. Lưu vào Sticky Session nếu được yêu cầu
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            int ttlMinutes = sessionMinutes.GetValueOrDefault(10);
            if (ttlMinutes <= 0) ttlMinutes = 10;

            var newEntry = new StickySessionEntry
            {
                SessionKey = sessionKey,
                ProxyId = selected.Id,
                TenantId = tenantId,
                Country = targetCountry,
                City = targetCity,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ttlMinutes),
                LastActiveAt = DateTime.UtcNow,
                ActiveConnections = 1
            };

            _stickySessions[sessionKey] = newEntry;
            _logger.LogInformation("[RotationEngine] Đã gắn Sticky Session '{Key}' vào Proxy {Host}:{Port} ({Country}) - TTL: {Min} phút",
                sessionKey, selected.Host, selected.Port, selected.Country, ttlMinutes);
        }

        _proxyConnectionCounts.AddOrUpdate(selected.Id, 1, (_, c) => c + 1);
        return selected;
    }

    public void ReportFailure(string proxyId, string? sessionKey = null)
    {
        if (!string.IsNullOrWhiteSpace(sessionKey))
        {
            _stickySessions.TryRemove(sessionKey, out _);
        }

        // Giảm connection count
        _proxyConnectionCounts.AddOrUpdate(proxyId, 0, (_, c) => Math.Max(0, c - 1));

        // Tạm thời gắn cờ kiểm tra lại
        var proxy = _proxyManager.GetById(proxyId);
        if (proxy != null)
        {
            _logger.LogWarning("[RotationEngine] Proxy {Host}:{Port} gặp lỗi kết nối. Đánh dấu để kiểm tra lại.", proxy.Host, proxy.Port);
        }
    }

    public void ReportSuccess(string proxyId)
    {
        _proxyConnectionCounts.AddOrUpdate(proxyId, 0, (_, c) => Math.Max(0, c - 1));
    }

    public IReadOnlyList<StickySessionEntry> GetActiveStickySessions()
    {
        CleanupExpiredSessions();
        return _stickySessions.Values.ToList();
    }

    public void CleanupExpiredSessions()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _stickySessions)
        {
            if (now > kvp.Value.ExpiresAt)
            {
                _stickySessions.TryRemove(kvp.Key, out _);
            }
        }
    }
}
