using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Common;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.MessageBoxWindows.ConflictDetected;
using NextAiVPN.UI.TrustedNetwork;
using VpnSDK.Common.Enums;
using VpnSDK.Interfaces;

namespace NextAiVPN.UI.Settings;

public class ExpandedNextAiVpnSettingsControlViewModel : ViewModelBase
{
	private readonly object _lockObject = new object();

	private readonly SDKMonitor _sdkMonitor;

	private readonly TrustedNetworksWindowViewModel _trustedNetworksWindowViewModel;

	private readonly IStartUpService _startUpService;

	private readonly IDisconnectMessageBoxHelper _disconnectMessageBoxHelper;

	private readonly IVpnConfigurationSaver _vpnConfigurationSaver;

	private readonly IApiClient _apiClient;

	private readonly IAnalyticsService _analyticsService;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private bool _isKillSwitch;

	private bool _isSplitTunnelingEnabled;

	private bool _isStartUpEnabled;

	private bool _isAutoProtectEnabled;

	private bool _isNetworkDetectedNotifications;

	private Visibility _autoProtectNotificationStackPanel;

	private Visibility _streamingInfoBorderVisibility;

	private bool _isPreferencesStackPanelEnable;

	public bool IsKillSwitchEnabled
	{
		get
		{
			return _isKillSwitch;
		}
		set
		{
			if (_isKillSwitch != value && !_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected())
			{
				_isKillSwitch = value;
				OnPropertyChanged("IsKillSwitchEnabled");
				KillSwitchMouseLeftButtonUpCommand.Execute(this);
			}
		}
	}

	public bool IsSplitTunnelingEnabled
	{
		get
		{
			return _isSplitTunnelingEnabled;
		}
		set
		{
			if (_isSplitTunnelingEnabled != value)
			{
				_isSplitTunnelingEnabled = value;
				OnPropertyChanged("IsSplitTunnelingEnabled");
				SplitTunnelingMouseLeftButtonUpCommand.Execute(this);
			}
		}
	}

	public bool IsStartUpEnabled
	{
		get
		{
			return _isStartUpEnabled;
		}
		set
		{
			if (_isStartUpEnabled != value)
			{
				_isStartUpEnabled = value;
				StartupMouseLeftButtonUpCommand.Execute(this);
				OnPropertyChanged("IsStartUpEnabled");
			}
		}
	}

	public bool IsAutoProtectEnabled
	{
		get
		{
			return _isAutoProtectEnabled;
		}
		set
		{
			if (_isAutoProtectEnabled != value)
			{
				_isAutoProtectEnabled = value;
				AutoProtectMouseLeftButtonUpCommand.Execute(this);
				OnPropertyChanged("IsAutoProtectEnabled");
			}
		}
	}

	public bool IsNetworkDetectedNotifications
	{
		get
		{
			return _isNetworkDetectedNotifications;
		}
		set
		{
			if (_isNetworkDetectedNotifications != value)
			{
				_isNetworkDetectedNotifications = value;
				NetworkNotificationsMouseLeftButtonUpCommand.Execute(this);
				OnPropertyChanged("IsNetworkDetectedNotifications");
			}
		}
	}

	public Visibility AutoProtectNotificationStackPanel
	{
		get
		{
			return _autoProtectNotificationStackPanel;
		}
		set
		{
			if (_autoProtectNotificationStackPanel != value)
			{
				_autoProtectNotificationStackPanel = value;
				OnPropertyChanged("AutoProtectNotificationStackPanel");
			}
		}
	}

	public Visibility StreamingInfoBorderVisibility
	{
		get
		{
			return _streamingInfoBorderVisibility;
		}
		set
		{
			if (_streamingInfoBorderVisibility != value)
			{
				_streamingInfoBorderVisibility = value;
				OnPropertyChanged("StreamingInfoBorderVisibility");
			}
		}
	}

	public bool IsPreferencesStackPanelEnable
	{
		get
		{
			return _isPreferencesStackPanelEnable;
		}
		set
		{
			if (_isPreferencesStackPanelEnable != value)
			{
				_isPreferencesStackPanelEnable = value;
				OnPropertyChanged("IsPreferencesStackPanelEnable");
			}
		}
	}

	public ICommand KillSwitchMouseLeftButtonUpCommand { get; private set; }

	public ICommand SplitTunnelingMouseLeftButtonUpCommand { get; private set; }

	public ICommand SplitTunnelingTextBlockMouseLeftButtonDownCommand { get; private set; }

	public ICommand StartupMouseLeftButtonUpCommand { get; private set; }

	public ICommand AutoProtectMouseLeftButtonUpCommand { get; private set; }

	public ICommand SeeAllMouseLeftButtonDownCommand { get; private set; }

