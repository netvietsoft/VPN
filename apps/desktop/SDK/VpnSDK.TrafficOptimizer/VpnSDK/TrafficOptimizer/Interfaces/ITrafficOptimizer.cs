using System;
using VpnSDK.Core.Interfaces;
using VpnSDK.TrafficOptimizer.DTO;

namespace VpnSDK.TrafficOptimizer.Interfaces;

internal interface ITrafficOptimizer : IFeature
{
	event Action<TrafficOptimizerArgs> TrafficUpdate;

	void RaiseTrafficUpdateEvent(TrafficOptimizerArgs args);
}
