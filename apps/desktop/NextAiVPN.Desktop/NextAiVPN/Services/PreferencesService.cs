using System;
using System.IO;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;
using VpnSDK.Common.Enums;
using VpnSDK.DnsMonitor.DTO;

namespace NextAiVPN.Services;

internal class PreferencesService : IPreferencesService
{
	private readonly VPNWindowExpanded _windowExpanded;

	private readonly IBugsnagService _bugsnagService;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	public PreferencesService(VPNWindowExpanded windowExpanded, IBugsnagService bugsnagService, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_windowExpanded = windowExpanded;
		_bugsnagService = bugsnagService;
		_appSettingsHelper = appSettingsHelper;
		_logger = logger;
		VpnModeChangeEvent.OnVpnModeChanged = (Action<VpnType>)Delegate.Combine(VpnModeChangeEvent.OnVpnModeChanged, new Action<VpnType>(OnVpnModeChanged));
	}

	private void OnVpnModeChanged(VpnType obj)
	{
		RestoreStreamingSettings();
		switch (obj)
		{
		case VpnType.NextAiVPN:
			Restore();
			break;
		case VpnType.Streaming:
			RestoreForStreaming();
			break;
		default:
			throw new ArgumentOutOfRangeException("obj", obj, null);
		}
	}

	public void Set(string preferenceName, string preferenceValue)
	{
		_appSettingsHelper.SetValue(preferenceName, preferenceValue);
	}

	public void Restore()
	{
		RestoreAllAdvancedSettings();
		RestoreStreamingSettings();
	}

	private void RestoreAllAdvancedSettings()
	{
		RestoreAdvancedSettingsKillSwitch();
		RestoreAdvancedSettingsStartup();
		RestoreAdvancedSettingsAutoConnect();
		RestoreAdvancedSettingsProtocol();
		RestoreAdvancedSettingsSendAutoProtectNotifications();
		RestoreAdvancedSettingsSplitTunneling();
		RestoreAdvancedSettingsAdBlocker();
		RestoreAdvancedSettingsAllowLan();
		RestoreAdvancedSettingsIPv6LeakProtection();
		RestoreAdvancedSettingsDnsLeakProtection();
		RestoreAdvancedSettingsDnsMonitoring();
		RestoreAdvancedSettingsTrafficOptimizer();
	}

	private void RestoreStreamingSettings()
	{
		RestoreStreamingSplitTunneling();
	}

	private void RestoreStreamingSplitTunneling()
	{
		bool preferenceBoolValue = GetPreferenceBoolValue(_appSettingsHelper.GetValue("IsStreamingSplitTunnelingEnabled"));
		_windowExpanded.GeneralControl.StreamingSettingsControl.SplitTunnelingSwitch.Toggled1 = preferenceBoolValue;
		_windowExpanded.GeneralControl.StreamingSettingsControl.SplitTunnelingSwitch.Dot_InitializeState();
	}

	private void RestoreAdvancedSettingsTrafficOptimizer()
	{
		bool preferenceBoolValue = GetPreferenceBoolValue(_appSettingsHelper.GetValue("IsTrafficOptimizer"));
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.TrafficOptimizer.Toggled1 = preferenceBoolValue;
	}

