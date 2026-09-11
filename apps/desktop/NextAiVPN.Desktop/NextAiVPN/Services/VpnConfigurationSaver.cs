using System;
using System.Globalization;
using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

internal class VpnConfigurationSaver : IVpnConfigurationSaver
{
	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IPreferencesRepository _preferencesRepository;

	private readonly IBugsnagService _bugsnagService;

	private readonly INotificationPresenter _notificationPresenter;

	public VpnConfigurationSaver(IAppSettingsHelper appSettingsHelper, IPreferencesRepository preferencesRepository, IBugsnagService bugsnagService, INotificationPresenter notificationPresenter)
	{
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_preferencesRepository = preferencesRepository ?? throw new ArgumentNullException("preferencesRepository");
		_bugsnagService = bugsnagService ?? throw new ArgumentNullException("bugsnagService");
		_notificationPresenter = notificationPresenter ?? throw new ArgumentNullException("notificationPresenter");
	}

	public void SaveKillSwitchConfiguration(bool status)
	{
		SaveToggle("KillSwitch", "KillSwitch".ToLower(), status);
	}

	public void SaveBlockLANConfiguration(bool status)
	{
		SaveToggle("BlockLAN", "BlockLAN".ToLower(), status);
	}

	public void SaveScrambleConfiguration(bool status)
	{
		SaveToggle("scramble", "scramble", status);
	}

	public void SaveStartupConfiguration(bool status)
	{
		SaveToggle("Startup", "Startup".ToLower(), status);
	}

	public void SaveAutoConnectConfiguration(bool status)
	{
		SaveToggle("AutoConnect", "AutoConnect".ToLower(), status);
	}

	public void SaveLogsEnabledConfiguration(bool status)
	{
		SaveToggle("LocalEnabled", "LocalEnabled".ToLower(), status, "Logs Enabled");
	}

	public void SaveProtocol(string protocol)
	{
		try
		{
			_appSettingsHelper.SetValue("protocol", protocol);
			_preferencesRepository.SaveSinglePreference("protocol", protocol);
		}
		catch (Exception ex)
		{
			SendConfigurationExceptionMessage("protocol", ex);
		}
	}

	public void SaveOpenVpnType(string type)
	{
		try
		{
			string value = (type.Equals("IKEv2") ? string.Empty : type);
			_appSettingsHelper.SetValue("protocolType", value);
			_preferencesRepository.SaveSinglePreference("protocolType", value);
		}
		catch (Exception ex)
		{
			SendConfigurationExceptionMessage("protocolType", ex);
		}
	}

	private void SaveToggle(string settingsKey, string preferenceName, bool status, string errorConfigurationName = null)
	{
		try
		{
			string value = (status ? "1" : "0");
			_appSettingsHelper.SetValue(settingsKey, value);
			_preferencesRepository.SaveSinglePreference(preferenceName, value);
		}
		catch (Exception ex)
		{
			SendConfigurationExceptionMessage(errorConfigurationName ?? settingsKey, ex);
		}
	}

	private void SendConfigurationExceptionMessage(string configurationName, Exception ex)
	{
		_notificationPresenter.ShowError(string.Format(CultureInfo.InvariantCulture, "Unable to save {0} Configuration: {1}", configurationName, ex.Message));
		_bugsnagService.Notify(string.Format(CultureInfo.InvariantCulture, "[{0} Config] - {1}", configurationName, ex.Message));
	}
}
