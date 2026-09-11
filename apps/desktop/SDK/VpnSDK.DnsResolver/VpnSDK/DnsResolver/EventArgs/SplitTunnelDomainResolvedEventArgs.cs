using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace VpnSDK.DnsResolver.EventArgs;

public class SplitTunnelDomainResolvedEventArgs : System.EventArgs
{
	public string Domain { get; }

	public IPAddress[] IpAddresses { get; }

	public SplitTunnelDomainResolvedEventArgs(string domain, List<IPAddress> ipAddresses)
	{
		Domain = domain;
		IpAddresses = ipAddresses.Where((IPAddress ip) => ip != null).ToArray();
	}
}
