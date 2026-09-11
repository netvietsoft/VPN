using System.Collections.Generic;
using System.Net;
using VpnSDK.DnsResolver.Resolvers;

namespace VpnSDK.DnsResolver.Factory;

internal interface IDnsResolverFactory
{
	IDnsResolver CreateLocalResolver(IEnumerable<IPAddress> localDnsAddresses);

	IDnsResolver CreateVpnResolver(IEnumerable<IPAddress> vpnDnsAddresses);

	IDnsResolver CreateFallbackResolver();
}
