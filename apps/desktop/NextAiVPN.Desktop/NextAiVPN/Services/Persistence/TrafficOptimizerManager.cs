using System;
using System.Linq;
using NextAiVPN.Common;
using VpnSDK.TrafficOptimizer.DTO;

namespace NextAiVPN.Services.Persistence;

internal class TrafficOptimizerManager : ITrafficOptimizerManager
{
	private readonly SDKMonitor _sdk;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public TrafficOptimizerManager(SDKMonitor sdk, IAppSettingsHelper appSettingsHelper)
	{
		_sdk = sdk;
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
	}

	public bool IsTrafficOptimizerEnabled()
	{
		return _appSettingsHelper.GetValue("IsTrafficOptimizer").Equals("1");
	}

	public void SetApps()
	{
		string value = _appSettingsHelper.GetValue("TrafficOptimizerAppList");
		if (!string.IsNullOrEmpty(value))
		{
			TrafficOptimizerConfig trafficOptimizerConfig = new TrafficOptimizerConfig();
			trafficOptimizerConfig.PrioritizedApps = value.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
			TrafficOptimizerConfig trafficOptimizerConfig2 = trafficOptimizerConfig;
			_sdk.NextAiVpnSdkManager.SetTrafficOptimizerConfig(trafficOptimizerConfig2);
		}
	}
}
