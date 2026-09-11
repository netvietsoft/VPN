using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using VpnSDK.DnsResolver.Enum;
using VpnSDK.DnsResolver.Factory;

namespace VpnSDK.DnsResolver.Resolvers;

internal class DnsResolverSelector
{
	private readonly Dictionary<DnsResolverType, IDnsResolver> _resolvers;

	private readonly Func<DomainName, DnsResolverType> _resolverSelector;

	internal DnsResolverSelector(IDnsResolverFactory resolverFactory, IEnumerable<IPAddress> localDnsAddresses, IEnumerable<IPAddress> vpnDnsAddresses, Func<DomainName, DnsResolverType> resolverSelector)
	{
		_resolvers = new Dictionary<DnsResolverType, IDnsResolver>
		{
			{
				DnsResolverType.Local,
				resolverFactory.CreateLocalResolver(localDnsAddresses)
			},
			{
				DnsResolverType.Vpn,
				resolverFactory.CreateVpnResolver(vpnDnsAddresses)
			},
			{
				DnsResolverType.None,
				resolverFactory.CreateFallbackResolver()
			}
		};
		_resolverSelector = resolverSelector;
	}

	internal async Task<List<T>?> ResolveAsync<T>(DomainName domainName, RecordType recordType, RecordClass recordClass, CancellationToken cancellationToken) where T : DnsRecordBase
	{
		DnsResolverType key = _resolverSelector(domainName);
		return await _resolvers[key].ResolveAsync<T>(domainName, recordType, recordClass, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal void Dispose()
	{
		foreach (IDnsResolver value in _resolvers.Values)
		{
			value.ClearCache();
			value.Dispose();
		}
	}
}
