using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

internal class VpnModesService : IVpnModesService
{
	private readonly IVpnTypeDefiner _vpnTypeDefiner;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public VpnModesService(IVpnTypeDefiner vpnTypeDefiner, IAppSettingsHelper appSettingsHelper)
	{
		_vpnTypeDefiner = vpnTypeDefiner;
		_appSettingsHelper = appSettingsHelper;
	}

	public VpnType GetVpnMode()
	{
		return _vpnTypeDefiner.DefineVpnType();
	}

	public void SetVpnMode(VpnType vpnType)
	{
		_appSettingsHelper.SetValue("VpnType", vpnType.ToString());
	}
}
