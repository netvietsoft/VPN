using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class VpnTypeDefiner : IVpnTypeDefiner
{
	private readonly IAppSettingsHelper _appSettingsHelper;

	public VpnTypeDefiner(IAppSettingsHelper appSettingsHelper)
	{
		_appSettingsHelper = appSettingsHelper;
	}

	public VpnType DefineVpnType()
	{
		string text = _appSettingsHelper.GetValue("VpnType").ToLower();
		if (!(text == "nextaivpn"))
		{
			if (text == "streaming")
			{
				return VpnType.Streaming;
			}
			return VpnType.NextAiVPN;
		}
		return VpnType.NextAiVPN;
	}
}
