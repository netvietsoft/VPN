using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using NextAiVPN.Common;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.MessageBoxWindows.ConflictDetected;
using NextAiVPN.UI.TrafficOptimizer;
using NextAiVPN.UI.TrustedNetwork;
using VpnSDK.Common.Enums;
using VpnSDK.DnsMonitor.DTO;
using VpnSDK.Interfaces;

namespace NextAiVPN.UI.Settings;

public partial class ExpandedNextAiVpnSettingsControl : UserControl, IComponentConnector
{
	private SDKMonitor _sdkMonitor;

	private TrafficOptimizerWindow _trafficOptimizerWindow;

	private readonly object _lockObject = new object();

	public TrustedNetworksWindowViewModel TrustedNetworksWindowViewModel;

	public string ConnectedNetwork;

	public IStartUpService StartUpService;

	private IDisconnectMessageBoxHelper _disconnectMessageBoxHelper;

	public ExpandedNextAiVpnSettingsControl()
	{
		InitializeComponent();
		base.Loaded += OnStyleLoaded;
		base.Unloaded += OnStyleUnloaded;
	}

	public void OnVpnModeChanged(VpnType obj)
	{
		switch (obj)
		{
		case VpnType.NextAiVPN:
			SetPrivateMode();
			break;
		case VpnType.Streaming:
			SetStreamingMode();
			break;
		default:
			throw new ArgumentOutOfRangeException("obj", obj, null);
		}
	}

	private void SetStreamingMode()
	{
		StreamingInfoBorder.Visibility = Visibility.Visible;
		PreferencesPanel.IsEnabled = false;
		PreferencesPanel.Opacity = 0.4;
	}

	private void SetPrivateMode()
	{
		StreamingInfoBorder.Visibility = Visibility.Collapsed;
		PreferencesPanel.IsEnabled = true;
		PreferencesPanel.Opacity = 1.0;
	}

	public void GetDependencies(SDKMonitor sdkMonitor, TrafficOptimizerWindow trafficOptimizerWindow, IDisconnectMessageBoxHelper disconnectMessageBoxHelper)
	{
		_sdkMonitor = sdkMonitor;
		_trafficOptimizerWindow = trafficOptimizerWindow;
		_disconnectMessageBoxHelper = disconnectMessageBoxHelper;
		VpnModeChangeEvent.OnVpnModeChanged = (Action<VpnType>)Delegate.Combine(VpnModeChangeEvent.OnVpnModeChanged, new Action<VpnType>(OnVpnModeChanged));
	}

	public void GetStartUpService(IStartUpService startupService)
	{
		StartUpService = startupService;
	}

	public void GetTrustedNetworksWindowViewModel(TrustedNetworksWindowViewModel trustedNetworksWindowViewModel)
	{
		TrustedNetworksWindowViewModel = trustedNetworksWindowViewModel;
	}

	private void StyleChanged(NextAiVPN.Services.Persistence.Style arg1, NextAiVPN.Services.Persistence.Style arg2)
	{
		InitTogglesState();
	}

