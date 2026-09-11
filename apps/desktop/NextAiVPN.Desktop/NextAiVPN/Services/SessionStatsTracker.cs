using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Threading;
using NextAiVPN.Common;
using NextAiVPN.Streaming;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

internal class SessionStatsTracker : ISessionStatsTracker
{
	private readonly IAppLogger _logger;

	private readonly Func<ISDK> _sdkProvider;

	private readonly Func<SdkManager> _streamingSdkProvider;

	private readonly string _interfaceAppName;

	private readonly DispatcherTimer _timer = new DispatcherTimer();

	private readonly DispatcherTimer _streamingTimer = new DispatcherTimer();

	private NetworkInterface _networkInterface;

	private double _accumulativeDown;

	private double _accumulativeUp;

	private long _sessionAccumulativeDown;

	private long _sessionAccumulativeUp;

	private double _onResetValueDown;

	private double _onResetValueUp;

	private long _streamingDownloadedStartPointMb;

	private long _streamingUploadedStartPointMb;

	private long _gatewayStartBytesIn;

	private long _gatewayStartBytesOut;

	private DateTime _endTime;

	public DateTime StartTime { get; private set; }

	public TimeSpan TimeElapsed => _endTime.Subtract(StartTime);

	public string DownloadedUsageText { get; private set; } = "0 MB";

	public string UploadedUsageText { get; private set; } = "0 MB";

	public long DownloadedBytes { get; private set; }

	public long UploadedBytes { get; private set; }

	public event EventHandler Ticked;

	public event EventHandler UsageUpdated;

	public SessionStatsTracker(IAppSettingsHelper appSettingsHelper, IAppLogger logger, Func<ISDK> sdkProvider, Func<SdkManager> streamingSdkProvider)
	{
		if (appSettingsHelper == null)
		{
			throw new ArgumentNullException("appSettingsHelper");
		}
		_logger = logger;
		_sdkProvider = sdkProvider ?? throw new ArgumentNullException("sdkProvider");
		_streamingSdkProvider = streamingSdkProvider ?? throw new ArgumentNullException("streamingSdkProvider");
		_interfaceAppName = SystemInfo.AppName;
		_streamingTimer.Interval = TimeSpan.FromSeconds(1L);
		_streamingTimer.Tick += async delegate
		{
			try
			{
				await UpdateStreamingStatsAsync();
			}
			catch (Exception exception)
			{
				_logger?.Error(exception, ".ctor", "SessionStatsTracker.cs", 95);
			}
		};
	}

	public void StartSession()
	{
		if (!_timer.IsEnabled)
		{
			_timer.Tick -= Timer_Tick;
			ResetClock();
			_ = CaptureGatewayStartPointsAsync();
			_timer.Tick += Timer_Tick;
			_timer.Interval = TimeSpan.FromSeconds(1L);
			_timer.Start();
		}
	}

	private async Task CaptureGatewayStartPointsAsync()
	{
		try
		{
			(long gwIn, long gwOut, _, _) = await NextAiLocationService.GetGatewayTrafficStatsAsync();
			_gatewayStartBytesIn = gwIn;
			_gatewayStartBytesOut = gwOut;
		}
		catch { }
	}

	public void StopSession()
	{
		if (_timer.IsEnabled)
		{
			_timer.Stop();
			_timer.Tick -= Timer_Tick;
		}
	}

	public void StartStreamingTracking()
	{
		_streamingTimer.Start();
	}

	public void StopStreamingTracking()
	{
		_streamingTimer.Stop();
	}

	public async Task CaptureStreamingStartPoints()
	{
		(long, long) obj = await _streamingSdkProvider().GetStatisticDataBytes();
		long item = obj.Item1;
		long item2 = obj.Item2;
		_streamingDownloadedStartPointMb = ConvertBytesToMb(item2);
		_streamingUploadedStartPointMb = ConvertBytesToMb(item);
	}

	public void TrackNetworkInterface(NetworkInterface networkInterface)
	{
		_networkInterface = networkInterface;
	}

	public void ResetClock()
	{
		StartTime = DateTime.Now;
		_endTime = DateTime.Now;
	}

	public void SaveDataUsageOnDisconnect()
	{
		_onResetValueDown = 0.0;
		_onResetValueUp = 0.0;
		_accumulativeDown = ParseMbValue(DownloadedUsageText);
		_accumulativeUp = ParseMbValue(UploadedUsageText);
		_gatewayStartBytesIn = 0;
		_gatewayStartBytesOut = 0;
		if (_networkInterface != null && _networkInterface.Description.Contains("TAP"))
		{
			_sessionAccumulativeDown = _networkInterface.GetIPv4Statistics().BytesReceived;
			_sessionAccumulativeUp = _networkInterface.GetIPv4Statistics().BytesSent;
		}
	}

