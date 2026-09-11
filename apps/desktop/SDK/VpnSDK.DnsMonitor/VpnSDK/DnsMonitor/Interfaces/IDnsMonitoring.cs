using System;
using VpnSDK.Core.Interfaces;
using VpnSDK.DnsMonitor.DTO;

namespace VpnSDK.DnsMonitor.Interfaces;

internal interface IDnsMonitoring : IFeature
{
	event Action<DnsMonitoringArgs> DnsMonitoringUpdate;

	event Action<DnsMonitoringConfig> DnsMonitoringStarted;

	event Action<DnsMonitoringConfig> DnsMonitoringStopped;

	void AddException(string[] domains);
}
