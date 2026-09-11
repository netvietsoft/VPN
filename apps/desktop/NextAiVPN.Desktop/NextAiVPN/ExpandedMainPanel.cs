using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using NextAiVPN.Enums.Streaming;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.Account.PopUp;
using NextAiVPN.UI.Feedback.PopUp;
using NextAiVPN.UI.MainPanelConnectionData;
using NextAiVPN.UI.MainPanelNotification;
using NextAiVPN.UI.SplitTunneling.PopUp;

namespace NextAiVPN;

public partial class ExpandedMainPanel : UserControl, IComponentConnector
{
	private int _networkType;

	private VPNWindowExpanded _vpnExpandedWindow;

	private readonly MenuItem _connectDisconnectOption = new MenuItem();

	private readonly DoubleAnimation _da = new DoubleAnimation();

	private readonly RotateTransform _rt = new RotateTransform();

	public SDKMonitor SdkObject { get; private set; }

	public IFeedbackPopUpControlViewModel FeedbackPopUpControlViewModel { get; set; }

	public MainPanelNotificationControlViewModel NotificationControlViewModel { get; set; }

	public SubscriptionExpireSoonControlViewModel SubscriptionExpireSoonControlViewModel { get; set; }

	public SplitTunnelingReconnectPopUpViewModel SplitTunnelingReconnectPopUpViewModel { get; set; }

	private ConnectionDataViewModel _connectionDataViewModel;
	internal ConnectionDataViewModel ConnectionDataViewModel
	{
		get => _connectionDataViewModel;
		set
		{
			_connectionDataViewModel = value;
			if (ConnectionData != null && value != null)
			{
				ConnectionData.DataContext = value;
			}
		}
	}

	public ExpandedMainPanel()
	{
		InitializeComponent();
		base.Loaded += OnStyleLoaded;
		base.Unloaded += OnStyleUnloaded;
		FeedbackPopUpControlViewModel = new FeedbackPopUpControlViewModel(Utils.AppSettingsHelper);
		FeedbackPopUpControl.DataContext = FeedbackPopUpControlViewModel;
		SubscriptionExpireSoonControlViewModel = new SubscriptionExpireSoonControlViewModel();
		SubscriptionExpireSoon.DataContext = SubscriptionExpireSoonControlViewModel;
		SplitTunnelingReconnectPopUpViewModel = new SplitTunnelingReconnectPopUpViewModel();
		SplitTunnelingReconnectPopUp.DataContext = SplitTunnelingReconnectPopUpViewModel;
	}

	public void GetSdkObject(SDKMonitor sdk)
	{
		SdkObject = sdk;
		PrivateFavoriteLocationControl.GetSdkObject(SdkObject);
		PrivateFavoriteLocationControl.GetVpnMainWindowExpanded(_vpnExpandedWindow);
		SingleProtocolSDKSet();
		SdkObject.SetLastConnectedFlag();
		NotificationControlViewModel = new MainPanelNotificationControlViewModel(_vpnExpandedWindow);
		MainPanelNotification.DataContext = NotificationControlViewModel;
		FeedbackPopUpControlViewModel.ExpandedWindow = _vpnExpandedWindow;
		SubscriptionExpireSoonControlViewModel.ExpandedWindow = _vpnExpandedWindow;
		SplitTunnelingReconnectPopUpViewModel.SdkMonitor = SdkObject;
		VpnModeChangeEvent.OnVpnModeChanged = (Action<VpnType>)Delegate.Combine(VpnModeChangeEvent.OnVpnModeChanged, new Action<VpnType>(OnVpnModeChanged));
	}

	public void EnableConnectButton()
	{
		Connect.IsEnabled = true;
	}

	public void DisableConnectButton()
	{
		Connect.IsEnabled = false;
	}