	public void ResetUsage(bool isConnected)
	{
		if (!isConnected)
		{
			DownloadedUsageText = "0 MB";
			UploadedUsageText = "0 MB";
			DownloadedBytes = 0L;
			UploadedBytes = 0L;
			_accumulativeDown = 0.0;
			_accumulativeUp = 0.0;
			_onResetValueDown = 0.0;
			_onResetValueUp = 0.0;
			_gatewayStartBytesIn = 0;
			_gatewayStartBytesOut = 0;
			UsageUpdated?.Invoke(this, EventArgs.Empty);
			return;
		}
		try
		{
			_onResetValueDown = ParseMbValue(DownloadedUsageText);
			_onResetValueUp = ParseMbValue(UploadedUsageText);
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "ResetUsage", "SessionStatsTracker.cs", 200);
		}
	}

	private async void Timer_Tick(object sender, EventArgs e)
	{
		try
		{
			await UpdateNextAiVpnStatsAsync();
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "Timer_Tick", "SessionStatsTracker.cs", 215);
		}
	}

	internal async Task UpdateNextAiVpnStatsAsync()
	{
		_endTime = DateTime.Now;
		Ticked?.Invoke(this, EventArgs.Empty);

		// [VI] 1. Lấy lưu lượng từ Universal Gateway port 10000 (Backend API port 6033)
		// [EN] 1. Retrieve traffic statistics from Universal Gateway port 10000 (Backend API port 6033)
		long sessionGwIn = 0;
		long sessionGwOut = 0;
		try
		{
			(long gwIn, long gwOut, _, _) = await NextAiLocationService.GetGatewayTrafficStatsAsync();
			if (_gatewayStartBytesIn == 0 && _gatewayStartBytesOut == 0 && (gwIn > 0 || gwOut > 0))
			{
				_gatewayStartBytesIn = gwIn;
				_gatewayStartBytesOut = gwOut;
			}
			sessionGwIn = Math.Max(0, gwIn - _gatewayStartBytesIn);
			sessionGwOut = Math.Max(0, gwOut - _gatewayStartBytesOut);
		}
		catch { }

		// [VI] 2. Lấy lưu lượng từ Windows TAP/WFP Adapter (nếu đang bật)
		// [EN] 2. Retrieve traffic from Windows TAP/WFP Adapter (if enabled)
		long tapIn = await ReadUsageAsync(received: true);
		long tapOut = await ReadUsageAsync(received: false);

		DownloadedBytes = sessionGwIn + tapIn;
		UploadedBytes = sessionGwOut + tapOut;

		double downMb = (double)DownloadedBytes / 1048576.0;
		double upMb = (double)UploadedBytes / 1048576.0;

		double totalDownMb = _accumulativeDown + downMb - _onResetValueDown;
		double totalUpMb = _accumulativeUp + upMb - _onResetValueUp;

		if (totalDownMb < 0) totalDownMb = 0;
		if (totalUpMb < 0) totalUpMb = 0;

		DownloadedUsageText = totalDownMb.ToString("0.##", CultureInfo.InvariantCulture) + " MB";
		UploadedUsageText = totalUpMb.ToString("0.##", CultureInfo.InvariantCulture) + " MB";

		UsageUpdated?.Invoke(this, EventArgs.Empty);
	}

	internal async Task UpdateStreamingStatsAsync()
	{
		_endTime = DateTime.Now;
		Ticked?.Invoke(this, EventArgs.Empty);
		(long, long) obj = await _streamingSdkProvider().GetStatisticDataBytes();
		long item = obj.Item1;
		long item2 = obj.Item2;
		bool flag = false;
		if (item2 != 0L)
		{
			DownloadedUsageText = ((double)item2 * 1E-06 - (double)_streamingDownloadedStartPointMb).ToString("0.##", CultureInfo.InvariantCulture) + " MB";
			flag = true;
		}
		if (item != 0L)
		{
			UploadedUsageText = ((double)item * 1E-06 - (double)_streamingUploadedStartPointMb).ToString("0.##", CultureInfo.InvariantCulture) + " MB";
			flag = true;
		}
		if (flag)
		{
			UsageUpdated?.Invoke(this, EventArgs.Empty);
		}
	}

	private Task<long> ReadUsageAsync(bool received)
	{
		return Task.Run(delegate
		{
			if (!_sdkProvider().IsConnected || _networkInterface == null)
			{
				return 0L;
			}
			try
			{
				if (!_networkInterface.Name.Contains(_interfaceAppName) && !_networkInterface.Description.Contains("NextAiVPN Windows Tap Adapter"))
				{
					return 0L;
				}
				IPv4InterfaceStatistics iPv4Statistics = _networkInterface.GetIPv4Statistics();
				long num = (received ? iPv4Statistics.BytesReceived : iPv4Statistics.BytesSent);
				if (_networkInterface.Description.Contains("TAP"))
				{
					num -= (received ? _sessionAccumulativeDown : _sessionAccumulativeUp);
				}
				return num;
			}
			catch (Exception exception)
			{
				_logger?.Error(exception, "ReadUsageAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\SessionStatsTracker.cs", 313);
				return 0L;
			}
		});
	}

	private static double ParseMbValue(string usageText)
	{
		int num = usageText.IndexOf(" MB", StringComparison.Ordinal);
		if (num < 0)
		{
			return 0.0;
		}
		return double.Parse(usageText.Substring(0, num), CultureInfo.InvariantCulture);
	}

	private static long ConvertBytesToMb(long bytes)
	{
		return (long)((double)bytes * 1E-06);
	}
}
