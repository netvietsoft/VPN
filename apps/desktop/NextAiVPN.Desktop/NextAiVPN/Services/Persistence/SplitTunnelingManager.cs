using System;
using System.Linq;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using VpnSDK.Common.Settings;

namespace NextAiVPN.Services.Persistence;

internal class SplitTunnelingManager : ISplitTunnelingManager
{
	private readonly ISplitTunnelingRepository _splitTunnelingRepository;

	private readonly SDKMonitor _sdkMonitor;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	public SplitTunnelingManager(ISplitTunnelingRepository splitTunnelingRepository, SDKMonitor sdkMonitor, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_splitTunnelingRepository = splitTunnelingRepository;
		_sdkMonitor = sdkMonitor;
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
	}

	public bool IsSplitTunnelingEnabled()
	{
		return _appSettingsHelper.GetValue("IsSplitTunnelingEnabled").Equals("1");
	}

	public void SetApps()
	{
		try
		{
			_sdkMonitor.NextAiVpnSdkManager.SplitTunnelAllowedApps = (from appPath in _splitTunnelingRepository.GetAppList()
				select new SplitTunnelingApp().Parse(appPath) into app
				select new SplitTunnelApp(app.Name, app.Path)).ToList();
		}
		catch (Exception ex)
		{
			_logger?.Error("SetApps() - " + ex.Message, "SetApps", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SplitTunnelingManager.cs", 42);
		}
	}

	public void SetHostnamesAndIps()
	{
		try
		{
			_sdkMonitor.NextAiVpnSdkManager.SplitTunnelAllowedDomains = (from domain in _splitTunnelingRepository.GetHostnamesAndIpsListTuple()
				select new SplitTunnelDomain(domain.Domain, domain.isDomainIncluded)).ToList();
		}
		catch (Exception ex)
		{
			_logger?.Error("SetHostnamesAndIps() - " + ex.Message, "SetHostnamesAndIps", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SplitTunnelingManager.cs", 55);
		}
	}
}