	private void OnVpnModeChanged(VpnType obj)
	{
		switch (obj)
		{
		case VpnType.NextAiVPN:
			if (SdkObject.IsNextAiVpnModeAvailable)
			{
				SdkObject.HideLoginError();
			}
			else
			{
				SdkObject.ShowLoginError(SdkObject.NextAiVpnModeErrorMessage);
			}
			TopImage.Source = new BitmapImage(new Uri("/Assets/vpn-background.png", UriKind.Relative));
			EnableConnectButton();
			SdkObject.VpnExpandedWindow.ExpandedLocations.StreamingLocations.StreamingLocationsViewModel.ShowFavoriteControl();
			break;
		case VpnType.Streaming:
			SdkObject.HideLoginError();
			TopImage.Source = new BitmapImage(new Uri("/Assets/vpn-background-streaming.png", UriKind.Relative));
			if (SdkObject.StreamingSdk.Locations.Count > 0)
			{
				EnableConnectButton();
				SdkObject.VpnExpandedWindow.ExpandedLocations.StreamingLocations.StreamingLocationsViewModel.ShowFavoriteControl();
			}
			else
			{
				DisableConnectButton();
				SdkObject.VpnExpandedWindow.ExpandedLocations.StreamingLocations.StreamingLocationsViewModel.HideFavoriteControl();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("obj", obj, null);
		}
	}

	private void SetFavoriteLocationBorder(VpnType vpnMode)
	{
		switch (vpnMode)
		{
		case VpnType.NextAiVPN:
			PrivateFavoriteLocationControl.LoadOnlyBestAvailable();
			PrivateFavoriteLocationControl.Visibility = Visibility.Visible;
			PrivateFavoriteLocationControl.AddMoreSection.Visibility = Visibility.Collapsed;
			StreamingFavoriteLocationControl.Visibility = Visibility.Collapsed;
			break;
		case VpnType.Streaming:
			StreamingFavoriteLocationControl.Visibility = Visibility.Visible;
			PrivateFavoriteLocationControl.Visibility = Visibility.Collapsed;
			break;
		default:
			throw new ArgumentOutOfRangeException("vpnMode", vpnMode, null);
		}
	}

	public void GetVpnWindows(VPNWindowExpanded expanded)
	{
		_vpnExpandedWindow = expanded;
	}

	private void ShowBeforeConnect()
	{
		HideFavoriteControlsIfVisible();
		_vpnExpandedWindow.Hide();
		_vpnExpandedWindow.BeforeConnectWindowWrapper.Show();
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		base.DataContext = SdkObject;
		SetNetworkInfo();
		InitializeSDKValues();
		if (SdkObject.NextAiVpnSdkManager.IsConnecting)
		{
			NetworkInfoShow(Visibility.Collapsed);
		}
		if (SdkObject.NextAiVpnSdkManager.IsConnected || SdkObject.IsError)
		{
			NetworkInfoShow(Visibility.Collapsed);
			if (SdkObject.NextAiVpnLocation.CountryCode != "bestavailable")
			{
				PrivateFavoriteLocationControl.IsFavorites = true;
			}
			else
			{
				PrivateFavoriteLocationControl.IsBestAvailable = true;
			}
			PrivateFavoriteLocationControl.ConnectAnimationAsync(connected: false);
			SpinnerAnimation(animate: false);
		}
		if (!SdkObject.NextAiVpnSdkManager.IsConnected)
		{
			SpinnerAnimation(animate: false);
		}
	}

	private void MainWindowStyleChanged(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style sysStyle)
	{
		StyleChange(appStyle);
	}

	private void OnStyleLoaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged += MainWindowStyleChanged;
	}

