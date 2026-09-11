using System;
using VpnSDK;
using VpnSDK.DTO;
using VpnSDK.Interfaces;
using VpnSDK.Private.OpenVpn.Enums;

namespace NextAiVPN;

public static class SDKInstance
{
	private static ISDK _instance;

	internal static ISDK GetInstance()
	{
		if (_instance != null && _instance.IsDisposed)
		{
			_instance = CreateInstance();
			return _instance;
		}
		return _instance ?? (_instance = CreateInstance());
	}

	private static ISDK CreateInstance()
	{
		return new SDKBuilder<ISDK>().SetApiKey("933f67de383fb9987d8c11216bc94da1").SetAuthenticationToken("@nextaitechnology").SetApplicationName(SystemInfo.AppName)
			.SetOpenVpnConfiguration(new OpenVpnConfiguration
			{
				PreferredTapAdapter = (OpenVpnTapAdapter)1, // [VI] NextAiVPN Adapter mapping
				TapDeviceFriendlyName = "NextAiVPN Windows Tap Adapter"
			})
			.SetServerListCache(TimeSpan.FromDays(1))
			.Create();
	}
}
