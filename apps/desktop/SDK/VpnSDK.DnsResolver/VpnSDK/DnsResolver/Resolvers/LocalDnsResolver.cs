using System.Collections.Generic;
using System.Net;

namespace VpnSDK.DnsResolver.Resolvers;

internal class LocalDnsResolver : DnsResolverBase
{
	internal LocalDnsResolver(IEnumerable<IPAddress> localDnsAddresses)
		: base(localDnsAddresses)
	{
	}
}