	private void OnStyleLoaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged += StyleChanged;
	}

	private void OnStyleUnloaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged -= StyleChanged;
	}

	private void InitTogglesState()
	{
		KillSwitchToggle.Dot_InitializeState();
		NetworkDetectionNotificationToggle.Dot_InitializeState();
		AutoProtectToggle.Dot_InitializeState();
		SplitTunnelingToggle.Dot_InitializeState();
	}

	private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		ScrollViewer obj = (ScrollViewer)sender;
		obj.ScrollToVerticalOffset(obj.VerticalOffset - (double)e.Delta);
		e.Handled = true;
	}

	private void KillSwitchToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			RestoreToggleState(KillSwitchToggle, "KillSwitch");
			return;
		}
		if (!_sdkMonitor.VpnExpandedWindow.AdminPermissionsRunnerService.TryToRun(RunAsAdminOption.KillSwitch))
		{
			RestoreToggleState(KillSwitchToggle, "KillSwitch");
			return;
		}
		if (Utils.AppSettingsHelper.GetValue("IsSplitTunnelingEnabled").Equals("1") && KillSwitchToggle.Toggled1)
		{
			if (!ShowConflictDetectedWindow("Conflict Detected", "Kill Switch is inactive when you use Split Tunneling on custom hostnames.", "Turn Off Split Tunneling", "Cancel"))
			{
				KillSwitchToggle.Toggled1 = false;
				return;
			}
			SplitTunnelingToggle.Toggled1 = false;
			Utils.AppSettingsHelper.SetValue("IsSplitTunnelingEnabled", "0");
		}
		SetKillSwitch();
	}

	public void SetKillSwitch()
	{
		lock (_lockObject)
		{
			Task.Run(delegate
			{
				Utils.VpnConfigurationSaver.SaveKillSwitchConfiguration(KillSwitchToggle.Toggled1);
				if (!KillSwitchToggle.Toggled1)
				{
					Utils.VpnConfigurationSaver.SaveBlockLANConfiguration(status: false);
					Utils.MixpanelNotification.SendNotification("Settings - Killswitch_OFF");
				}
				else
				{
					Utils.MixpanelNotification.SendNotification("Settings - Killswitch_ON");
				}
				_sdkMonitor.NextAiVpnSdkManager.AllowOnlyVPNConnectivity = KillSwitchToggle.Toggled1;
			});
		}
	}

	private static bool ShowConflictDetectedWindow(string title, string description, string okButtonText, string cancelButtonText)
	{
		ConflictDetectedMessageBoxWindow conflictDetectedMessageBoxWindow = new ConflictDetectedMessageBoxWindow(new ConflictDetectedMessageBoxWindowViewModel(Utils.Logger)
		{
			Title = title,
			Description = description,
			OkButtonText = okButtonText,
			CancelButtonText = cancelButtonText
		});
		conflictDetectedMessageBoxWindow.ShowDialog();
		return conflictDetectedMessageBoxWindow.DialogResult;
	}

	public void EnableSettings(bool isEnabled)
	{
	}

	private void SplitTunnelingToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			RestoreToggleState(SplitTunnelingToggle, "IsSplitTunnelingEnabled");
			return;
		}
		lock (_lockObject)
		{
			if (Utils.AppSettingsHelper.GetValue("KillSwitch").Equals("1") && SplitTunnelingToggle.Toggled1)
			{
				if (!ShowConflictDetectedWindow("Conflict Detected", "Split Tunneling (custom hostnames) can't be enabled with Kill Switch ON.", "Turn Off Kill Switch", "Cancel"))
				{
					SplitTunnelingToggle.Toggled1 = false;
					return;
				}
				KillSwitchToggle.Toggled1 = false;
				SetKillSwitch();
			}
			if (Utils.AppSettingsHelper.GetValue(AppSettingsKeys.IsLanAllowed).Equals("0") && SplitTunnelingToggle.Toggled1)
			{
				if (!ShowConflictDetectedWindow("Conflict Detected", "Split Tunneling can only work when \"Control access to your devices\" is turned ON.", "Turn On Control Access", "Cancel"))
				{
					SplitTunnelingToggle.Toggled1 = false;
					return;
				}
				AllowLan.Toggled1 = true;
				EnableAllowLan();
			}
			Task.Run(delegate
			{
				EnableAllowLan();
				_sdkMonitor.NextAiVpnSdkManager.SplitTunnelMode = (SplitTunnelingToggle.Toggled1 ? SplitTunnelMode.RouteSelectedTrafficOutsideVpn : SplitTunnelMode.Disabled);
				_sdkMonitor.NextAiVpnSdkManager.IsSplitTunnelEnabled = SplitTunnelingToggle.Toggled1;
				Utils.AppSettingsHelper.SetValue("IsSplitTunnelingEnabled", SplitTunnelingToggle.Toggled1 ? "1" : "0");
				LogSettings(SplitTunnelingToggle.Toggled1 ? "Settings - SplitTunneling_ON" : "Settings - SplitTunneling_OFF", "SplitTunnelingToggle_OnMouseLeftButtonDown");
			});
		}
	}

	private void RestoreToggleState(ToggleSwitch toggleSwitch, string settingsVariableName)
	{
		toggleSwitch.Toggled1 = Utils.AppSettingsHelper.GetValue(settingsVariableName).Equals("1");
		toggleSwitch.Dot_InitializeState();
	}

	private void AutoProtectToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			RestoreToggleState(AutoProtectToggle, "AutoConnect");
			return;
		}
		Utils.VpnConfigurationSaver.SaveAutoConnectConfiguration(AutoProtectToggle.Toggled1);
		SetAutoProtectNotificationControl();
		Task.Run(async delegate
		{
			if (AutoProtectToggle.Toggled1)
			{
				if (!_sdkMonitor.ConnectionStatus.ToLower().Equals("connected"))
				{
					switch (_sdkMonitor.GetVpnMode())
					{
					case VpnType.NextAiVPN:
					{
						ILocation nextaivpnLocation = _sdkMonitor.NextAiVpnLocation;
						if (nextaivpnLocation != null)
						{
							ConnectToVpn(nextaivpnLocation);
							_sdkMonitor.ConnectToUntrustedNetwork = false;
						}
						break;
					}
					case VpnType.Streaming:
						_sdkMonitor.ConnectToVPN(VpnType.Streaming);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
			}
			else
			{
				await _sdkMonitor.DisconnectVPN();
			}
		});
	}

	private void ConnectToVpn(ILocation location)
	{
		_sdkMonitor.ConnectToVPN(location);
		Utils.Api.SetIsBestAvailable((_sdkMonitor.NextAiVpnLocation.Id == "bestavailable") ? "1" : "0");
	}

	public void SetAutoProtectNotificationControl()
	{
		AutoProtectNotificationStackPanel.Visibility = (Utils.AppSettingsHelper.GetValue("AutoConnect").Equals("1") ? Visibility.Collapsed : Visibility.Visible);
	}

	private void NetworkDetectionNotificationToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			RestoreToggleState(NetworkDetectionNotificationToggle, "IsNeedSendAutoProtectNotification");
		}
		else
		{
			Utils.AppSettingsHelper.SetValue("IsNeedSendAutoProtectNotification", NetworkDetectionNotificationToggle.Toggled1 ? "1" : "0");
		}
	}

	private void Manage_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!_sdkMonitor.NextAiVpnSdkManager.IsConnected)
		{
			_sdkMonitor.VpnExpandedWindow.SplitTunnelingMainWindow.ShowDialog();
			if (_sdkMonitor.VpnExpandedWindow.SplitTunnelingMainWindow.IsNeededToShowPopUp && _sdkMonitor.ConnectionStatus.ToLower().Equals("connected"))
			{
				_sdkMonitor.VpnExpandedWindow.Mainpanel.SplitTunnelingReconnectPopUpViewModel.UserControlVisibility = Visibility.Visible;
			}
		}
	}

	private void SeeAll_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			TrustedNetworksWindowViewModel.SortTrustedNetworks();
			new TrustedNetworksWindow(TrustedNetworksWindowViewModel).ShowDialog();
		}
	}

	private void AdBlockerToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			RestoreToggleState(AdBlockerToggle, "IsAdBlocker");
		}
		else
		{
			_sdkMonitor.AddBlockerToggle(AdBlockerToggle.Toggled1);
		}
	}

	private void AllowLan_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			RestoreToggleState(AllowLan, AppSettingsKeys.IsLanAllowed);
			return;
		}
		if (Utils.AppSettingsHelper.GetValue("IsSplitTunnelingEnabled").Equals("1") && !AllowLan.Toggled1)
		{
			if (ShowConflictDetectedWindow("Conflict Detected", "Split Tunneling needs \"Control access to your devices\" ON to work. Turning off \"Control access to your devices\" will disable Split Tunneling.", "Turn Off Both", "Cancel"))
			{
				SplitTunnelingToggle.Toggled1 = false;
				Utils.AppSettingsHelper.SetValue("IsSplitTunnelingEnabled", "0");
			}
			else
			{
				AllowLan.Toggled1 = true;
			}
		}
		EnableAllowLan();
	}

	private void EnableAllowLan()
	{
		Task.Run(delegate
		{
			_sdkMonitor.NextAiVpnSdkManager.AllowLocalAdaptersWhenConnected = AllowLan.Toggled1;
			Utils.AppSettingsHelper.SetValue(AppSettingsKeys.IsLanAllowed, AllowLan.Toggled1 ? "1" : "0");
			LogSettings(AllowLan.Toggled1 ? "Settings - AllowLan_ON" : "Settings - AllowLan_OFF", "AllowLan_OnMouseLeftButtonDown");
		});
	}

	private void IPv6LeakProtection_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			RestoreToggleState(IPv6LeakProtection, "IsIPv6LeakProtection");
			return;
		}
		Task.Run(delegate
		{
			_sdkMonitor.NextAiVpnSdkManager.DisableIPv6LeakProtection = IPv6LeakProtection.Toggled1;
			Utils.AppSettingsHelper.SetValue("IsIPv6LeakProtection", IPv6LeakProtection.Toggled1 ? "1" : "0");
			LogSettings(IPv6LeakProtection.Toggled1 ? "Settings - IPv6LeakProtection_ON" : "Settings - IPv6LeakProtection_OFF", "IPv6LeakProtection_OnMouseLeftButtonDown");
		});
	}

	private static void LogSettings(string message, string methodName)
	{
		Utils.MixpanelNotification.SendNotification(message);
		Utils.Logger.Information(message, "LogSettings", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Settings\\ExpandedNextAiVpnSettingsControl.xaml.cs", 426);
	}

	private void DnsLeakProtection_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			RestoreToggleState(DnsLeakProtection, "IsDnsLeakProtection");
			return;
		}
		Task.Run(delegate
		{
			_sdkMonitor.NextAiVpnSdkManager.DisableDNSLeakProtection = DnsLeakProtection.Toggled1;
			Utils.AppSettingsHelper.SetValue("IsDnsLeakProtection", DnsLeakProtection.Toggled1 ? "1" : "0");
			LogSettings(DnsLeakProtection.Toggled1 ? "Settings - DnsLeakProtection_ON" : "Settings - DnsLeakProtection_OFF", "DnsLeakProtection_OnMouseLeftButtonDown");
		});
	}

	private void DnsMonitoring_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			RestoreToggleState(DnsMonitoring, "IsDnsMonitoring");
			return;
		}
		Task.Run(delegate
		{
			if (!_sdkMonitor.NextAiVpnSdkManager.IsConnected && !_sdkMonitor.NextAiVpnSdkManager.IsConnecting)
			{
				_sdkMonitor.NextAiVpnSdkManager.SetDnsMonitorConfig(new DnsMonitoringConfig
				{
					IsEnabled = DnsMonitoring.Toggled1
				});
				Utils.AppSettingsHelper.SetValue("IsDnsMonitoring", DnsMonitoring.Toggled1 ? "1" : "0");
				LogSettings(DnsMonitoring.Toggled1 ? "Settings - DnsMonitoring_ON" : "Settings - DnsMonitoring_OFF", "DnsMonitoring_OnMouseLeftButtonDown");
			}
		});
	}

	private void TrafficOptimizerLink_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		ShowTrafficOptimizer();
	}

	private void ShowTrafficOptimizer()
	{
		_trafficOptimizerWindow?.ShowDialog();
	}

	private void TrafficOptimizer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
		{
			RestoreToggleState(TrafficOptimizer, "IsTrafficOptimizer");
		}
		else
		{
			Utils.AppSettingsHelper.SetValue("IsTrafficOptimizer", TrafficOptimizer.Toggled1 ? "1" : "0");
		}
	}
}
