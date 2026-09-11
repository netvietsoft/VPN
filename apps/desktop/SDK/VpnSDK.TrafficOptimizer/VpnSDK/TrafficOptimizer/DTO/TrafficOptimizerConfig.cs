using VpnSDK.Core.Interfaces;

namespace VpnSDK.TrafficOptimizer.DTO;

public class TrafficOptimizerConfig : IConfig
{
	public TrafficMode Mode => TrafficMode.ApplicationPriority;

	public bool IsEnabled { get; set; }

	public string[] PrioritizedApps { get; set; }

	public int PrioritizedAppsMaxBandwidth { get; set; } = 80;
}
