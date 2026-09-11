using System.Threading.Tasks;

namespace NextAiVPN.Streaming;

public interface ISplitTunnelRoutingService
{
	Task AddDomainsToBypassVpnAsync();

	Task AddDomainsToSplitTunnel();

	Task RemoveAllBypassRoutesAsync(string vpnInterfaceAlias);

	Task RemoveAllBypassRoutesAsync();

	Task RemoveBypassRoutesAsync();

	Task<bool> IsSplitTunnelingEnabledAsync();
}
