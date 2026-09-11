using System.Collections.Generic;
using System.Net;
using VpnSDK.DnsResolver.Resolvers;

namespace VpnSDK.DnsResolver.Factory;

internal class DnsResolverFactory : IDnsResolverFactory
{
	public IDnsResolver CreateLocalResolver(IEnumerable<IPAddress> localDnsAddresses)
	{
		return new LocalDnsResolver(localDnsAddresses);
	}

	public IDnsResolver CreateVpnResolver(IEnumerable<IPAddress> vpnDnsAddresses)
	{
		return new VpnDnsResolver(vpnDnsAddresses);
	}

	public IDnsResolver CreateFallbackResolver()
	{
		return new FallbackDnsResolver();
	}
}
