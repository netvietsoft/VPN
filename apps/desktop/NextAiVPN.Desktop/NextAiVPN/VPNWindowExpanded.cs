using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using NextAiVPN.Common;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Enums;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.Account;
using NextAiVPN.UI.AutoProtectNotificationBallons;
using NextAiVPN.UI.Feedback;
using NextAiVPN.UI.GetHelp;
using NextAiVPN.UI.Settings;
using NextAiVPN.UI.SplitTunneling;
using NextAiVPN.UI.TermsAndPolicies;

namespace NextAiVPN;

public partial class VPNWindowExpanded : Window, IComponentConnector
{
	private const int AutoProtectBalloonShowingTimeTicks = 10000;

	private readonly IShowFeedbackWindowService _feedbackWindowService;

	private readonly IBugsnagService _bugsnagService;

	private readonly IAppLogger _logger;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private DispatcherTimer _internetConnectionTimer;

	private DispatcherTimer _balloonTimer;

	private DispatcherTimer _subscriptionPopUpTimer;

	private DispatcherTimer _resetStreamingFlagTimer;

	private AutoProtectNotificationBalloonViewModel _autoProtectNotificationBalloonViewModel;

	private string _connectedNetworkName;

	private bool _isStreamingUpdatingLocationFlag;

	public FeedbackWindow FeedbackWindow;

	public int Score;

	public IUpdateService UpdateService;

	public bool IsBeforeConnectVisible;

	public IPreferencesService PreferencesService;

	public INetworkService NetworkService;

	public ITrustedNetworkService TrustedNetworkService;

	public IAdminPermissionsRunnerService AdminPermissionsRunnerService;

	public IBeforeConnectWindowWrapper BeforeConnectWindowWrapper;

	public ISubscriptionInfo SubscriptionInfo;

	public SplitTunnelingMainWindow SplitTunnelingMainWindow;

	public SDKMonitor SdkObject { get; set; }

	public IExpandedGetHelpViewModel GetHelpViewModel { get; set; }

	public IExpandedTermsAndPoliciesViewModel TermsAndPoliciesViewModel { get; set; }

	public ExpandedAccountViewModel AccountViewModel { get; set; }

	public SettingsMainControlViewModel SettingsControlViewModel { get; set; }

	public bool WasClosed { get; set; }

	public VPNWindowExpanded(SDKMonitor sdk, IShowFeedbackWindowService feedbackWindowService, IBugsnagService bugsnagService, IAppLogger logger, IAppSettingsHelper appSettingsHelper, IUninstallRegistryVersionUpdater uninstallRegistryVersionUpdater)
	{
		SdkObject = sdk;
		NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
		InitializeComponent();
		SetTimers();
		_feedbackWindowService = feedbackWindowService;
		_bugsnagService = bugsnagService;
		_logger = logger;
		_appSettingsHelper = appSettingsHelper;
		InitWindowDependencies();
		SystemEvents.PowerModeChanged += SystemEvents_PowerModeChangedAsync;
		uninstallRegistryVersionUpdater.UpdateDisplayVersion();
		InitializeSdkValues();
		SingleProtocolSDKSet();
		GlobalEvents.StyleChanged += ExpandedWndStyleChanged;
		base.Closed += OnWindowClosed;
		SubscribeToConnectionUi();
	}

	private void OnWindowClosed(object sender, EventArgs e)
	{
		GlobalEvents.StyleChanged -= ExpandedWndStyleChanged;
	}

	private void SubscribeToConnectionUi()
	{
		SdkObject.ConnectionUiChanged += OnConnectionUiChanged;
		SdkObject.AdvancedSettingsEnableChanged += delegate(bool isEnabled)
		{
			base.Dispatcher.Invoke(delegate
			{
				GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.EnableSettings(isEnabled);
			});
		};
	}

