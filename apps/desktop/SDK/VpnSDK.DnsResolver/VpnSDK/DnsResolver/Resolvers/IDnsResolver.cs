using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace VpnSDK.DnsResolver.Resolvers;

internal interface IDnsResolver : IDisposable
{
	Task<List<T>> ResolveAsync<T>(DomainName domainName, RecordType recordType, RecordClass recordClass, CancellationToken cancellationToken) where T : DnsRecordBase;

	void ClearCache();
}
