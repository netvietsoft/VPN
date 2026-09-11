using System.Collections.Generic;
using System.Net;

namespace VpnSDK.Common.Dns;

public class DnsResolutionResult
{
	public List<IPAddress>? IpAddresses { get; set; }

	public int TimeToLive { get; set; }

	public bool IsDnsResolutionAllowedFurther { get; set; }
}