	private void OnConnectionUiChanged(ConnectionUiEvent uiEvent)
	{
		switch (uiEvent)
		{
		case ConnectionUiEvent.Connecting:
			ShowConnectingScreen();
			break;
		case ConnectionUiEvent.Connected:
			ShowConnectedScreen();
			break;
		case ConnectionUiEvent.ShuffleIpAvailability:
			base.Dispatcher.Invoke(delegate
			{
				Mainpanel.ConnectionDataViewModel.ShuffleIpVisibility = ((!SdkObject.HasMultipleServersForCurrentLocation()) ? Visibility.Collapsed : Visibility.Visible);
			});
			break;
		case ConnectionUiEvent.Disconnected:
			ShowDisconnectedScreen();
			break;
		case ConnectionUiEvent.ConnectionFailed:
			ShowConnectionFailedState();
			break;
		case ConnectionUiEvent.FavoritesConnectError:
			base.Dispatcher.Invoke(delegate
			{
				Mainpanel.PrivateFavoriteLocationControl.ErrorConnectedAsync();
				Mainpanel.HideFavoriteControlsIfVisible();
			});
			break;
		case ConnectionUiEvent.ReconnectSpinner:
			base.Dispatcher.Invoke(delegate
			{
				Mainpanel.SpinnerAnimation(animate: true);
			});
			break;
		case ConnectionUiEvent.ErrorScreenShown:
			base.Dispatcher.Invoke(delegate
			{
				Mainpanel.ReconnectingBlink.Visibility = Visibility.Collapsed;
				Mainpanel.ImageReconnecting.Visibility = Visibility.Collapsed;
				Mainpanel.VpnError();
			});
			break;
		case ConnectionUiEvent.ErrorScreenHidden:
			HideErrorScreenState();
			break;
		}
	}

	private void ShowConnectingScreen()
	{
		base.Dispatcher.Invoke(delegate
		{
			Mainpanel.SetConnectedStatusScreen("connecting");
			Mainpanel.ConnectionError.ViewModel.ControlVisibility = Visibility.Collapsed;
			Mainpanel.NetworkInfoShow(Visibility.Collapsed);
			Mainpanel.SpinnerAnimation(animate: true);
			SetLocationsCountryFlag(ConnectionStatus.Connecting);
			GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.SetProtocolEnable(value: false);
			if (base.IsVisible && SdkObject.ConnectFromFavorites)
			{
				Mainpanel.PrivateFavoriteLocationControl.ConnectAnimationAsync(connected: false);
			}
		});
	}

	private void ShowConnectedScreen()
	{
		base.Dispatcher.Invoke(delegate
		{
			Mainpanel.ConnectionDataViewModel.ControlVisibility = Visibility.Visible;
			Mainpanel.AnimationConnect(connected: true);
			Mainpanel.SpinnerAnimation(animate: false);
			Mainpanel.SetConnectedStatusScreen("connected");
			GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.SetProtocolEnable(value: false);
			if (base.IsVisible && Mainpanel.PrivateFavoriteLocationControl.IsVisible)
			{
				Mainpanel.PrivateFavoriteLocationControl.ConnectAnimationAsync(connected: true);
				Mainpanel.HideFavoriteControlsIfVisible();
			}
			Mainpanel.BtnConnectColor(connected: true);
			SetLocationsCountryFlag(ConnectionStatus.Connected);
		});
		base.Dispatcher.Invoke(delegate
		{
			Mainpanel.NotificationControlViewModel.UserControlVisibility = Visibility.Collapsed;
			Mainpanel.ConnectionError.ViewModel.ControlVisibility = Visibility.Collapsed;
			Mainpanel.ConnectionDataViewModel.Protocol = SdkObject.SelectedProtocol;
		});
	}

