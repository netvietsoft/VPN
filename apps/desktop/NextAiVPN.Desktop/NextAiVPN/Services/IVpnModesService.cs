using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

public interface IVpnModesService
{
	VpnType GetVpnMode();

	void SetVpnMode(VpnType vpnType);
}
