using System;
using System.Reflection;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using Serilog;

namespace NextAiVPN.Services.Persistence;

internal class ConfigurationLoggerHelper : IConfigurationLoggerHelper
{
	private readonly IOSVersionService _oSVersionService;

	private readonly ICredentialStore _credentialStore;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public ConfigurationLoggerHelper(IOSVersionService oSVersionService, ICredentialStore credentialStore, IAppSettingsHelper appSettingsHelper)
	{
		_oSVersionService = oSVersionService;
		_credentialStore = credentialStore;
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
	}

	public void LogAll()
	{
		VpnCredentials credentials = _credentialStore.GetCredentials();
		Log.ForContext<ConfigurationLoggerHelper>().Information("\n\n==============Configuration settings==============\n");
		Log.ForContext<ConfigurationLoggerHelper>().Information("UserName: {0}", (credentials == null) ? "Not authenticated" : credentials.VpnUsername);
		Log.ForContext<ConfigurationLoggerHelper>().Information("AccountType: {0}", _appSettingsHelper.GetValue("AccountType"));
		Log.ForContext<ConfigurationLoggerHelper>().Information("AppVersion: {0}", Assembly.GetExecutingAssembly());
		Log.ForContext<ConfigurationLoggerHelper>().Information("Windows: {0}", _oSVersionService.GetOSVersion());
		Log.ForContext<ConfigurationLoggerHelper>().Information("IsLogined: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("IsLoggedIn")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("ThemeAppearance: {0}", _appSettingsHelper.GetValue("ThemeAppearance"));
		Log.ForContext<ConfigurationLoggerHelper>().Information("SelectedVpnMode: {0}", _appSettingsHelper.GetValue("VpnType"));
		Log.ForContext<ConfigurationLoggerHelper>().Information("StreamingProtocol: {0}", _appSettingsHelper.GetValue("StreamingProtocol"));
		Log.ForContext<ConfigurationLoggerHelper>().Information("PrivateProtocol: {0}", _appSettingsHelper.GetValue("protocol"));
		Log.ForContext<ConfigurationLoggerHelper>().Information("ProtocolType: {0}", _appSettingsHelper.GetValue("protocolType"));
		Log.ForContext<ConfigurationLoggerHelper>().Information("KillSwitch: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("KillSwitch")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("AutoConnect: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("AutoConnect")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("Startup: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("Startup")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("Scramble: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("scramble")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("IsSplitTunnelingEnabled: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("IsSplitTunnelingEnabled")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("IsAdBlocker: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("IsAdBlocker")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("IsLanAllowed: {0}", StringToBoolConvert(_appSettingsHelper.GetValue(AppSettingsKeys.IsLanAllowed)));
		Log.ForContext<ConfigurationLoggerHelper>().Information("IsIPv6LeakProtection: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("IsIPv6LeakProtection")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("IsDnsLeakProtection: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("IsDnsLeakProtection")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("IsDnsMonitoring: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("IsDnsMonitoring")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("IsTrafficOptimizer: {0}", StringToBoolConvert(_appSettingsHelper.GetValue("IsTrafficOptimizer")));
		Log.ForContext<ConfigurationLoggerHelper>().Information("\n\n==============Configuration settings==============\n");
	}

	private static string StringToBoolConvert(string value)
	{
		try
		{
			return value.Equals("1") ? "true" : "false";
		}
		catch (Exception)
		{
			return "false";
		}
	}
}