	private void ShowDisconnectedScreen()
	{
		base.Dispatcher.Invoke(delegate
		{
			Mainpanel.ConnectionDataViewModel.ControlVisibility = Visibility.Collapsed;
			SetLocationsCountryFlag(ConnectionStatus.Disconnected);
			GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.SetProtocolEnable(value: true);
			Mainpanel.SetConnectedStatusScreen("disconnected");
			Mainpanel.AnimationConnect(connected: false);
			if (Mainpanel.ConnectionError.ViewModel.ControlVisibility == Visibility.Visible)
			{
				SdkObject.StatusButton = "Reconnect";
				Mainpanel.NetworkInfoShow(Visibility.Collapsed);
			}
		});
	}

	private void ShowConnectionFailedState()
	{
		base.Dispatcher.Invoke(delegate
		{
			if (!SdkObject.IsReconnecting)
			{
				Mainpanel.SpinnerAnimation(animate: false);
			}
			Mainpanel.NetworkInfoShow(Visibility.Collapsed);
			GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.SetProtocolEnable(value: true);
		});
	}

	private void HideErrorScreenState()
	{
		base.Dispatcher.Invoke(delegate
		{
			Mainpanel.ConnectionError.ViewModel.ControlVisibility = Visibility.Collapsed;
			Mainpanel.NetworkType.Visibility = Visibility.Visible;
			Mainpanel.NetworkTypeImage.Visibility = Visibility.Visible;
			Mainpanel.NetworkDescription.Visibility = Visibility.Visible;
			SdkObject.StatusButton = "Connect VPN";
		});
	}

	private void SetLocationsCountryFlag(ConnectionStatus status)
	{
		ExpandedLocations.FavoriteLocationsTabControl.SetCountryFlagImageStatus(status);
		ExpandedLocations.AllLocationsControl.SetCountryFlagImageStatus(status);
	}

	private void ExpandedWndStyleChanged(NextAiVPN.Services.Persistence.Style arg1, NextAiVPN.Services.Persistence.Style arg2)
	{
		SetTrademarkLogo();
	}

	public void SetTrademarkLogo()
	{
		BrandImage.Visibility = Visibility.Collapsed;
	}

	public void SetDependencies(AutoProtectNotificationBalloonViewModel autoProtectNotificationBalloonViewModel)
	{
		_autoProtectNotificationBalloonViewModel = autoProtectNotificationBalloonViewModel;
	}

