using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NextAiVPN.Enums;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

internal class TapDriverInstaller : ITapDriverInstaller
{
	private readonly IAnalyticsService _analyticsService;

	private readonly IBugsnagService _bugsnagService;

	private readonly Func<ISDK> _sdkProvider;

	public event Action<TapDriverInstallStatus> InstallStatusChanged;

	public TapDriverInstaller(IAnalyticsService analyticsService, IBugsnagService bugsnagService, Func<ISDK> sdkProvider)
	{
		_analyticsService = analyticsService ?? throw new ArgumentNullException("analyticsService");
		_bugsnagService = bugsnagService ?? throw new ArgumentNullException("bugsnagService");
		_sdkProvider = sdkProvider ?? throw new ArgumentNullException("sdkProvider");
	}

	public bool IsTapDriverInstalled()
	{
		try
		{
			if (NetworkInterface.GetAllNetworkInterfaces().Any((NetworkInterface x) => x.Description.Contains("NextAiVPN Windows Tap Adapter")))
			{
				return true;
			}
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify("[IsTapDriverInstalled] - " + ex.Message);
		}
		return false;
	}

	public async Task InstallTapDriver()
	{
		_analyticsService.SendNotification("TAPDriver Install");
		InstallStatusChanged?.Invoke(TapDriverInstallStatus.Installing);
		if (await _sdkProvider().InstallTapDriver() == DriverInstallResult.Success)
		{
			InstallStatusChanged?.Invoke(TapDriverInstallStatus.Succeeded);
			return;
		}
		InstallStatusChanged?.Invoke(TapDriverInstallStatus.Failed);
		_bugsnagService.Notify("[TAPDriverInstall] - Unable to install TAP Driver");
	}
}
