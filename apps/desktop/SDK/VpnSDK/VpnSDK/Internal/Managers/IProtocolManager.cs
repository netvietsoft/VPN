using System;
using System.Threading;
using System.Threading.Tasks;
using VpnSDK.Common.Settings;
using VpnSDK.Interfaces;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Internal.Managers;

internal interface IProtocolManager : IDisposable
{
	bool IsActive { get; }

	Action UnexpectedDisconnect { get; set; }

	Task Connect(Server server, IConnectionConfiguration connectionConfiguration, IUser user, SplitTunnelClientSettings splitTunnelClientSettings, DnsSettings dnsSettings, CancellationToken token = default(CancellationToken));

	Task Disconnect();

	Task DisposeAsync();
}
