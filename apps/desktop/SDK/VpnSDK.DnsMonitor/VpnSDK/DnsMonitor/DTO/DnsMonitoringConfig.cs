using VpnSDK.Core.Interfaces;

namespace VpnSDK.DnsMonitor.DTO;

public class DnsMonitoringConfig : IConfig
{
	public bool IsEnabled { get; set; }
}
