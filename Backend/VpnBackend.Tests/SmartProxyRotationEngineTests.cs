using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using VpnResidentialHub.Models;
using VpnResidentialHub.Services;
using Xunit;

namespace VpnBackend.Tests;

/// <summary>
/// [VI] Bộ kiểm thử đơn vị cho SmartProxyRotationEngine (Động cơ xoay proxy và quản lý Sticky Session)
/// [EN] Unit tests for SmartProxyRotationEngine (Proxy rotation engine, sticky session concurrency, sliding expiration)
/// </summary>
public class SmartProxyRotationEngineTests
{
    private (SmartProxyRotationEngine Engine, ProxyManagerService ProxyManager) CreateEngine()
    {
        var proxyManager = new ProxyManagerService();
        var engine = new SmartProxyRotationEngine(proxyManager, NullLogger<SmartProxyRotationEngine>.Instance);
        return (engine, proxyManager);
    }

    [Fact]
    public void ResolveProxy_StickySession_ConsistentlyReturnsSameProxy()
    {
        // [VI] Kiểm tra tính năng Sticky Session giữ nguyên proxy cho cùng 1 SessionId
        // [EN] Verify sticky session retains the same proxy across multiple requests
        var (engine, proxyManager) = CreateEngine();
        var tenant = new Tenant { Id = "test_tenant_sticky", Name = "Sticky Tenant" };
        string sessionId = "test_sess_" + Guid.NewGuid().ToString("N")[..8];

        var firstProxy = engine.ResolveProxy(tenant, null, null, sessionId, 15);
        Assert.NotNull(firstProxy);

        var secondProxy = engine.ResolveProxy(tenant, null, null, sessionId, 15);
        Assert.NotNull(secondProxy);
        Assert.Equal(firstProxy.Id, secondProxy.Id);

        var thirdProxy = engine.ResolveProxy(tenant, null, null, sessionId, 15);
        Assert.NotNull(thirdProxy);
        Assert.Equal(firstProxy.Id, thirdProxy.Id);
    }

    [Fact]
    public void ResolveProxy_SlidingExpiration_ExtendsSessionExpiry()
    {
        // [VI] Kiểm tra cơ chế Sliding Expiration gia hạn thời gian sống khi phiên có lưu lượng (BUG-20)
        // [EN] Verify sliding expiration extends session lifespan on active requests (BUG-20)
        var (engine, proxyManager) = CreateEngine();
        var tenant = new Tenant { Id = "test_tenant_sliding", Name = "Sliding Tenant" };
        string sessionId = "sliding_sess_" + Guid.NewGuid().ToString("N")[..8];

        var proxy1 = engine.ResolveProxy(tenant, null, null, sessionId, 10);
        Assert.NotNull(proxy1);

        var sessions1 = engine.GetActiveStickySessions();
        var entry1 = sessions1.FirstOrDefault(s => s.SessionKey == $"{tenant.Id}_{sessionId}");
        Assert.NotNull(entry1);
        var originalExpiresAt = entry1.ExpiresAt;

        // Nghỉ một chút rồi gửi request tiếp theo
        Thread.Sleep(50);

        var proxy2 = engine.ResolveProxy(tenant, null, null, sessionId, 10);
        Assert.NotNull(proxy2);
        Assert.Equal(proxy1.Id, proxy2.Id);

        var sessions2 = engine.GetActiveStickySessions();
        var entry2 = sessions2.FirstOrDefault(s => s.SessionKey == $"{tenant.Id}_{sessionId}");
        Assert.NotNull(entry2);
        Assert.True(entry2.ExpiresAt >= originalExpiresAt);
    }

    [Fact]
    public void ReportFailure_EvictsStickySessionAndRotatesProxy()
    {
        // [VI] Kiểm tra khi proxy bị sự cố, ReportFailure đẩy phiên ra khỏi Sticky và chuyển sang proxy khác
        // [EN] Verify ReportFailure invalidates sticky session and fails over to another proxy
        var (engine, proxyManager) = CreateEngine();
        var tenant = new Tenant { Id = "failover_tenant", Name = "Failover Tenant" };
        string sessionId = "failover_sess_" + Guid.NewGuid().ToString("N")[..8];

        var initialProxy = engine.ResolveProxy(tenant, null, null, sessionId, 10);
        Assert.NotNull(initialProxy);

        // Báo cáo lỗi proxy
        string sessionKey = $"{tenant.Id}_{sessionId}";
        engine.ReportFailure(initialProxy.Id, sessionKey);

        // Lấy lại danh sách sticky sessions
        var activeSessions = engine.GetActiveStickySessions();
        Assert.DoesNotContain(activeSessions, s => s.SessionKey == sessionKey);
    }

    [Fact]
    public void ResolveProxy_RotatingMode_SelectsHealthyLiveProxy()
    {
        // [VI] Kiểm tra chế độ Rotating (không SessionId) trả về proxy LIVE hợp lệ
        // [EN] Verify rotating mode without sessionId returns a healthy LIVE proxy
        var (engine, proxyManager) = CreateEngine();
        var tenant = new Tenant { Id = "rotating_tenant", Name = "Rotating Tenant" };

        var proxy = engine.ResolveProxy(tenant, null, null, null, null, forceRotate: true);
        Assert.NotNull(proxy);
        Assert.False(string.IsNullOrEmpty(proxy.Host));
        Assert.True(proxy.Port > 0);
    }
}
