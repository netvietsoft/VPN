namespace VpnSDK.TrafficOptimizer.DTO;

internal class SpeedStatistics
{
	public ulong Maximum { get; set; }

	public ulong MaxAverage { get; set; }

	public ulong Current { get; set; }

	public uint MaxAverageKBps()
	{
		return (uint)(MaxAverage / 1024);
	}

	public uint CurrentKBps()
	{
		return (uint)(Current / 1024);
	}
}