	public ICommand NetworkNotificationsMouseLeftButtonUpCommand { get; private set; }

	public ExpandedNextAiVpnSettingsControlViewModel(SDKMonitor sdkMonitor, TrustedNetworksWindowViewModel trustedNetworksWindowViewModel, IStartUpService startUpService, IDisconnectMessageBoxHelper disconnectMessageBoxHelper, IVpnConfigurationSaver vpnConfigurationSaver, IApiClient apiClient, IAnalyticsService analyticsService, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_sdkMonitor = sdkMonitor;
		_trustedNetworksWindowViewModel = trustedNetworksWindowViewModel;
		_startUpService = startUpService;
		_disconnectMessageBoxHelper = disconnectMessageBoxHelper;
		_vpnConfigurationSaver = vpnConfigurationSaver;
		_apiClient = apiClient;
		_analyticsService = analyticsService;
		_appSettingsHelper = appSettingsHelper;
		_logger = logger;
		KillSwitchMouseLeftButtonUpCommand = new ActionCommand(KillSwitchMouseLeftButtonUpCommandExecute);
		SplitTunnelingMouseLeftButtonUpCommand = new ActionCommand(SplitTunnelingMouseLeftButtonUpCommandExecute);
		SplitTunnelingTextBlockMouseLeftButtonDownCommand = new ActionCommand(SplitTunnelingTextBlockMouseLeftButtonDownCommandExecute);
		StartupMouseLeftButtonUpCommand = new ActionCommand(StartupMouseLeftButtonUpCommandExecute);
		AutoProtectMouseLeftButtonUpCommand = new ActionCommand(AutoProtectMouseLeftButtonUpCommandExecute);
		SeeAllMouseLeftButtonDownCommand = new ActionCommand(SeeAllMouseLeftButtonDownCommandExecute);
		NetworkNotificationsMouseLeftButtonUpCommand = new ActionCommand(NetworkNotificationsMouseLeftButtonUpCommandExecute);
		VpnModeChangeEvent.OnVpnModeChanged = (Action<VpnType>)Delegate.Combine(VpnModeChangeEvent.OnVpnModeChanged, new Action<VpnType>(OnVpnModeChanged));
	}

	private void OnVpnModeChanged(VpnType obj)
	{
		switch (obj)
		{
		case VpnType.NextAiVPN:
			StreamingInfoBorderVisibility = Visibility.Collapsed;
			IsPreferencesStackPanelEnable = true;
			break;
		case VpnType.Streaming:
			StreamingInfoBorderVisibility = Visibility.Visible;
			IsPreferencesStackPanelEnable = false;
			break;
		default:
			throw new ArgumentOutOfRangeException("obj", obj, null);
		}
	}

	private void NetworkNotificationsMouseLeftButtonUpCommandExecute()
	{
		_appSettingsHelper.SetValue("IsNeedSendAutoProtectNotification", IsNetworkDetectedNotifications ? "1" : "0");
	}

	private void SeeAllMouseLeftButtonDownCommandExecute()
	{
		_trustedNetworksWindowViewModel.SortTrustedNetworks();
		new TrustedNetworksWindow(_trustedNetworksWindowViewModel).ShowDialog();
	}

