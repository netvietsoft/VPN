namespace VpnSDK.TrafficOptimizer;

internal class Constants
{
	internal class ErrorCodes
	{
		public const int AlreadyStarted = 1;

		public const int NotConfigured = 2;
	}

	public const ulong Threshold = 131072uL;

	public const uint HoldTime = 5000u;

	public const ulong MinimumLimit = 131072uL;

	public const int RefreshTimeInMs = 1000;

	public const int MaxBandwidth = 100;

	public const int PrioritizedAppsMinBandwidth = 0;

	public const int PrioritizedAppsMaxBandwidth = 100;

	public const int PrioritizedAppsBandwidth = 80;

	public const int OtherAppsBandwidth = 20;

	public const string IpAddressMask = "255.0.0.0";

	public const string ServiceHostPath = "C:\\Windows\\System32\\svchost.exe";

	public const int RefreshStatisticsInterval = 2000;

	public const int MaxPrioritizedApps = 5;
}
