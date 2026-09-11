using System;
using System.Collections.Generic;

namespace VpnSDK.DnsMonitor.DTO;

public class DnsMonitoringArgs : EventArgs
{
	public List<string> DomainNames { get; set; }
}