	private async void AutoProtectMouseLeftButtonUpCommandExecute()
	{
		_vpnConfigurationSaver.SaveAutoConnectConfiguration(IsAutoProtectEnabled);
		SetAutoProtectNotificationControl();
		if (IsAutoProtectEnabled)
		{
			if (_sdkMonitor.ConnectionStatus.ToLower().Equals("connected"))
			{
				return;
			}
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
		else
		{
			await _sdkMonitor.DisconnectVPN();
		}
	}

	private void ConnectToVpn(ILocation location)
	{
		_sdkMonitor.ConnectToVPN(location);
		_apiClient.SetIsBestAvailable((_sdkMonitor.NextAiVpnLocation.Id == "bestavailable") ? "1" : "0");
	}

	public void SetAutoProtectNotificationControl()
	{
		AutoProtectNotificationStackPanel = (_appSettingsHelper.GetValue("AutoConnect").Equals("1") ? Visibility.Collapsed : Visibility.Visible);
	}

	private void StartupMouseLeftButtonUpCommandExecute()
	{
		lock (_lockObject)
		{
			Task.Run(delegate
			{
				_vpnConfigurationSaver.SaveStartupConfiguration(IsStartUpEnabled);
				if (IsStartUpEnabled)
				{
					_startUpService.AddStartUp();
					_analyticsService.SendNotification("Settings - Startup_ON");
				}
				else
				{
					_startUpService.RemoveStartUp();
					_analyticsService.SendNotification("Settings - Startup_OFF");
				}
			});
		}
	}

	private void SplitTunnelingTextBlockMouseLeftButtonDownCommandExecute()
	{
		_sdkMonitor.VpnExpandedWindow.SplitTunnelingMainWindow.ShowDialog();
		if (_sdkMonitor.VpnExpandedWindow.SplitTunnelingMainWindow.IsNeededToShowPopUp && _sdkMonitor.ConnectionStatus.ToLower().Equals("connected"))
		{
			_sdkMonitor.VpnExpandedWindow.Mainpanel.SplitTunnelingReconnectPopUpViewModel.UserControlVisibility = Visibility.Visible;
		}
	}

	private void SplitTunnelingMouseLeftButtonUpCommandExecute()
	{
		lock (_lockObject)
		{
			if (_appSettingsHelper.GetValue("KillSwitch").Equals("1") && IsSplitTunnelingEnabled)
			{
				if (!ShowConflictDetectedWindow("Conflict Detected", "Split Tunneling (custom hostnames) can't be enabled with Kill Switch ON.", "Turn Off Kill Switch", "Cancel"))
				{
					_isSplitTunnelingEnabled = false;
					_sdkMonitor.VpnExpandedWindow.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SplitTunnelingToggle.Toggled1 = false;
					_sdkMonitor.VpnExpandedWindow.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SplitTunnelingToggle.Dot_InitializeState();
					return;
				}
				IsKillSwitchEnabled = false;
				_sdkMonitor.VpnExpandedWindow.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Toggled1 = false;
				_sdkMonitor.VpnExpandedWindow.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Dot_InitializeState();
				SetKillSwitch();
			}
			Task.Run(delegate
			{
				_sdkMonitor.NextAiVpnSdkManager.SplitTunnelMode = (IsSplitTunnelingEnabled ? SplitTunnelMode.RouteSelectedTrafficOutsideVpn : SplitTunnelMode.Disabled);
				_sdkMonitor.NextAiVpnSdkManager.IsSplitTunnelEnabled = IsSplitTunnelingEnabled;
				_appSettingsHelper.SetValue("IsSplitTunnelingEnabled", IsSplitTunnelingEnabled ? "1" : "0");
				_analyticsService.SendNotification(IsSplitTunnelingEnabled ? "Settings - SplitTunneling_ON" : "Settings - SplitTunneling_OFF");
			});
		}
	}

	private void KillSwitchMouseLeftButtonUpCommandExecute()
	{
		if (!_sdkMonitor.VpnExpandedWindow.AdminPermissionsRunnerService.TryToRun(RunAsAdminOption.KillSwitch))
		{
			return;
		}
		if (_appSettingsHelper.GetValue("IsSplitTunnelingEnabled").Equals("1") && IsKillSwitchEnabled)
		{
			if (!ShowConflictDetectedWindow("Conflict Detected", "Kill Switch is inactive when you use Split Tunneling on custom hostnames.", "Turn Off Split Tunneling", "Cancel"))
			{
				_isKillSwitch = false;
				_sdkMonitor.VpnExpandedWindow.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Toggled1 = false;
				_sdkMonitor.VpnExpandedWindow.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Dot_InitializeState();
				return;
			}
			IsSplitTunnelingEnabled = false;
			_sdkMonitor.VpnExpandedWindow.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SplitTunnelingToggle.Toggled1 = false;
			_sdkMonitor.VpnExpandedWindow.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.SplitTunnelingToggle.Dot_InitializeState();
			_appSettingsHelper.SetValue("IsSplitTunnelingEnabled", "0");
		}
		SetKillSwitch();
	}

	private bool ShowConflictDetectedWindow(string title, string description, string okButtonText, string cancelButtonText)
	{
		ConflictDetectedMessageBoxWindow conflictDetectedMessageBoxWindow = new ConflictDetectedMessageBoxWindow(new ConflictDetectedMessageBoxWindowViewModel(_logger)
		{
			Title = title,
			Description = description,
			OkButtonText = okButtonText,
			CancelButtonText = cancelButtonText
		});
		conflictDetectedMessageBoxWindow.ShowDialog();
		return conflictDetectedMessageBoxWindow.DialogResult;
	}

	private void SetKillSwitch()
	{
		lock (_lockObject)
		{
			Task.Run(delegate
			{
				_vpnConfigurationSaver.SaveKillSwitchConfiguration(IsKillSwitchEnabled);
				if (!IsKillSwitchEnabled)
				{
					_vpnConfigurationSaver.SaveBlockLANConfiguration(status: false);
					_analyticsService.SendNotification("Settings - Killswitch_OFF");
				}
				else
				{
					_analyticsService.SendNotification("Settings - Killswitch_ON");
				}
				_sdkMonitor.NextAiVpnSdkManager.AllowOnlyVPNConnectivity = IsKillSwitchEnabled;
			});
		}
	}
}
