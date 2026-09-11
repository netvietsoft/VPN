namespace VpnSDK.TrafficOptimizer.DTO;

public class TrafficSpeedData
{
	public ulong Current { get; set; }

	internal ulong Limit { get; set; }

	internal ulong Maximum { get; set; }

	internal ulong MaxAverage { get; set; }
}