	public void SetTimers()
	{
		_balloonTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(5000L)
		};
		_balloonTimer.Tick += _balloonTimer_Tick;
		_internetConnectionTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(10000L)
		};
		_internetConnectionTimer.Tick += _internetConnectionTimer_Tick;
		_resetStreamingFlagTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMinutes(1L)
		};
		_resetStreamingFlagTimer.Tick += _resetStreamingFlagTimer_Tick;
	}

	private void SingleProtocolSDKSet()
	{
		SdkObject.OpenVpnScramble = _appSettingsHelper.GetValue("scramble").Equals("1");
		if (_appSettingsHelper.GetValue("protocol").Equals("IKEv2"))
		{
			SdkObject.SetProtocol(1, null);
			SdkObject.OpenVpnScramble = false;
		}
		else if (_appSettingsHelper.GetValue("protocol").Equals("OpenVPN"))
		{
			if (_appSettingsHelper.GetValue("protocolType").Equals("TCP"))
			{
				SdkObject.SetProtocol(2, "tcp");
			}
			else
			{
				SdkObject.SetProtocol(2, "udp");
			}
		}
	}

	private void InitializeSdkValues()
	{
		SdkObject.NextAiVpnSdkManager.AllowOnlyVPNConnectivity = _appSettingsHelper.GetValue("KillSwitch").Equals("1");
		SdkObject.NextAiVpnSdkManager.AllowLANTraffic = !_appSettingsHelper.GetValue("BlockLAN").Equals("1");
	}

	private async void SystemEvents_PowerModeChangedAsync(object sender, PowerModeChangedEventArgs e)
	{
		if (e.Mode == PowerModes.Suspend && SdkObject != null)
		{
			await SdkObject.DisconnectVPN();
		}
	}

	private void UpdateVersionCheckOnQuit()
	{
		try
		{
			_appSettingsHelper.SetValue("LastVersionCheck", string.Empty);
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify("Error saving Version Check Time on Quit: " + ex.Message);
			_logger?.Error(ex, "UpdateVersionCheckOnQuit", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\VPNWindowExpanded.xaml.cs", 357);
		}
	}

	private void InitWindowDependencies()
	{
		Mainpanel.GetVpnWindows(this);
		Mainpanel.GetSdkObject(SdkObject);
		ExpandedLocations.GetSdkObject(SdkObject);
		Mainpanel.PrivateFavoriteLocationControl.ShowSections();
		ExpandedSideMenu.GetExpandedWindow(this);
		LoginError.GetExpandedWindow(this);
		AdvanceViewLogs.GetSdk(SdkObject);
	}

	public void BindViewModels()
	{
		AccountControl.DataContext = AccountViewModel;
		GetHelpControl.DataContext = GetHelpViewModel;
		TermsAndPoliciesControl.DataContext = TermsAndPoliciesViewModel;
	}

	public void SetConnectedNetwork()
	{
		_connectedNetworkName = NetworkService.GetConnectedNetworkName();
		if (string.IsNullOrEmpty(_connectedNetworkName))
		{
			_connectedNetworkName = NetworkService.GetConnectedNetworkName();
		}
		GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ConnectedNetwork = _connectedNetworkName;
		SetTrustedNetworkBalloon();
	}

	private void _balloonTimer_Tick(object sender, EventArgs e)
	{
		_autoProtectNotificationBalloonViewModel.ContextMenu.IsOpen = false;
		_balloonTimer.Stop();
	}

	private async void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
	{
		await CheckStreamingVpnConnection();
		_internetConnectionTimer.Start();
		SetTrustedNetworkBalloon();
	}

	private async Task CheckStreamingVpnConnection()
	{
		if (SdkObject.IsStreamingConnected && NetworkInterface.GetIsNetworkAvailable() && InternetConnection.IsAvailable() && !_isStreamingUpdatingLocationFlag && !SdkObject.StreamingSdk.IsActive)
		{
			_isStreamingUpdatingLocationFlag = true;
			IStreamingLocation location = SdkObject.StreamingLocation;
			_logger?.Information("Reconnecting to the streaming server. Location: " + location.FullLocationName, "CheckStreamingVpnConnection", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\VPNWindowExpanded.xaml.cs", 428);
			await SdkObject.GetStreamingLocations();
			SdkObject.StreamingLocation = SdkObject.StreamingLocations.First((IStreamingLocation x) => x.FullLocationName.Equals(location.FullLocationName));
			await SdkObject.DisconnectVPN();
			await SdkObject.ConnectToVPN(VpnType.Streaming);
			_resetStreamingFlagTimer.Start();
		}
	}

	private void _resetStreamingFlagTimer_Tick(object sender, EventArgs e)
	{
		_isStreamingUpdatingLocationFlag = false;
		_logger?.Information("Reconnecting to the streaming server. variable reseted: _isStreamingUpdatingLocationFlag = false", "_resetStreamingFlagTimer_Tick", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\VPNWindowExpanded.xaml.cs", 445);
		_resetStreamingFlagTimer.Stop();
	}

	private void _internetConnectionTimer_Tick(object sender, EventArgs e)
	{
		_internetConnectionTimer.Stop();
		if (!InternetConnection.IsAvailable() && NoNetworkExpanded.Visibility != Visibility.Visible)
		{
			SdkObject.SetNetworkStatus();
		}
		if (InternetConnection.IsAvailable() && NoNetworkExpanded.Visibility == Visibility.Visible)
		{
			HideNoConnection();
		}
		else
		{
			if (!GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AutoProtectToggle.Toggled1 || SdkObject.NextAiVpnSdkManager.IsConnected || SdkObject.NextAiVpnSdkManager.IsConnecting)
			{
				return;
			}
			if (!SdkObject.ConnectToUntrustedNetwork)
			{
				SdkObject.ConnectToUntrustedNetwork = true;
				return;
			}
			string connectedNetworkName = NetworkService.GetConnectedNetworkName();
			if (string.IsNullOrEmpty(connectedNetworkName))
			{
				connectedNetworkName = NetworkService.GetConnectedNetworkName();
			}
			if (!string.IsNullOrEmpty(connectedNetworkName) && !TrustedNetworkService.Contain(connectedNetworkName))
			{
				SdkObject.ConnectToVPN(SdkObject.NextAiVpnLocation);
				SdkObject.ConnectToUntrustedNetwork = false;
			}
		}
	}

	private void SetTrustedNetworkBalloon()
	{
		try
		{
			GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.TrustedNetworksWindowViewModel.SortTrustedNetworks();
			GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.TrustedNetworksWindowViewModel.ShowTrustedNetworkPanels();
			if (GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AutoProtectToggle.Toggled1 || (!GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.AutoProtectToggle.Toggled1 && !GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.NetworkDetectionNotificationToggle.Toggled1))
			{
				return;
			}
			Application.Current.Dispatcher.Invoke(delegate
			{
				string connectedNetworkName = NetworkService.GetConnectedNetworkName();
				if (!string.IsNullOrEmpty(connectedNetworkName) && !connectedNetworkName.ToLower().Equals("WireGuard Tunnel".ToLower()) && !connectedNetworkName.ToLower().Contains("NextAiVpn".ToLower()) && !connectedNetworkName.ToLower().Equals("nextaivpn-b2c92a50-ee99-41ba-8e1a-7285170d1190".ToLower()))
				{
					if (connectedNetworkName.ToLower().Contains("Identifying...".ToLower()))
					{
						Thread.Sleep(300);
						connectedNetworkName = NetworkService.GetConnectedNetworkName();
						if (connectedNetworkName.ToLower().Contains("Identifying...".ToLower()))
						{
							return;
						}
					}
					if (!GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.TrustedNetworksWindowViewModel.IsContainNetwork(connectedNetworkName))
					{
						_connectedNetworkName = connectedNetworkName;
						_balloonTimer.Start();
						_autoProtectNotificationBalloonViewModel.UserControlVisibility = Visibility.Visible;
						try
						{
							_autoProtectNotificationBalloonViewModel.NetworkName = connectedNetworkName;
							_autoProtectNotificationBalloonViewModel.Header = string.Format(CultureInfo.InvariantCulture, "Untrusted network detected\n\"{0}\"", connectedNetworkName);
							ShowBalloonNotification(new AutoProtectNotificationBalloon(_autoProtectNotificationBalloonViewModel), 10000);
						}
						catch (Exception ex)
						{
							_bugsnagService.Notify("VPNWindowExpanded.SetTrustedNetworkBalloon" + ex.Message);
							_logger?.Error(ex, "SetTrustedNetworkBalloon", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\VPNWindowExpanded.xaml.cs", 582);
						}
					}
				}
			});
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "SetTrustedNetworkBalloon", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\VPNWindowExpanded.xaml.cs", 588);
		}
	}

	public void ShowBalloonNotification(UserControl balloonNotification, int timeToShow)
	{
		SdkObject.TaskBarService.TaskbarIcon.ShowCustomBalloon(balloonNotification, PopupAnimation.Slide, timeToShow);
	}

	public void ShowNoConnection()
	{
		NoNetworkExpanded.Visibility = Visibility.Visible;
		Mainpanel.Visibility = Visibility.Collapsed;
		SdkObject.TaskBarService.ContextMenuHide("quit", "sign out");
	}

	public void HideNoConnection()
	{
		Mainpanel.Visibility = Visibility.Visible;
		NoNetworkExpanded.Visibility = Visibility.Collapsed;
		SdkObject.TaskBarService.ContextMenuShow(string.Empty);
	}

	public bool AllowClose { get; set; } = false;

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		try
		{
			if (!AllowClose)
			{
				e.Cancel = true;
				this.Hide();
				this.WindowState = WindowState.Minimized;
				WasClosed = true;
				if (SdkObject?.TaskBarService?.TaskbarIcon != null)
				{
					SdkObject.TaskBarService.TaskbarIcon.Visibility = Visibility.Visible;
				}
				return;
			}
			System.Windows.Application.Current.Shutdown();
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "Window_Closing", "VPNWindowExpanded.cs", 637);
		}
	}

	private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		try
		{
			if (!(bool)e.OldValue && (bool)e.NewValue)
			{
				FeedbackCheck();
				if (SdkObject.ButtonEnable && SdkObject.AllCitiesPing)
				{
					SdkObject.RefreshLocationsIfNeeded();
				}
			}
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify("[Expanded_IsVisibleChanged] There was a error in the verification: " + ex.Message);
			_logger?.Error(ex, "Window_IsVisibleChanged", "VPNWindowExpanded.cs", 662);
		}
	}

	public void FeedbackCheck()
	{
		try
		{
			if (_feedbackWindowService.NeedToShow())
			{
				Mainpanel.FeedbackPopUpControlViewModel.UserControlVisibility = Visibility.Visible;
			}
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify("[LastFeedbackSent] There was a error verifying last feedback Sent: " + ex.Message);
			_logger?.Error(ex, "FeedbackCheck", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\VPNWindowExpanded.xaml.cs", 678);
		}
	}

	public void ShowFeedbackWindow()
	{
		if (!FeedbackWindow.IsVisible)
		{
			FeedbackWindow.ShowDialog();
			return;
		}
		FeedbackWindow.Topmost = true;
		FeedbackWindow.WindowState = WindowState.Normal;
		FeedbackWindow.Visibility = Visibility.Visible;
		FeedbackWindow.Focus();
	}

	public void ShowProblemReportWindow(string message)
	{
		FeedbackWindow.ShowIssueTab();
		FeedbackControlViewModel obj = FeedbackWindow.FeedbackControl.DataContext as FeedbackControlViewModel;
		obj.IssueText = message;
		obj.IssueCharacterCounter = obj.IssueText.Length.ToString();
		FeedbackWindow.ShowDialog();
		FeedbackWindow.ShowFeedbackTab();
	}

	private void VPNWindowExpanded_OnLoaded(object sender, RoutedEventArgs e)
	{
		WindowHeader.GetParent(this);
		ShowPopUps();
	}

	private void _subscriptionPopUpTimer_Tick(object sender, EventArgs e)
	{
		try
		{
			_subscriptionPopUpTimer.Stop();
			if (SubscriptionInfo?.Subscription != null && !SubscriptionInfo.IsTrial && SubscriptionInfo.IsSubscriptionExpireSoon && !SubscriptionInfo.Subscription.Autorenewal)
			{
				Mainpanel.SubscriptionExpireSoonControlViewModel.UserControlVisibility = Visibility.Visible;
			}
		}
		catch (Exception ex)
		{
			Utils.Logger?.Error(ex, "_subscriptionPopUpTimer_Tick", "VPNWindowExpanded.cs", 615);
		}
	}

	private void ShowPopUps()
	{
		ShowSubscriptionPopUp();
	}

	private void ShowSubscriptionPopUp()
	{
		_subscriptionPopUpTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(3L)
		};
		_subscriptionPopUpTimer.Tick += _subscriptionPopUpTimer_Tick;
		_subscriptionPopUpTimer.Start();
	}

	private void VPNWindowExpanded_OnKeyDown(object sender, KeyEventArgs e)
	{
	}

	private void VPNWindowExpanded_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		Mainpanel.Connect.Focus();
	}

	private void VPNWindowExpanded_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		Mainpanel.PrivateFavoriteLocationControl.Visibility = Visibility.Collapsed;
		Mainpanel.StreamingFavoriteLocationControl.Visibility = Visibility.Collapsed;
	}
}
