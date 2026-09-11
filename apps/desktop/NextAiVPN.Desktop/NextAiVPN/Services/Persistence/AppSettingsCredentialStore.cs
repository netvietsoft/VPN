using System;
using NextAiVPN.Common;
using NextAiVPN.Entities;

namespace NextAiVPN.Services.Persistence;

internal class AppSettingsCredentialStore : ICredentialStore
{
	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	public AppSettingsCredentialStore(IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
	}

	public void SaveCredentials(VpnCredentials credentials)
	{
		_appSettingsHelper.SetValue(AppSettingsKeys.VpnUsername, credentials.VpnUsername);
		_appSettingsHelper.SetValue(AppSettingsKeys.VpnPassword, credentials.VpnPassword);
	}

	public VpnCredentials GetCredentials()
	{
		string value = _appSettingsHelper.GetValue(AppSettingsKeys.VpnUsername);
		if (string.IsNullOrEmpty(value))
		{
			_logger?.Warning("Username is empty", "GetCredentials", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AppSettingsCredentialStore.cs", 48);
			return null;
		}
		string value2 = _appSettingsHelper.GetValue(AppSettingsKeys.VpnPassword);
		if (string.IsNullOrEmpty(value2))
		{
			_logger?.Warning("Password is empty", "GetCredentials", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AppSettingsCredentialStore.cs", 56);
			return null;
		}
		return new VpnCredentials(value, value2);
	}

	public void DeleteCredentials()
	{
		_appSettingsHelper.SetValue(AppSettingsKeys.VpnUsername, string.Empty);
		_appSettingsHelper.SetValue(AppSettingsKeys.VpnPassword, string.Empty);
	}
}