	private void OnStyleUnloaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged -= MainWindowStyleChanged;
	}

	private void SetMainImageGridBackground(bool isConnected)
	{
		if (isConnected)
		{
			if (IsDarkmode())
			{
				ConnectedGrid.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#0000D072");
				return;
			}
			BackgroundImageGrid.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#0000D072");
			ConnectedGrid.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00D072");
		}
		else
		{
			ConnectedGrid.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#1D1D20");
			if (!IsDarkmode())
			{
				BackgroundImageGrid.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#1D1D20");
			}
		}
	}

	public void StyleChange(NextAiVPN.Services.Persistence.Style appStyle)
	{
		switch (SdkObject.ConnectionStatus.ToLower())
		{
		case "connected":
		{
			SetMainImageGridBackground(isConnected: true);
			string empty = string.Empty;
			SetLocationImageSource(SdkObject.ConnectedVpnMode switch
			{
				VpnType.NextAiVPN => (SdkObject.NextAiVpnSdkManager.ActiveConnectionInformation != null) ? ("/Resources/Flags/" + SdkObject.NextAiVpnSdkManager.ActiveConnectionInformation.Location.CountryCode + ".png") : ("/Resources/Flags/" + SdkObject.NextAiVpnLocation.CountryCode + ".png"), 
				VpnType.Streaming => "/Resources/Flags/" + SdkObject.StreamingLocation.CountryCode + ".png", 
				_ => throw new ArgumentOutOfRangeException(), 
			});
			break;
		}
		case "disconnected":
		{
			SetMainImageGridBackground(isConnected: false);
			string locationImageSource = (appStyle.Equals(NextAiVPN.Services.Persistence.Style.Light) ? "/Resources/Flags/bestavailable.png" : "/Resources/Flags/bestavailable_darkMode.png");
			SetLocationImageSource(locationImageSource);
			break;
		}
		case "error":
			TopImage.Source = new BitmapImage(new Uri(IconHelper.GetIcon("backgroundError"), UriKind.Relative));
			break;
		case "connecting":
			SetConnectButtonSpinnerDuringConnecting();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		switch (appStyle)
		{
		case NextAiVPN.Services.Persistence.Style.Dark:
			if (Connect.Content.Equals("Connect VPN"))
			{
				Connect.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00D072");
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
			}
			else
			{
				Connect.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#848487");
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
			}
			break;
		case NextAiVPN.Services.Persistence.Style.Light:
			if (Connect.Content.Equals("Connect VPN"))
			{
				Connect.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00D072");
			}
			else
			{
				Connect.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#848487");
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SetLocationImageSource(string flagPath)
	{
		base.Dispatcher.Invoke(delegate
		{
			locationImage.Source = new BitmapImage(new Uri(flagPath, UriKind.Relative));
		});
	}

	private void SetConnectButtonSpinnerDuringConnecting()
	{
		string spinnerIconPath = (IsDarkmode() ? "/Assets/spinner_green.png" : "/Assets/spinner_white.png");
		base.Dispatcher.Invoke(delegate
		{
			spinner.Source = new BitmapImage(new Uri(spinnerIconPath, UriKind.Relative));
		});
	}

	private void topImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		HideFavoriteControlsIfVisible();
	}

	private void ErrorVPN_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		HideFavoriteControlsIfVisible();
	}

	private void graypanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		HideFavoriteControlsIfVisible();
	}

	private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		HideFavoriteControlsIfVisible();
	}

	private void ExpandCollapse_MouseEnter(object sender, MouseEventArgs e)
	{
		ExpandCollapse.Opacity = 1.0;
	}

	private void ExpandCollapse_MouseLeave(object sender, MouseEventArgs e)
	{
		ExpandCollapse.Opacity = 0.72;
	}

	private void ExpandCollapse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		HideFavoriteControlsIfVisible();
		_vpnExpandedWindow.Hide();
	}

	private void Connect_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (IsDarkmode())
		{
			SolidColorBrush solidColorBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00D072");
			if (solidColorBrush != null)
			{
				if (Connect.IsEnabled)
				{
					solidColorBrush.Opacity = 1.0;
				}
				else
				{
					solidColorBrush.Opacity = 0.4;
				}
				Connect.Foreground = solidColorBrush;
			}
		}
		else if (Connect.IsEnabled)
		{
			Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00B161");
		}
		else if (SdkObject.NextAiVpnSdkManager.IsConnected)
		{
			Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#959596");
		}
		else if (SdkObject.LocationsRefreshing)
		{
			Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00B161");
		}
		else
		{
			Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#B2F0C1");
		}
	}

	private bool IsDarkmode()
	{
		return StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark;
	}

	private async void Connect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ClickCount != 1)
		{
			return;
		}
		if (Connect.Content.Equals("Disconnect"))
		{
			await SdkObject.DisconnectVPN();
			if (SdkObject.ConnectedVpnMode == VpnType.NextAiVPN)
			{
				Utils.Api.SaveConnectedTo("");
			}
		}
		else
		{
			if (SdkObject.NextAiVpnSdkManager.IsConnecting || SdkObject.StreamingSdk.VpnConnectionStatus == StreamingConnectionStatus.Connecting)
			{
				return;
			}
			if (Utils.AppSettingsHelper.GetValue("IUnderstand").Equals("0"))
			{
				ShowBeforeConnect();
				return;
			}
			SdkObject.ConnectedVpnMode = SdkObject.GetVpnMode();
			SdkObject.ConnectToVPN(SdkObject.NextAiVpnLocation);
			if (SdkObject.ConnectedVpnMode == VpnType.NextAiVPN)
			{
				Utils.Api.SetIsBestAvailable((SdkObject.NextAiVpnLocation.Id == "bestavailable") ? "1" : "0");
			}
		}
	}

	private void Connect_MouseEnter(object sender, MouseEventArgs e)
	{
		SetDisconnectButtonBackground();
	}

	public void SetDisconnectButtonBackground()
	{
		if (!IsDarkmode())
		{
			if (Connect.Content.Equals("Disconnect"))
			{
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#787879");
			}
			else if (Connect.IsEnabled && !Connect.Content.Equals(""))
			{
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00B161");
			}
		}
	}

	private void Connect_MouseLeave(object sender, MouseEventArgs e)
	{
		if (!IsDarkmode())
		{
			if (Connect.Content.Equals("Disconnect"))
			{
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#959596");
			}
			else
			{
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00D072");
			}
		}
	}

	private void ConnectionButton_Click(object sender, RoutedEventArgs e)
	{
		if (!HideFavoriteControlsIfVisible())
		{
			SetFavoriteLocationBorder(SdkObject.GetVpnMode());
		}
	}

	public bool HideFavoriteControlsIfVisible()
	{
		if (PrivateFavoriteLocationControl.Visibility == Visibility.Visible)
		{
			PrivateFavoriteLocationControl.Visibility = Visibility.Hidden;
			return true;
		}
		if (StreamingFavoriteLocationControl.Visibility == Visibility.Visible)
		{
			StreamingFavoriteLocationControl.Visibility = Visibility.Hidden;
			return true;
		}
		return false;
	}

	private void chevronGrid_MouseEnter(object sender, MouseEventArgs e)
	{
		string colorCode = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark) ? "#E7E7E7" : "#F3F3F3");
		chevroncircle.Opacity = 0.1;
		chevroncircle.Fill = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush(colorCode);
	}

	private void chevronGrid_MouseLeave(object sender, MouseEventArgs e)
	{
		chevroncircle.Fill = Brushes.Transparent;
	}

	public void NetworkInfoShow(Visibility status)
	{
		base.Dispatcher.Invoke(delegate
		{
			if (status == Visibility.Visible)
			{
				SetNetworkInfo();
			}
			NetworkType.Visibility = status;
			NetworkDescription.Visibility = status;
			SetNetworkTypeImage(status);
		});
	}

	private void SetNetworkTypeImage(Visibility visibility)
	{
		int networkType = _networkType;
		NetworkTypeImage.Source = new BitmapImage(new Uri(networkType switch
		{
			0 => "/Assets/defaultNetwork.png", 
			1 => "/Assets/wifi-white.png", 
			_ => "/Assets/ethernet-white.png", 
		}, UriKind.Relative));
		NetworkTypeImage.Visibility = visibility;
	}

	public void SetNetworkInfo()
	{
		try
		{
			NetworkInterface activeNetworkInterface = NetworkService.GetActiveNetworkInterface();
			if (activeNetworkInterface != null)
			{
				NetworkDescription.Text = _vpnExpandedWindow?.NetworkService.GetConnectedNetworkName(activeNetworkInterface);
				switch (activeNetworkInterface.NetworkInterfaceType)
				{
				case NetworkInterfaceType.Ethernet:
					NetworkType.Text = "Ethernet";
					_networkType = 2;
					break;
				case NetworkInterfaceType.Wireless80211:
					NetworkType.Text = "Wi-Fi";
					_networkType = 1;
					break;
				default:
					NetworkType.Text = "Network connected";
					_networkType = 0;
					break;
				}
			}
			SetNetworkTypeImage(Visibility.Visible);
		}
		catch (Exception exception)
		{
			Utils.Logger.Error(exception, "SetNetworkInfo", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\ExpandedMainPanel.xaml.cs", 610);
		}
	}

	public void AnimateLoadingSpinner(bool animate)
	{
		_da.From = 0.0;
		_da.To = 360.0;
		_da.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_da.RepeatBehavior = RepeatBehavior.Forever;
		loadingspinner.RenderTransform = _rt;
		loadingspinner.RenderTransformOrigin = new Point(0.5, 0.5);
		_rt.BeginAnimation(RotateTransform.AngleProperty, animate ? _da : null);
	}

	public void AnimationConnect(bool connected)
	{
		if (connected)
		{
			NetworkInfoShow(Visibility.Collapsed);
			ThicknessAnimation animation = new ThicknessAnimation
			{
				Duration = TimeSpan.FromSeconds(0.3),
				From = new Thickness(0.0, 275.0, 0.0, 0.0),
				To = new Thickness(55.0, 70.0, 0.0, 0.0),
				EasingFunction = new QuarticEase()
			};
			ConnectionData.BeginAnimation(FrameworkElement.MarginProperty, animation);
			ThicknessAnimation animation2 = new ThicknessAnimation
			{
				Duration = TimeSpan.FromSeconds(0.2),
				To = new Thickness(0.0, 0.0, 0.0, 60.0),
				EasingFunction = new QuarticEase()
			};
			TopImage.BeginAnimation(FrameworkElement.MarginProperty, animation2);
			Connect.Margin = new Thickness(0.0, -40.0, 0.0, 0.0);
			SetMainImageGridBackground(connected);
			ThicknessAnimation animation3 = new ThicknessAnimation
			{
				Duration = TimeSpan.FromSeconds(0.2),
				To = new Thickness(0.0, 25.0, 0.0, 0.0),
				EasingFunction = new QuarticEase()
			};
			ConnectButtonPanel.BeginAnimation(FrameworkElement.MarginProperty, animation3);
			ThicknessAnimation animation4 = new ThicknessAnimation
			{
				Duration = TimeSpan.FromSeconds(0.2),
				To = new Thickness(0.0, 245.0, 0.0, 0.0),
				EasingFunction = new QuarticEase()
			};
			RectangleSeparator.BeginAnimation(FrameworkElement.MarginProperty, animation4);
		}
		else
		{
			if (!SdkObject.NextAiVpnSdkManager.IsConnecting)
			{
				NetworkInfoShow(Visibility.Visible);
			}
			ThicknessAnimation animation5 = new ThicknessAnimation
			{
				Duration = TimeSpan.FromSeconds(0.2),
				To = new Thickness(0.0, 364.0, 0.0, 0.0),
				EasingFunction = new QuarticEase()
			};
			RectangleSeparator.BeginAnimation(FrameworkElement.MarginProperty, animation5);
			new ThicknessAnimation
			{
				Duration = TimeSpan.FromSeconds(0.5),
				To = new Thickness(0.0, 275.0, 0.0, 0.0),
				EasingFunction = new QuarticEase()
			};
			ThicknessAnimation animation6 = new ThicknessAnimation
			{
				Duration = TimeSpan.FromSeconds(0.2),
				To = new Thickness(0.0, 0.0, 0.0, 0.0),
				EasingFunction = new QuarticEase()
			};
			TopImage.BeginAnimation(FrameworkElement.MarginProperty, animation6);
			Connect.Margin = new Thickness(0.0, 0.0, 0.0, -85.0);
			SetMainImageGridBackground(connected);
			ThicknessAnimation animation7 = new ThicknessAnimation
			{
				Duration = TimeSpan.FromSeconds(0.2),
				To = new Thickness(0.0, 75.0, 0.0, 0.0),
				EasingFunction = new QuarticEase()
			};
			ConnectButtonPanel.BeginAnimation(FrameworkElement.MarginProperty, animation7);
		}
	}

	public void SpinnerAnimation(bool animate)
	{
		_da.From = 0.0;
		_da.To = 360.0;
		_da.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_da.RepeatBehavior = RepeatBehavior.Forever;
		spinner.RenderTransform = _rt;
		spinner.RenderTransformOrigin = new Point(0.5, 0.5);
		if (animate)
		{
			_rt.BeginAnimation(RotateTransform.AngleProperty, _da);
			return;
		}
		_rt.BeginAnimation(RotateTransform.AngleProperty, null);
		string text = "/Resources/Flags/";
		if (SdkObject.NextAiVpnLocation == null)
		{
			string text2 = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? "bestavailable.png" : "bestavailable_darkMode.png");
			text += text2;
		}
		else if (SdkObject.NextAiVpnLocation.Id == "bestavailable")
		{
			string text3 = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? (SdkObject.NextAiVpnLocation.Id + ".png") : "bestavailable_darkMode.png");
			text += text3;
		}
		else
		{
			text = text + GetCountryCode() + ".png";
		}
		locationImage.Source = new BitmapImage(new Uri(text, UriKind.Relative));
	}

	private string GetCountryCode()
	{
		return SdkObject.ConnectedVpnMode switch
		{
			VpnType.NextAiVPN => SdkObject.NextAiVpnLocation.CountryCode.ToLower(), 
			VpnType.Streaming => SdkObject.StreamingLocation.CountryCode.ToLower(), 
			_ => SdkObject.NextAiVpnLocation.CountryCode.ToLower(), 
		};
	}

	public void SingleProtocolSDKSet()
	{
		SdkObject.OpenVpnScramble = Utils.AppSettingsHelper.GetValue("scramble").Equals("1");
		if (Utils.AppSettingsHelper.GetValue("protocol").Equals("IKEv2"))
		{
			SdkObject.SetProtocol(1, null);
			SdkObject.OpenVpnScramble = false;
		}
		else if (Utils.AppSettingsHelper.GetValue("protocol").Equals("OpenVPN"))
		{
			if (Utils.AppSettingsHelper.GetValue("protocolType").Equals("TCP"))
			{
				SdkObject.SetProtocol(2, "tcp");
			}
			else
			{
				SdkObject.SetProtocol(2, "udp");
			}
		}
	}

	public void InitializeSDKValues()
	{
		SdkObject.NextAiVpnSdkManager.AllowOnlyVPNConnectivity = Utils.AppSettingsHelper.GetValue("KillSwitch").Equals("1");
		SdkObject.NextAiVpnSdkManager.AllowLANTraffic = !Utils.AppSettingsHelper.GetValue("BlockLAN").Equals("1");
	}

	public void BtnConnectColor(bool connected)
	{
		if (connected)
		{
			if (!IsDarkmode())
			{
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#8C8C92");
			}
			_connectDisconnectOption.Header = "Disconnect";
		}
		else
		{
			if (!IsDarkmode())
			{
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#05D480");
			}
			_connectDisconnectOption.Header = "Connect";
		}
	}

	public void SetConnectedStatusScreen(string status)
	{
		UpdatePowerOrbVisual(status);
		switch (status)
		{
		case "connecting":
			SetConnectButtonSpinnerDuringConnecting();
			break;
		case "connected":
			if (IsDarkmode())
			{
				Connect.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#848487");
			}
			else
			{
				ConnectedGrid.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00D072");
			}
			break;
		case "disconnected":
			spinner.Margin = new Thickness(0.0, 0.0, 0.0, -85.0);
			if (IsDarkmode())
			{
				Connect.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00D072");
			}
			else
			{
				Connect.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00B161");
			}
			break;
		case "reconnecting":
			if (IsDarkmode())
			{
				spinner.Source = new BitmapImage(new Uri("/Assets/DarkMode/spinnerGray.png", UriKind.Relative));
			}
			TopImage.Visibility = Visibility.Collapsed;
			ReconnectingAnimation();
			spinner.Margin = new Thickness(0.0, -40.0, 0.0, 0.0);
			break;
		case "error":
			TopImage.Source = new BitmapImage(new Uri(IconHelper.GetIcon("backgroundError"), UriKind.Relative));
			spinner.Margin = new Thickness(0.0, 0.0, 0.0, -85.0);
			break;
		}
	}

	public void ReconnectingAnimation()
	{
		DoubleAnimation doubleAnimation = new DoubleAnimation
		{
			Duration = TimeSpan.FromSeconds(1.5),
			From = 1.0,
			To = 0.25,
			AutoReverse = true,
			RepeatBehavior = RepeatBehavior.Forever,
			EasingFunction = new QuarticEase()
		};
		ReconnectingBlink.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
		if (spinner.Visibility == Visibility.Collapsed)
		{
			doubleAnimation.BeginAnimation(UIElement.OpacityProperty, null);
		}
	}

	public void VpnError()
	{
		NetworkInfoShow(Visibility.Collapsed);
		ConnectionError.ViewModel.ControlVisibility = Visibility.Visible;
	}

	private void ChangeServerBtn_Click(object sender, RoutedEventArgs e)
	{
		_vpnExpandedWindow?.ExpandedSideMenu?.SetMenuOption(NextAiVPN.Enums.SideMenuOption.Location);
	}

	public void UpdatePowerOrbVisual(string status)
	{
		base.Dispatcher.Invoke(delegate
		{
			if (PowerBtnRing == null || PowerBtnInner == null || vpnConnectedText == null) return;

			switch (status.ToLower())
			{
				case "connected":
					PowerBtnRing.BorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#059669");
					PowerBtnInner.Background = new LinearGradientBrush(
						(Color)System.Windows.Media.ColorConverter.ConvertFromString("#059669"),
						(Color)System.Windows.Media.ColorConverter.ConvertFromString("#047857"),
						new Point(0, 0), new Point(1, 1));
					vpnConnectedText.Text = "CONNECTED";
					vpnConnectedText.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#059669");
					_vpnExpandedWindow?.WindowHeader?.SetConnectedStatus(true, "192.111.130.5");
					break;

				case "connecting":
				case "reconnecting":
					PowerBtnRing.BorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#F59E0B");
					PowerBtnInner.Background = new LinearGradientBrush(
						(Color)System.Windows.Media.ColorConverter.ConvertFromString("#F59E0B"),
						(Color)System.Windows.Media.ColorConverter.ConvertFromString("#D97706"),
						new Point(0, 0), new Point(1, 1));
					vpnConnectedText.Text = "CONNECTING...";
					vpnConnectedText.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#D97706");
					_vpnExpandedWindow?.WindowHeader?.SetConnectingStatus();
					break;

				case "disconnected":
				default:
					PowerBtnRing.BorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#DCFCE7");
					PowerBtnInner.Background = new LinearGradientBrush(
						(Color)System.Windows.Media.ColorConverter.ConvertFromString("#059669"),
						(Color)System.Windows.Media.ColorConverter.ConvertFromString("#064E3B"),
						new Point(0, 0), new Point(1, 1));
					vpnConnectedText.Text = "DISCONNECTED";
					vpnConnectedText.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#64748B");
					_vpnExpandedWindow?.WindowHeader?.SetConnectedStatus(false);
					break;
			}
		});
	}
}
