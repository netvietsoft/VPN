using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NextAiVPN.Common;
using NextAiVPN.Enums;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.NotifyBalloons;

namespace NextAiVPN.UI.AutoProtectNotificationBallons;

public class AutoProtectNotificationBalloonViewModel : ViewModelBase
{
	private readonly VPNWindowExpanded _vpnWindowExpanded;

	private readonly IApiClient _apiClient;

	private Visibility _userControlVisibility;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public ContextMenu ContextMenu { get; set; }

	public string Header { get; set; }

	public string Message { get; set; }

	public string NetworkName { get; set; }

	public string ImageSource { get; set; }

	public Visibility UserControlVisibility
	{
		get
		{
			return _userControlVisibility;
		}
		set
		{
			_userControlVisibility = value;
			if (_userControlVisibility == Visibility.Collapsed)
			{
				ContextMenu.IsOpen = false;
			}
			OnPropertyChanged("UserControlVisibility");
		}
	}

	public ICommand CloseButtonClickCommand { get; set; }

	public ICommand ContextMenuClickCommand { get; set; }

	public ICommand AddTrustedNetworkClickCommand { get; set; }

	public ICommand ConnectToVpnClickCommand { get; set; }

	public AutoProtectNotificationBalloonViewModel(VPNWindowExpanded vpnWindowExpanded, IApiClient apiClient, IAppSettingsHelper appSettingsHelper)
	{
		GlobalEvents.StyleChanged += StyleChanged;
		_vpnWindowExpanded = vpnWindowExpanded;
		_apiClient = apiClient;
		_appSettingsHelper = appSettingsHelper;
		CloseButtonClickCommand = new RelayCommand(CloseButtonClickCommandExecute);
		ContextMenuClickCommand = new RelayCommand(ContextMenuClickCommandExecute);
		AddTrustedNetworkClickCommand = new RelayCommand(AddTrustedNetworkClickCommandExecute);
		ConnectToVpnClickCommand = new RelayCommand(ConnectToVpnClickCommandExecute);
		SetContextMenu();
		ChangeStyle(StyleModeDefiner.DefineAppStyle());
	}

	private void SetContextMenu()
	{
		MenuItem menuItem = new MenuItem
		{
			Header = "Don’t ask again"
		};
		menuItem.Click += AlwaysAskMenuItem_Click;
		MenuItem menuItem2 = new MenuItem
		{
			Header = "Config in NextAiVPN"
		};
		menuItem2.Click += ManegeSettings_Click;
		ContextMenu = new ContextMenu();
		ContextMenu.Items.Add(menuItem);
		ContextMenu.Items.Add(menuItem2);
	}

	private void ChangeStyle(NextAiVPN.Services.Persistence.Style appStyle)
	{
		System.Windows.Style style = new System.Windows.Style();
		try
		{
			switch (appStyle)
			{
			case NextAiVPN.Services.Persistence.Style.Dark:
				style = Application.Current.FindResource("ContextMenuStyle") as System.Windows.Style;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case NextAiVPN.Services.Persistence.Style.Light:
			case NextAiVPN.Services.Persistence.Style.Skip:
				break;
			}
		}
		catch (Exception)
		{
		}
		ContextMenu.Style = style;
	}

	private void StyleChanged(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style sysStyle)
	{
		ChangeStyle(appStyle);
	}

	private void ConnectToVpnClickCommandExecute(object obj)
	{
		if (!_vpnWindowExpanded.SdkObject.ConnectionStatus.ToLower().Equals("connected") || !_vpnWindowExpanded.SdkObject.ConnectionStatus.ToLower().Equals("connecting"))
		{
			_vpnWindowExpanded.SdkObject.ConnectToVPN(_vpnWindowExpanded.SdkObject.NextAiVpnLocation);
			_apiClient.SetIsBestAvailable((_vpnWindowExpanded.SdkObject.NextAiVpnLocation.Id == "bestavailable") ? "1" : "0");
			UserControlVisibility = Visibility.Collapsed;
		}
	}

	private void DisplaySettings()
	{
		_vpnWindowExpanded.Visibility = Visibility.Visible;
		_vpnWindowExpanded.ExpandedSideMenu.SetMenuOption(SideMenuOption.Settings);
		_vpnWindowExpanded.WindowState = WindowState.Normal;
	}

	private void ManegeSettings_Click(object sender, RoutedEventArgs e)
	{
		DisplaySettings();
	}

	private void AlwaysAskMenuItem_Click(object sender, RoutedEventArgs e)
	{
		_vpnWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.NetworkDetectionNotificationToggle.Toggled1 = false;
		_vpnWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.NetworkDetectionNotificationToggle.Dot_InitializeState();
		_appSettingsHelper.SetValue("IsNeedSendAutoProtectNotification", "0");
		UserControlVisibility = Visibility.Collapsed;
	}

	private void AddTrustedNetworkClickCommandExecute(object obj)
	{
		if (!string.IsNullOrEmpty(NetworkName) && !_vpnWindowExpanded.TrustedNetworkService.Contain(NetworkName))
		{
			_vpnWindowExpanded.TrustedNetworkService.Add(NetworkName);
			_vpnWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.TrustedNetworksWindowViewModel.AddTrustedNetwork(NetworkName);
			_vpnWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.TrustedNetworksWindowViewModel.ShowTrustedNetworkPanels();
			UserControlVisibility = Visibility.Collapsed;
			_vpnWindowExpanded.ShowBalloonNotification(new NotifyBalloon(new NotifyBalloonViewModel
			{
				Header = string.Format(CultureInfo.InvariantCulture, "\"{0}\" was successfully added as trusted network", NetworkName),
				Message = "Now enjoy private, secure browsing with NextAiVPN",
				ImageSource = "/Assets/check.png"
			}), 5000);
		}
	}

	private void ContextMenuClickCommandExecute(object obj)
	{
		if (!ContextMenu.IsOpen)
		{
			ContextMenu.IsOpen = true;
		}
	}

	private void CloseButtonClickCommandExecute(object obj)
	{
		UserControlVisibility = Visibility.Collapsed;
	}
}
