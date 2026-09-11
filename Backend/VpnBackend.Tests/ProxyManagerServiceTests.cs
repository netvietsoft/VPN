using System;
using System.Linq;
using System.Threading.Tasks;
using VpnResidentialHub.Services;
using Xunit;

namespace VpnBackend.Tests;

/// <summary>
/// [VI] Bộ kiểm thử đơn vị cho ProxyManagerService (Quản trị kho proxy và hạn mức 300MB/ngày)
/// [EN] Unit tests for ProxyManagerService (Proxy pool management, single active VPN, and quota accounting)
/// </summary>
public class ProxyManagerServiceTests
{
    private ProxyManagerService CreateService()
    {
        return new ProxyManagerService();
    }

    [Fact]
    public void EnsureSingleActiveVpn_ExactlyOneProxyIsActiveVpn()
    {
        // [VI] Đảm bảo luôn luôn chỉ có đúng 1 proxy được đánh dấu IsActiveVpn = true
        // [EN] Verify that exactly one proxy is marked as IsActiveVpn = true
        var service = CreateService();
        var all = service.GetAll();

        var activeList = all.Where(p => p.IsActiveVpn).ToList();
        Assert.Single(activeList);

        var activeVpn = service.GetActiveVpnProxy();
        Assert.NotNull(activeVpn);
        Assert.Equal(activeList[0].Id, activeVpn.Id);
    }

    [Fact]
    public void SelectActiveVpn_SwitchesActiveVpnProperly()
    {
        // [VI] Kiểm tra chuyển đổi proxy active cho Desktop VPN Client
        // [EN] Verify switching active VPN proxy updates flags correctly
        var service = CreateService();
        var all = service.GetAll();
        Assert.True(all.Count >= 2);

        var first = all[0];
        var second = all[1];

        bool switched1 = service.SelectActiveVpn(first.Id);
        Assert.True(switched1);
        Assert.Equal(first.Id, service.GetActiveVpnProxy()?.Id);

        bool switched2 = service.SelectActiveVpn(second.Id);
        Assert.True(switched2);
        Assert.Equal(second.Id, service.GetActiveVpnProxy()?.Id);

        // Đảm bảo sau khi chuyển đổi vẫn duy nhất 1 proxy active
        var activeList = service.GetAll().Where(p => p.IsActiveVpn).ToList();
        Assert.Single(activeList);
    }

    [Fact]
    public void RecordBandwidthUsage_ExceedsDailyQuota_MarksExhaustedToday()
    {
        // [VI] Kiểm tra khi dùng quá 300MB/ngày, proxy bị đánh dấu cạn kiệt (IsExhaustedToday)
        // [EN] Verify exceeding daily 300MB quota marks proxy as IsExhaustedToday
        var service = CreateService();
        var proxy = new UpstreamProxy
        {
            Host = "10.200.1.1",
            Port = 1080,
            Type = "socks5",
            DailyQuotaBytes = 100 * 1024, // 100 KB test quota
            UsedDailyBytes = 0,
            IsExhaustedToday = false
        };
        var added = service.Add(proxy);

        // Ghi nhận lưu lượng vượt ngưỡng
        service.RecordBandwidthUsage(added.Id, 150 * 1024);

        var refreshed = service.GetById(added.Id);
        Assert.NotNull(refreshed);
        Assert.True(refreshed.IsExhaustedToday);
        Assert.True(refreshed.UsedDailyBytes >= 150 * 1024);

        // Clean up
        service.Delete(added.Id);
    }

    [Fact]
    public void ResetDailyQuotas_ResetsExhaustedStatusAndUsedBytes()
    {
        // [VI] Kiểm tra cơ chế đặt lại hạn mức ngày (Daily Quota Reset)
        // [EN] Verify daily quota reset clears exhausted status and counters
        var service = CreateService();
        var proxy = new UpstreamProxy
        {
            Host = "10.200.1.2",
            Port = 1080,
            Type = "socks5",
            DailyQuotaBytes = 300L * 1024 * 1024,
            UsedDailyBytes = 350L * 1024 * 1024,
            IsExhaustedToday = true,
            DailyResetDate = "2020-01-01" // Ngày cũ trong quá khứ
        };
        var added = service.Add(proxy);

        int resetCount = service.ResetDailyQuotas();
        Assert.True(resetCount >= 1);

        var refreshed = service.GetById(added.Id);
        Assert.NotNull(refreshed);
        Assert.False(refreshed.IsExhaustedToday);
        Assert.Equal(0, refreshed.UsedDailyBytes);
        Assert.Equal(DateTime.UtcNow.ToString("yyyy-MM-dd"), refreshed.DailyResetDate);

        // Clean up
        service.Delete(added.Id);
    }

    [Fact]
    public void RecordBandwidthUsage_ConcurrentCalls_ThreadSafeAccumulation()
    {
        // [VI] Kiểm tra tính an toàn đa luồng khi ghi nhận băng thông đồng thời
        // [EN] Verify thread-safe quota accumulation under concurrent traffic
        var service = CreateService();
        var proxy = new UpstreamProxy
        {
            Host = "10.200.1.3",
            Port = 1080,
            Type = "socks5",
            DailyQuotaBytes = 10L * 1024 * 1024 * 1024
        };
        var added = service.Add(proxy);

        int threadCount = 20;
        long bytesPerThread = 10000;

        Parallel.For(0, threadCount, _ =>
        {
            service.RecordBandwidthUsage(added.Id, bytesPerThread);
        });

        var refreshed = service.GetById(added.Id);
        Assert.NotNull(refreshed);
        Assert.Equal(threadCount * bytesPerThread, refreshed.UsedDailyBytes);

        // Clean up
        service.Delete(added.Id);
    }
}
