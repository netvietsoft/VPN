using System.Collections.Generic;
using System.Net;

namespace VpnSDK.DnsResolver.Resolvers;

internal class VpnDnsResolver : DnsResolverBase
{
	internal VpnDnsResolver(IEnumerable<IPAddress> vpnDnsAddresses)
		: base(vpnDnsAddresses)
	{
	}
}
