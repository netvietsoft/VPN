using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NextAiVPN.Services;

public interface ISessionStatsTracker
{
	DateTime StartTime { get; }

	TimeSpan TimeElapsed { get; }

	string DownloadedUsageText { get; }

	string UploadedUsageText { get; }

	long DownloadedBytes { get; }

	long UploadedBytes { get; }

	event EventHandler Ticked;

	event EventHandler UsageUpdated;

	void StartSession();

	void StopSession();

	void StartStreamingTracking();

	void StopStreamingTracking();

	Task CaptureStreamingStartPoints();

	void TrackNetworkInterface(NetworkInterface networkInterface);

	void ResetClock();

	void SaveDataUsageOnDisconnect();

	void ResetUsage(bool isConnected);
}