	private void RestoreAdvancedSettingsDnsMonitoring()
	{
		try
		{
			if (!_windowExpanded.SdkObject.NextAiVpnSdkManager.IsConnected && !_windowExpanded.SdkObject.NextAiVpnSdkManager.IsConnecting)
			{
				bool preferenceBoolValue = GetPreferenceBoolValue(_appSettingsHelper.GetValue("IsDnsMonitoring"));
				_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.DnsMonitoring.Toggled1 = preferenceBoolValue;
				_windowExpanded.SdkObject.NextAiVpnSdkManager.SetDnsMonitorConfig(new DnsMonitoringConfig
				{
					IsEnabled = preferenceBoolValue
				});
			}
		}
		catch (Exception exception)
		{
			_logger.Error(exception, "RestoreAdvancedSettingsDnsMonitoring", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\PreferencesService.cs", 124);
		}
	}

	private void RestoreAdvancedSettingsDnsLeakProtection()
	{
		bool preferenceBoolValue = GetPreferenceBoolValue(_appSettingsHelper.GetValue("IsDnsLeakProtection"));
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.DnsLeakProtection.Toggled1 = preferenceBoolValue;
		_windowExpanded.SdkObject.NextAiVpnSdkManager.DisableDNSLeakProtection = preferenceBoolValue;
	}

	private void RestoreAdvancedSettingsIPv6LeakProtection()
	{
		bool preferenceBoolValue = GetPreferenceBoolValue(_appSettingsHelper.GetValue("IsIPv6LeakProtection"));
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.IPv6LeakProtection.Toggled1 = preferenceBoolValue;
		_windowExpanded.SdkObject.NextAiVpnSdkManager.DisableIPv6LeakProtection = preferenceBoolValue;
	}

	private void RestoreAdvancedSettingsAllowLan()
	{
		bool preferenceBoolValue = GetPreferenceBoolValue(_appSettingsHelper.GetValue(AppSettingsKeys.IsLanAllowed));
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AllowLan.Toggled1 = preferenceBoolValue;
		_windowExpanded.SdkObject.NextAiVpnSdkManager.AllowLocalAdaptersWhenConnected = preferenceBoolValue;
	}

	private void RestoreAdvancedSettingsAdBlocker()
	{
		bool preferenceBoolValue = GetPreferenceBoolValue(_appSettingsHelper.GetValue("IsAdBlocker"));
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AdBlockerToggle.Toggled1 = preferenceBoolValue;
		_windowExpanded.SdkObject.AddBlockerToggle(preferenceBoolValue);
	}

	private void RestoreAdvancedSettingsSplitTunneling()
	{
		bool preferenceBoolValue = GetPreferenceBoolValue(_appSettingsHelper.GetValue("IsSplitTunnelingEnabled"));
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SplitTunnelingToggle.Toggled1 = preferenceBoolValue;
		_windowExpanded.SdkObject.NextAiVpnSdkManager.SplitTunnelMode = (preferenceBoolValue ? SplitTunnelMode.RouteSelectedTrafficOutsideVpn : SplitTunnelMode.Disabled);
		_windowExpanded.SdkObject.NextAiVpnSdkManager.IsSplitTunnelEnabled = preferenceBoolValue;
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SplitTunnelingToggle.Dot_InitializeState();
	}

	private void RestoreAdvancedSettingsSendAutoProtectNotifications()
	{
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.NetworkDetectionNotificationToggle.Toggled1 = GetPreferenceBoolValue(_appSettingsHelper.GetValue("IsNeedSendAutoProtectNotification"));
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.NetworkDetectionNotificationToggle.Dot_InitializeState();
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SetAutoProtectNotificationControl();
	}

	private void RestoreAdvancedSettingsProtocol()
	{
		switch (_windowExpanded.SdkObject.GetVpnMode())
		{
		case VpnType.NextAiVPN:
			SetPrivateModeProtocolAdvanced();
			break;
		case VpnType.Streaming:
			SetStreamingModeProtocolAdvanced();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void RestoreAdvancedSettingsAutoConnect()
	{
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AutoProtectToggle.Toggled1 = GetPreferenceBoolValue(_appSettingsHelper.GetValue("AutoConnect"));
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AutoProtectToggle.Dot_InitializeState();
	}

	private void RestoreAdvancedSettingsStartup()
	{
		bool isStartUp = GetPreferenceBoolValue(_appSettingsHelper.GetValue("Startup"));
		_windowExpanded.Preferences.StartUpToggle.Toggled1 = isStartUp;
		_windowExpanded.Preferences.StartUpToggle.Dot_InitializeState();
		Task.Run(delegate
		{
			SetStartup(isStartUp);
		});
	}

	private void RestoreAdvancedSettingsKillSwitch()
	{
		bool preferenceBoolValue = GetPreferenceBoolValue(_appSettingsHelper.GetValue("KillSwitch"));
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Toggled1 = preferenceBoolValue;
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Dot_InitializeState();
		_windowExpanded.SdkObject.NextAiVpnSdkManager.AllowOnlyVPNConnectivity = preferenceBoolValue;
	}

	public Task RestoreAsync()
	{
		return Task.Run(delegate
		{
			_windowExpanded.Dispatcher.Invoke(RestoreAllAdvancedSettings);
		});
	}

	private void RestoreForStreaming()
	{
		DisableKillSwitch();
		DisableAutoProtect();
		DisableSplitTunneling();
		DisableNetworkDetectionNotification();
		DisableAllowLan();
		RestoreAdvancedSettingsStartup();
	}

	private void DisableAllowLan()
	{
		_windowExpanded.SdkObject.NextAiVpnSdkManager.AllowLocalAdaptersWhenConnected = false;
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AllowLan.Toggled1 = false;
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AllowLan.Dot_InitializeState();
	}

	private void DisableNetworkDetectionNotification()
	{
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.NetworkDetectionNotificationToggle.Toggled1 = false;
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.NetworkDetectionNotificationToggle.Dot_InitializeState();
	}

	private void DisableSplitTunneling()
	{
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SplitTunnelingToggle.Toggled1 = false;
		_windowExpanded.SdkObject.NextAiVpnSdkManager.SplitTunnelMode = SplitTunnelMode.Disabled;
		_windowExpanded.SdkObject.NextAiVpnSdkManager.IsSplitTunnelEnabled = false;
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SplitTunnelingToggle.Dot_InitializeState();
	}

	private void DisableAutoProtect()
	{
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AutoProtectToggle.Toggled1 = false;
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AutoProtectToggle.Dot_InitializeState();
	}

	private void DisableKillSwitch()
	{
		_windowExpanded.SdkObject.NextAiVpnSdkManager.AllowOnlyVPNConnectivity = false;
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Toggled1 = false;
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Dot_InitializeState();
	}

	private bool GetPreferenceBoolValue(string preferenceValue)
	{
		return preferenceValue.Equals("1");
	}

	private void SetStartup(bool isStartUp)
	{
		try
		{
			RemoveFromTheStartUpIfExist(SystemInfo.AppName);
			RemoveFromTheStartUpIfExist("NextAiTechnology VPN");
			if (isStartUp)
			{
				_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.StartUpService.AddStartUp();
			}
			else
			{
				_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.StartUpService.RemoveStartUp();
			}
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify("[AppStartup]" + ex.Message);
		}
	}

	private static void RemoveFromTheStartUpIfExist(string appName)
	{
		if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup)))
		{
			File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + appName + ".lnk");
		}
	}

	public void RestoreProtocol()
	{
		switch (_windowExpanded.SdkObject.GetVpnMode())
		{
		case VpnType.NextAiVPN:
			SetPrivateModeProtocol();
			break;
		case VpnType.Streaming:
			SetStreamingModeProtocol();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void SetStreamingModeProtocol()
	{
		_appSettingsHelper.GetValue("StreamingProtocol");
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.IKEv2.IsChecked = true;
	}

	private void SetStreamingModeProtocolAdvanced()
	{
		_appSettingsHelper.GetValue("StreamingProtocol");
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.IKEv2.IsChecked = true;
	}

	private void SetPrivateModeProtocolAdvanced()
	{
		switch (_appSettingsHelper.GetValue("protocol"))
		{
		case "WireGuard":
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.WireGuard.IsChecked = true;
			break;
		case "IKEv2":
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.IKEv2.IsChecked = true;
			break;
		case "OpenVPN":
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.WireGuard.IsChecked = true;
			break;
		default:
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.WireGuard.IsChecked = true;
			break;
		}
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.scramble.IsChecked = GetPreferenceBoolValue(_appSettingsHelper.GetValue("scramble"));
	}

	private void SetPrivateModeProtocol()
	{
		switch (_appSettingsHelper.GetValue("protocol"))
		{
		case "WireGuard":
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.WireGuard.IsChecked = true;
			break;
		case "IKEv2":
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.IKEv2.IsChecked = true;
			break;
		case "OpenVPN":
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.WireGuard.IsChecked = true;
			break;
		default:
			_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.WireGuard.IsChecked = true;
			break;
		}
		_windowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.scramble.IsChecked = GetPreferenceBoolValue(_appSettingsHelper.GetValue("scramble"));
	}
}
