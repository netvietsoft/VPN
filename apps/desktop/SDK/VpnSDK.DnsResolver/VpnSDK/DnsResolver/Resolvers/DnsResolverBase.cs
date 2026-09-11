using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace VpnSDK.DnsResolver.Resolvers;

internal abstract class DnsResolverBase : IDnsResolver, IDisposable
{
	private DnsClient? _dnsClient;

	private bool _disposed;

	protected readonly DnsStubResolver Resolver;

	protected DnsResolverBase(IEnumerable<IPAddress> dnsAddresses)
	{
		CreateDnsClient(dnsAddresses);
		Resolver = new DnsStubResolver(_dnsClient);
	}

	private void CreateDnsClient(IEnumerable<IPAddress> dnsAddresses)
	{
		IClientTransport[] transports = new IClientTransport[2]
		{
			new UdpClientTransport(),
			new TcpClientTransport()
		};
		_dnsClient = new DnsClient(dnsAddresses, transports, disposeTransport: true)
		{
			IsResponseValidationEnabled = true,
			Is0x20ValidationEnabled = true
		};
	}

	public async Task<List<T>> ResolveAsync<T>(DomainName domainName, RecordType recordType, RecordClass recordClass, CancellationToken cancellationToken) where T : DnsRecordBase
	{
		return await Resolver.ResolveAsync<T>(domainName, recordType, recordClass, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public void ClearCache()
	{
		Resolver.ClearCache();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				((IDisposable)_dnsClient)?.Dispose();
			}
			_disposed = true;
		}
	}
}
