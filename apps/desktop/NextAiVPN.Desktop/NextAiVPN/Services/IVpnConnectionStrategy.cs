using System.Threading.Tasks;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

public interface IVpnConnectionStrategy
{
	VpnType Mode { get; }

	Task ConnectAsync(ILocation location);

	Task DisconnectAsync();
}
