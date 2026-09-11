using System.Net;

namespace VpnSDK.DnsResolver.Resolvers;

internal class FallbackDnsResolver : DnsResolverBase
{
	private static readonly IPAddress[] FallbackIpAddresses = new IPAddress[2]
	{
		IPAddress.Parse("8.8.8.8"),
		IPAddress.Parse("8.8.4.4")
	};

	internal FallbackDnsResolver()
		: base(FallbackIpAddresses)
	{
	}
}
