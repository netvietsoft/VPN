using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VpnResidentialHub.Services;

/// <summary>
/// [VI] Dịch vụ nền tự động đo kiểm ping định kỳ 10 phút/lần cho toàn bộ kho proxy
/// [EN] Background service that automatically health-checks and pings all proxies every 10 minutes
/// </summary>
public class ProxyHealthCheckBackgroundService : BackgroundService
{
    private readonly IProxyManagerService _proxyManager;
    private readonly ILogger<ProxyHealthCheckBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(10);
    private readonly SemaphoreSlim _execLock = new(1, 1);

    public DateTime? LastRunTime { get; private set; }
    public DateTime NextRunTime { get; private set; }
    public bool IsRunning { get; private set; } = false;
    public object? LastSummary { get; private set; }

    public ProxyHealthCheckBackgroundService(
        IProxyManagerService proxyManager,
        ILogger<ProxyHealthCheckBackgroundService> logger)
    {
        _proxyManager = proxyManager;
        _logger = logger;
        NextRunTime = DateTime.UtcNow.Add(_interval);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[HealthCheckService] Dịch vụ tự động đo kiểm Ping định kỳ 10 phút đã khởi chạy. Lần chạy đầu tiên: {Time}", NextRunTime);

        // Chờ 30 giây sau khi hệ thống khởi động để chạy vòng kiểm tra đầu tiên
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            if (!stoppingToken.IsCancellationRequested)
            {
                await RunHealthCheckAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            NextRunTime = DateTime.UtcNow.Add(_interval);
            try
            {
                await Task.Delay(_interval, stoppingToken);
                await RunHealthCheckAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HealthCheckService] Lỗi trong chu kỳ kiểm tra sức khỏe proxy");
            }
        }
    }

    /// <summary>
    /// [VI] Thực thi vòng kiểm tra sức khỏe toàn diện cho tất cả proxy trong kho
    /// [EN] Runs comprehensive health check for all proxies in pool
    /// </summary>
    public async Task<object> RunHealthCheckAsync(CancellationToken ct = default)
    {
        if (!await _execLock.WaitAsync(0, ct))
        {
            return new
            {
                Success = false,
                Message = "Đang có một tiến trình đo ping đang chạy. Vui lòng chờ trong giây lát.",
                IsRunning = true
            };
        }

        try
        {
            IsRunning = true;
            var sw = Stopwatch.StartNew();
            var all = _proxyManager.GetAll();
            // [VI] Tự động kiểm tra và reset hạn mức ngày (Daily Quota) khi sang ngày mới
            // [EN] Automatically reset daily quotas when UTC date rolls over
            int resetCount = _proxyManager.ResetDailyQuotas();
            if (resetCount > 0)
            {
                _logger.LogInformation("[HealthCheckService] Đã tự động reset hạn mức 300MB/ngày cho {Count} proxy.", resetCount);
            }

            await _proxyManager.TestAllAsync();

            sw.Stop();
            LastRunTime = DateTime.UtcNow;
            NextRunTime = DateTime.UtcNow.Add(_interval);

            var updatedList = _proxyManager.GetAll();
            int liveCount = updatedList.Count(p => p.Status.Equals("LIVE", StringComparison.OrdinalIgnoreCase));
            int offlineCount = updatedList.Count(p => p.Status.Equals("OFFLINE", StringComparison.OrdinalIgnoreCase));
            int uncheckedCount = updatedList.Count - liveCount - offlineCount;

            var summary = new
            {
                Success = true,
                TotalChecked = updatedList.Count,
                Live = liveCount,
                Offline = offlineCount,
                Unchecked = uncheckedCount,
                DurationMs = sw.ElapsedMilliseconds,
                LastChecked = LastRunTime,
                NextCheck = NextRunTime,
                Message = $"Hoàn thành kiểm tra {updatedList.Count} proxy ({liveCount} LIVE, {offlineCount} OFFLINE) trong {sw.ElapsedMilliseconds}ms."
            };

            LastSummary = summary;
            _logger.LogInformation("[HealthCheckService] {Message}", summary.Message);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HealthCheckService] Lỗi khi thực thi RunHealthCheckAsync");
            return new
            {
                Success = false,
                Error = ex.Message
            };
        }
        finally
        {
            IsRunning = false;
            _execLock.Release();
        }
    }

    public object GetStatus()
    {
        var secondsUntilNext = (int)Math.Max(0, (NextRunTime - DateTime.UtcNow).TotalSeconds);
        return new
        {
            IntervalMinutes = 10,
            IsRunning,
            LastRunTime,
            NextRunTime,
            SecondsUntilNext = secondsUntilNext,
            LastSummary
        };
    }
}
