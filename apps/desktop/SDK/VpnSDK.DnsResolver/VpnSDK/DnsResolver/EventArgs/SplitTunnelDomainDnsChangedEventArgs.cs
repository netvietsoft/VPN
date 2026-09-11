using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace VpnSDK.DnsResolver.EventArgs;

public class SplitTunnelDomainDnsChangedEventArgs
{
	public string Domain { get; }

	public IPAddress[] OldIpAddresses { get; }

	public IPAddress[] NewIpAddresses { get; }

	public SplitTunnelDomainDnsChangedEventArgs(string domain, List<IPAddress> oldIpAddresses, List<IPAddress> newIpAddresses)
	{
		Domain = domain;
		OldIpAddresses = oldIpAddresses.Where((IPAddress ip) => ip != null).ToArray();
		NewIpAddresses = newIpAddresses.Where((IPAddress ip) => ip != null).ToArray();
	}
}
