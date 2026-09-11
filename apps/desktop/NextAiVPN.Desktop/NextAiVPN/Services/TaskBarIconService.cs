using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using H.NotifyIcon;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using NextAiVPN.Extensions;
using NextAiVPN.Services.Persistence;
using NextAiVPN.Streaming.Entities;
using NextAiVPN.UI.MessageBoxWindows.SignOutMessageWindow;
using NextAiVPN.UI.Streaming.Balloons;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

public class TaskBarIconService
{
	private readonly IFavoriteLocationsServices _favoriteService;

	private readonly IStyleService _styleService;

	private readonly IBugsnagService _bugsnagService;

	private readonly IApiClient _apiClient;

	private readonly IAnalyticsService _analyticsService;

	private readonly SDKMonitor _sdkMonitor;

	private readonly MenuItem _connectDisconnectOption = new MenuItem();

	private readonly MenuItem _quitOption = new MenuItem();

	private readonly MenuItem _favoriteMenuItem = new MenuItem();

	private readonly MenuItem _streamingMenuItem = new MenuItem();

	private readonly MenuItem _bestAvailableMenuItem = new MenuItem();

	private readonly MenuItem _notificationsMenuItem = new MenuItem();

	private readonly MenuItem _connectingLocationMenuItem = new MenuItem();

	private readonly MenuItem _connectingToItem = new MenuItem();

	private readonly ContextMenu _contextMenu = new ContextMenu();

	private List<SystemTrayFavoriteMenuItem> _favoriteMenuItems;

	private readonly object _lockObject = new object();

	private readonly ICountryFlagService _countryFlagService;

	private readonly SignOutMessageBoxWindow _signOutMessageBoxWindow;

	private readonly IAppLogger _logger;

	private int _clicks;

	private bool _isFirstAfterClosing;

	private bool _isStreamingVisible = true;

	public bool SignOut;

	public bool CloseApp;

	public bool HasNotifications;

	public TaskbarIcon TaskbarIcon { get; set; } = new TaskbarIcon();

	public TaskBarIconService(SDKMonitor sdkMonitor, ICountryFlagService countryFlagService, IFavoriteLocationsServices favoriteLocationsServices, IStyleService styleService, SignOutMessageBoxWindow signOutMessageBoxWindow, IBugsnagService bugsnagService, IApiClient apiClient, IAnalyticsService analyticsService, IAppLogger logger)
	{
		_sdkMonitor = sdkMonitor;
		_bugsnagService = bugsnagService;
		_apiClient = apiClient;
		_analyticsService = analyticsService;
		_logger = logger;
		_favoriteService = favoriteLocationsServices;
		_styleService = styleService;
		_countryFlagService = countryFlagService;
		_signOutMessageBoxWindow = signOutMessageBoxWindow;
		_sdkMonitor.ConnectionNotifier += SdkObject_ConnectionNotifier;
		ContextMenuSet();
		GlobalEvents.StyleChanged += StyleChanged;
		VpnModeChangeEvent.OnVpnModeChanged = (Action<VpnType>)Delegate.Combine(VpnModeChangeEvent.OnVpnModeChanged, new Action<VpnType>(OnVpnModeChanged));
		if (Environment.UserInteractive)
		{
			try
			{
				TaskbarIcon.Icon = ResourceFile.icontray_regular;
				TaskbarIcon.ForceCreate(enablesEfficiencyMode: false);
			}
			catch (Exception ex)
			{
				_logger?.Warning("TaskbarIcon creation failed: " + ex.Message, "TaskBarIconService", "TaskBarIconService.cs", 100);
			}
		}
	}

	private void OnVpnModeChanged(VpnType obj)
	{
	}

	public void HideShowTrayIcon(Visibility value)
	{
		TaskbarIcon.Visibility = value;
	}

	public void SetNotifyIcon(string value, object obj)
	{
		SetNotifyIcon(value, obj, VpnType.NextAiVPN);
	}

	public void SetNotifyIcon(string value, object obj, VpnType vpnType)
	{
		try
		{
			switch (value)
			{
			case "regular":
				TaskbarIcon.Icon = ((StyleModeDefiner.DefineSystemStyle() == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.icontray_regular : ResourceFile.DarkModeDefault);
				break;
			case "connected":
				TaskbarIcon.Icon = ((StyleModeDefiner.DefineSystemStyle() == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.icontray_connected : ResourceFile.DarkModeSuccess);
				switch (vpnType)
				{
				case VpnType.NextAiVPN:
					try { TaskbarIcon.ShowCustomBalloon(new NotifyBalloonConnected(), PopupAnimation.Slide, 5000); } catch { }
					break;
				case VpnType.Streaming:
					try { TaskbarIcon.ShowCustomBalloon(new NotifyBalloonStreamingConnected(), PopupAnimation.Slide, 5000); } catch { }
					break;
				default:
					throw new ArgumentOutOfRangeException("vpnType", vpnType, null);
				}
				break;
			case "reconnecting":
				TaskbarIcon.Icon = ((StyleModeDefiner.DefineSystemStyle() == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.icontray_warning : ResourceFile.DarkModeWarning);
				try { TaskbarIcon.ShowCustomBalloon(new NotifyBalloonWarning(), PopupAnimation.Slide, 5000); } catch { }
				break;
			case "nonetwork":
				TaskbarIcon.Icon = ResourceFile.icontray_nonetwork;
				try { TaskbarIcon.ShowCustomBalloon(new NotifyBalloonNoNetwork(), PopupAnimation.Slide, 5000); } catch { }
				break;
			case "error":
				TaskbarIcon.Icon = ((StyleModeDefiner.DefineSystemStyle() == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.icontray_error : ResourceFile.DarkModeError);
				try { TaskbarIcon.ShowCustomBalloon(new NotifyBalloonError(), PopupAnimation.Slide, 5000); } catch { }
				break;
			}
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "SetNotifyIcon", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\TaskBarIconService.cs", 198);
		}
	}

	public void RemoveSystemTrayNotificationIcon()
	{
		_notificationsMenuItem.Icon = null;
		_sdkMonitor.VpnExpandedWindow.ExpandedSideMenu.NewNotificationsAlertImage.Visibility = Visibility.Collapsed;
	}

	public void ShowSystemTrayNotificationIcon()
	{
		_notificationsMenuItem.Icon = new Image
		{
			Source = GetNotificationIcon()
		};
		_sdkMonitor.VpnExpandedWindow.ExpandedSideMenu.NewNotificationsAlertImage.Visibility = Visibility.Visible;
	}

	public async void SignOutCommonFunction()
	{
		if (_signOutMessageBoxWindow.IsVisible)
		{
			return;
		}
		_signOutMessageBoxWindow.ShowDialog();
		if (!_signOutMessageBoxWindow.DialogResult)
		{
			SignOut = false;
			return;
		}
		_apiClient.LogOut(_sdkMonitor);
		if (_sdkMonitor.VpnExpandedWindow.Mainpanel.Connect.Content != null && _sdkMonitor.VpnExpandedWindow.Mainpanel.Connect.Content.Equals("Disconnect"))
		{
			await _sdkMonitor.DisconnectVPN();
			_sdkMonitor.Disconnected();
		}
		if (_sdkMonitor.VpnExpandedWindow != null)
		{
			_sdkMonitor.VpnExpandedWindow.Hide();
			_sdkMonitor.VpnEntities.MainWindow.Show();
			_sdkMonitor.VpnExpandedWindow.ExpandedLocations.SetVpnMode(VpnType.NextAiVPN);
		}
		_sdkMonitor.VpnEntities.MainWindow.SignIn.IsEnabled = true;
		ContextMenuHide("quit");
		SignOut = true;
		_analyticsService.SendNotification("Sign Out");
	}

	public async void QuitCommonFunction()
	{
		if (_sdkMonitor.VpnExpandedWindow != null)
		{
			_sdkMonitor.VpnExpandedWindow.AllowClose = true;
			if (_sdkMonitor.VpnExpandedWindow.Mainpanel?.Connect?.Content != null && _sdkMonitor.VpnExpandedWindow.Mainpanel.Connect.Content.Equals("Disconnect"))
			{
				await _sdkMonitor.DisconnectVPN();
			}
		}
		Application.Current?.Shutdown();
	}

	public void ContextMenuHide(params string[] headers)
	{
		foreach (object item2 in (IEnumerable)_contextMenu.Items)
		{
			MenuItem item = item2 as MenuItem;
			if (item != null)
			{
				if (headers.Any((string x) => x.ToLower().Equals(item.Header.ToString().ToLower())))
				{
					item.Visibility = Visibility.Visible;
					continue;
				}
				item.Visibility = Visibility.Collapsed;
			}
			if (item2 is Separator separator)
			{
				separator.Visibility = Visibility.Collapsed;
			}
		}
	}

	public void ContextMenuShow(params string[] headers)
	{
		List<string> list = headers.ToList();
		if (!_sdkMonitor.IsNextAiVpnModeAvailable)
		{
			list.AddRange(new List<string>
			{
				"Favorite",
				"Best available",
				string.Empty
			});
		}
		foreach (object item2 in (IEnumerable)_contextMenu.Items)
		{
			MenuItem item = item2 as MenuItem;
			if (item != null)
			{
				if (list.Any((string x) => x.ToLower().Equals(item.Header.ToString().ToLower())))
				{
					item.Visibility = Visibility.Collapsed;
					continue;
				}
				item.Visibility = Visibility.Visible;
			}
			if (item2 is Separator separator)
			{
				separator.Visibility = Visibility.Visible;
			}
		}
	}

	public async Task ShowNewNotificationWarningAsync()
	{
		HasNotifications = await _sdkMonitor.VpnExpandedWindow.NotificationCenter.NotificationService.HasNewNotificationAsync();
		_notificationsMenuItem.Icon = (HasNotifications ? new Image
		{
			Source = GetNotificationIcon()
		} : null);
	}

	private void ContextMenuSet()
	{
		_connectDisconnectOption.Header = "Connect";
		_quitOption.Header = "Quit";
		_connectingLocationMenuItem.Header = string.Empty;
		_connectingLocationMenuItem.IsEnabled = false;
		_connectingLocationMenuItem.Visibility = Visibility.Collapsed;
		MenuItem menuItem = new MenuItem
		{
			Header = "Help"
		};
		MenuItem menuItem2 = new MenuItem
		{
			Header = "Customer Support"
		};
		menuItem2.Click += InnerHelpCustomerSupportMenuItem_Click;
		MenuItem menuItem3 = new MenuItem
		{
			Header = "Terms of Service"
		};
		menuItem3.Click += InnerHelpTermsOfServiceMenuItem_Click;
		MenuItem menuItem4 = new MenuItem
		{
			Header = "Privacy Policy"
		};
		menuItem4.Click += InnerHelpPrivacyPolicyMenuItem_Click;
		MenuItem menuItem5 = new MenuItem
		{
			Header = "FAQ"
		};
		menuItem5.Click += InnerFAQMenuItem_Click;
		menuItem.Items.Add(menuItem5);
		menuItem.Items.Add(menuItem2);
		menuItem.Items.Add(menuItem3);
		menuItem.Items.Add(menuItem4);
		MenuItem menuItem6 = new MenuItem
		{
			Header = "Sign Out"
		};
		menuItem6.Click += SignOutMenuItem_Click;
		MenuItem menuItem7 = new MenuItem
		{
			Header = "Settings"
		};
		menuItem7.Click += SettingsOutMenuItem_Click;
		MenuItem menuItem8 = new MenuItem
		{
			Header = "Send Feedback"
		};
		menuItem8.Click += SendFeedbackMenuItem_Click;
		MenuItem menuItem9 = new MenuItem
		{
			Header = "Show NextAiVPN"
		};
		menuItem9.Click += SeeAllLocations_Click;
		_bestAvailableMenuItem.Header = "Best available";
		_bestAvailableMenuItem.Icon = new Image
		{
			Source = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.LightModeServerLocation.ToImageSource() : ResourceFile.DarkModeServerLocation.ToImageSource())
		};
		_bestAvailableMenuItem.Click += BestAvailableMenuItem_Click;
		_connectingToItem.Header = "Connect to:";
		_connectingToItem.IsEnabled = false;
		GenerateFavoriteTrayMenuItem();
		AddEmptyItemToFavoriteTaskbarContextMenu();
		GenerateStreamingTaskBarMenu();
		_connectDisconnectOption.Click += ConnectDisconnectOption_Click;
		_quitOption.Click += QuitOption_Click;
		_notificationsMenuItem.Header = "Notifications";
		_notificationsMenuItem.Click += NotificationsMenuItem_Click;
		_contextMenu.Items.Add(_connectingToItem);
		_contextMenu.Items.Add(_connectingLocationMenuItem);
		_contextMenu.Items.Add(_bestAvailableMenuItem);
		_contextMenu.Items.Add(_favoriteMenuItem);
		if (OSDetector.GetOSVersion() >= 10)
		{
			_contextMenu.Items.Add(_streamingMenuItem);
		}
		_contextMenu.Items.Add(new Separator());
		_contextMenu.Items.Add(menuItem9);
		_contextMenu.Items.Add(new Separator());
		_contextMenu.Items.Add(menuItem7);
		_contextMenu.Items.Add(_notificationsMenuItem);
		_contextMenu.Items.Add(menuItem8);
		_contextMenu.Items.Add(new Separator());
		_contextMenu.Items.Add(menuItem);
		_contextMenu.Items.Add(new Separator());
		_contextMenu.Items.Add(menuItem6);
		_contextMenu.Items.Add(_quitOption);
		TaskbarIcon.ContextMenu = _contextMenu;
		TaskbarIcon.TrayMouseDoubleClick += TaskbarIconTrayMouseDoubleClick;
		TaskbarIcon.TrayLeftMouseDown += TaskbarIconTrayLeftMouseDown;
		NextAiVPN.Services.Persistence.Style style = StyleModeDefiner.DefineAppStyle();
		_styleService.SetStyle(style);
		_styleService.SetContextMenuStyle(style, TaskbarIcon);
		_styleService.SetTaskbarIconStyle(StyleModeDefiner.DefineSystemStyle(), TaskbarIcon, _sdkMonitor.ConnectionStatus);
	}

	public void HideConnectingLocationMenuItem()
	{
		_connectingLocationMenuItem.Visibility = Visibility.Collapsed;
	}

	public void ShowStreaming()
	{
		_isStreamingVisible = true;
		_streamingMenuItem.Visibility = Visibility.Visible;
	}

	public void HideStreaming()
	{
		_isStreamingVisible = false;
		_streamingMenuItem.Visibility = Visibility.Collapsed;
	}

	private void GenerateStreamingTaskBarMenu()
	{
		_streamingMenuItem.Header = "For Streaming";
		Image icon = new Image
		{
			Source = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.start_lightmode.ToImageSource() : ResourceFile.start_darkmode.ToImageSource())
		};
		_streamingMenuItem.Icon = icon;
		_streamingMenuItem.MouseEnter += StreamingMenuItem_MouseEnter;
	}

	private void StreamingMenuItem_MouseEnter(object sender, MouseEventArgs e)
	{
		lock (_lockObject)
		{
			ClearSubItemsStreamingTaskBarContextMenu();
			foreach (StreamingLocation location in _sdkMonitor.StreamingSdk.Locations)
			{
				MenuItem menuItem = new MenuItem
				{
					Header = location.FullLocationName,
					Icon = new Image
					{
						Source = _countryFlagService.GetCountryFlagImageSource(location.CountryCode),
						Stretch = Stretch.Uniform
					}
				};
				menuItem.Click += StreamingSubMenuItem_Click;
				_streamingMenuItem.Items.Add(menuItem);
			}
		}
	}

	private async void StreamingSubMenuItem_Click(object sender, RoutedEventArgs e)
	{
		MenuItem item = sender as MenuItem;
		if (item == null)
		{
			return;
		}
		StreamingLocation streamingLocation = _sdkMonitor.StreamingSdk.Locations.FirstOrDefault((StreamingLocation x) => x.FullLocationName.Equals(item.Header));
		if (streamingLocation == null)
		{
			_logger?.Error("Method: StreamingSubMenuItem_Click. Message: Can't find streaming location", "StreamingSubMenuItem_Click", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\TaskBarIconService.cs", 564);
			return;
		}
		_sdkMonitor.StreamingLocation = streamingLocation;
		if (_sdkMonitor.ConnectedVpnMode != VpnType.Streaming)
		{
			_sdkMonitor.ConnectedVpnMode = VpnType.Streaming;
			_sdkMonitor.VpnExpandedWindow.ExpandedLocations.SetVpnMode(VpnType.Streaming, skipValidation: true);
		}
		await _sdkMonitor.ConnectToVPN(VpnType.Streaming);
	}

	private void ClearSubItemsStreamingTaskBarContextMenu()
	{
		int count = _streamingMenuItem.Items.Count;
		for (int i = 0; i < count; i++)
		{
			_streamingMenuItem.Items.RemoveAt(0);
		}
	}

	private ImageSource GetNotificationIcon()
	{
		if (StyleModeDefiner.DefineAppStyle() != NextAiVPN.Services.Persistence.Style.Light)
		{
			return ResourceFile.NotificationsOrangeDarkMode.ToImageSource();
		}
		return ResourceFile.NotificationsOrangeLightMode.ToImageSource();
	}

	private async void NotificationsMenuItem_Click(object sender, RoutedEventArgs e)
	{
		RemoveSystemTrayNotificationIcon();
		_sdkMonitor.VpnExpandedWindow.Visibility = Visibility.Visible;
		_sdkMonitor.VpnExpandedWindow.ExpandedSideMenu.SetMenuOption(SideMenuOption.Notifications);
		await _sdkMonitor.VpnExpandedWindow.NotificationCenter.LoadNotifications();
		_sdkMonitor.VpnExpandedWindow.WindowState = WindowState.Normal;
	}

	private void InnerFAQMenuItem_Click(object sender, RoutedEventArgs e)
	{
		_sdkMonitor.BrowserLinksOpener.OpenBrowserLink(4);
	}

	private void TaskbarIconTrayLeftMouseDown(object sender, RoutedEventArgs e)
	{
		DisplayWindowsOnSystemTrayIconClick();
	}

	public void DisplayWindowsOnSystemTrayIconClick()
	{
		try
		{
			if (_sdkMonitor?.VpnExpandedWindow != null)
			{
				ShowWindow(_sdkMonitor.VpnExpandedWindow);
				_sdkMonitor.VpnExpandedWindow.ExpandedSideMenu?.SetMenuOption(SideMenuOption.Location);
				return;
			}
			if (!InternetConnection.IsAvailable() && _sdkMonitor?.VpnEntities?.ExpandedWindow != null)
			{
				ShowWindow(_sdkMonitor.VpnEntities.ExpandedWindow);
			}
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "DisplayWindowsOnSystemTrayIconClick", "TaskBarIconService.cs", 722);
		}
	}

	private void ShowWindow(Window window)
	{
		if (window == null) return;
		window.Show();
		window.Visibility = Visibility.Visible;
		window.WindowState = WindowState.Normal;
		window.Topmost = true;
		window.Activate();
		window.Focus();
		window.Topmost = false;
	}

	private void GenerateFavoriteTrayMenuItem()
	{
		_favoriteMenuItem.Header = "Favorite";
		_favoriteMenuItem.Icon = new Image
		{
			Source = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.Favorite.ToImageSource() : ResourceFile.DarkModeFavorite.ToImageSource())
		};
		_favoriteMenuItem.MouseEnter += FavoriteMenuItem_MouseEnter;
	}

	private void FavoriteMenuItem_MouseEnter(object sender, MouseEventArgs e)
	{
		ClearSubItemsFavoriteTaskbarContextMenu();
		lock (_lockObject)
		{
			_favoriteMenuItems = new List<SystemTrayFavoriteMenuItem>();
			IOrderedEnumerable<string> orderedEnumerable = from x in _favoriteService.GetFavoriteListFromConfig()
				orderby x
				select x;
			if (orderedEnumerable.Any())
			{
				foreach (string item2 in orderedEnumerable)
				{
					string favorite = item2;
					if (item2.Length > 3)
					{
						favorite = item2.Substring(3, 3);
					}
					ILocation location = _sdkMonitor.NextAiVpnSdkManager.Locations.FirstOrDefault((ILocation x) => x.Id == favorite);
					if (location != null)
					{
						SystemTrayFavoriteMenuItem item = GenerateMenuFavoriteMenuItem(location, isCountry: false);
						_favoriteMenuItems.Add(item);
					}
				}
				_favoriteMenuItems = (from x in _favoriteMenuItems
					orderby x.Country, x.City
					select x).ToList();
				{
					foreach (IGrouping<string, SystemTrayFavoriteMenuItem> item3 in (from x in _favoriteMenuItems
						group x by x.Country).ToList())
					{
						foreach (SystemTrayFavoriteMenuItem orderedLocationsWithoutCountry in GetOrderedLocationsWithoutCountries(item3))
						{
							_favoriteMenuItem.Items.Add(orderedLocationsWithoutCountry.MenuItem);
						}
					}
					return;
				}
			}
			MenuItem menuItem = new MenuItem
			{
				Header = "No favorites added",
				Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush(IsDarkmode() ? "#DCDCDF" : "#94959E")
			};
			SetSubFavoriteMenuItemStyle(menuItem);
			_favoriteMenuItem.Items.Add(menuItem);
		}
	}

	private static IEnumerable<SystemTrayFavoriteMenuItem> GetOrderedLocations(IEnumerable<SystemTrayFavoriteMenuItem> locations)
	{
		SystemTrayFavoriteMenuItem systemTrayFavoriteMenuItem = null;
		IOrderedEnumerable<SystemTrayFavoriteMenuItem> orderedEnumerable = locations.OrderBy((SystemTrayFavoriteMenuItem x) => x.City);
		List<SystemTrayFavoriteMenuItem> list = new List<SystemTrayFavoriteMenuItem>();
		foreach (SystemTrayFavoriteMenuItem item in orderedEnumerable)
		{
			if (!item.Header.Contains("-"))
			{
				systemTrayFavoriteMenuItem = item;
			}
			else
			{
				list.Add(item);
			}
		}
		if (systemTrayFavoriteMenuItem != null)
		{
			list.Insert(0, systemTrayFavoriteMenuItem);
		}
		return list;
	}

	private static IEnumerable<SystemTrayFavoriteMenuItem> GetOrderedLocationsWithoutCountries(IEnumerable<SystemTrayFavoriteMenuItem> locations)
	{
		return from x in locations
			orderby x.City
			group x by x.ConnectionString into x
			select x.First();
	}

	private void AddItemToFavoriteTaskbarContextMenuAddFavourites(bool isSeparatoeNeed)
	{
		Image image = new Image
		{
			Stretch = Stretch.Uniform,
			Source = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.addSystemTray.ToImageSource() : ResourceFile.DarkModeAdd.ToImageSource())
		};
		RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
		MenuItem menuItem = new MenuItem
		{
			Header = "Add new favorite",
			Icon = image
		};
		menuItem.Click += AddNewFavorite_Click;
		if (isSeparatoeNeed)
		{
			_favoriteMenuItem.Items.Add(new Separator());
		}
		_favoriteMenuItem.Items.Add(menuItem);
	}

	private void AddNewFavorite_Click(object sender, RoutedEventArgs e)
	{
		_sdkMonitor.SetVpnMode(VpnType.NextAiVPN);
		DisplayLocation();
	}

	private SystemTrayFavoriteMenuItem GenerateMenuFavoriteMenuItem(ILocation favoriteLocation, bool isCountry)
	{
		if (isCountry)
		{
			favoriteLocation = _sdkMonitor.LocationHelper.GetBestPingCountryLocation(favoriteLocation.CountryCode);
		}
		string header = favoriteLocation.CountryCode + " - " + favoriteLocation.City;
		Image image = new Image
		{
			Source = _countryFlagService.GetCountryFlagImageSource(favoriteLocation.CountryCode),
			Stretch = Stretch.Uniform
		};
		RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
		MenuItem menuItem = new MenuItem
		{
			Header = header,
			Icon = image
		};
		SetSubFavoriteMenuItemStyle(menuItem);
		menuItem.Click += SubFavoriteMenuItem_Click;
		return new SystemTrayFavoriteMenuItem
		{
			Header = header,
			ConnectionString = favoriteLocation.CountryCode + " - " + favoriteLocation.City,
			MenuItem = menuItem,
			Country = favoriteLocation.Country,
			City = favoriteLocation.City
		};
	}

	private static void SetSubFavoriteMenuItemStyle(MenuItem subFavoriteMenuItem)
	{
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark && Application.Current?.TryFindResource("MenuItemControlTemplate") is ControlTemplate template)
		{
			subFavoriteMenuItem.Template = template;
		}
	}

	private async void SubFavoriteMenuItem_Click(object sender, RoutedEventArgs e)
	{
		MenuItem menuItem = sender as MenuItem;
		string text = _favoriteMenuItems.Find((SystemTrayFavoriteMenuItem x) => x.MenuItem == menuItem)?.ConnectionString;
		if (!string.IsNullOrEmpty(text))
		{
			string[] source = text.Split('-');
			string countryCode = source.First().Replace(" ", "");
			string city = text.Replace(countryCode + " - ", "");
			ILocation location = _sdkMonitor.NextAiVpnSdkManager.Locations.FirstOrDefault((ILocation x) => x.CountryCode.ToLower().Equals(countryCode.ToLower()) && x.City.ToLower().Equals(city.ToLower()));
			await _sdkMonitor.DisconnectVPN();
			await ConnectVpn(location);
			_apiClient.SetIsBestAvailable("0");
		}
	}

	private void FavoriteMenuItem_MouseLeave(object sender, MouseEventArgs e)
	{
		ClearSubItemsFavoriteTaskbarContextMenu();
	}

	private void ClearSubItemsFavoriteTaskbarContextMenu()
	{
		int count = _favoriteMenuItem.Items.Count;
		for (int i = 0; i < count; i++)
		{
			_favoriteMenuItem.Items.RemoveAt(0);
		}
	}

	private void AddEmptyItemToFavoriteTaskbarContextMenu()
	{
		MenuItem newItem = new MenuItem
		{
			Header = string.Empty,
			Width = 5.0
		};
		_favoriteMenuItem.Items.Add(newItem);
	}

	private async void BestAvailableMenuItem_Click(object sender, RoutedEventArgs e)
	{
		if (_bestAvailableMenuItem.Header.ToString().ToLower() != "disconnect")
		{
			_sdkMonitor.ConnectedVpnMode = VpnType.NextAiVPN;
			await ConnectToBestAvailable();
			_bestAvailableMenuItem.Header = "Disconnect";
		}
		else
		{
			await DisconnectVpn();
			_bestAvailableMenuItem.Header = "Best available";
		}
	}

	private void SdkObject_ConnectionNotifier(string message)
	{
		TaskbarIcon.Dispatcher.Invoke(delegate
		{
			switch (message.ToLower())
			{
			case "connected":
				_streamingMenuItem.Visibility = Visibility.Collapsed;
				_connectingToItem.Header = "Connected to:";
				_bestAvailableMenuItem.Header = "Disconnect";
				_bestAvailableMenuItem.Visibility = Visibility.Visible;
				_bestAvailableMenuItem.Icon = new Image
				{
					Source = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.Favorite.ToImageSource() : ResourceFile.DarkModeFavorite.ToImageSource())
				};
				break;
			case "disconnected":
				_streamingMenuItem.Visibility = ((!_isStreamingVisible) ? Visibility.Collapsed : Visibility.Visible);
				_connectingToItem.Header = "Connect to:";
				if (_sdkMonitor.IsNextAiVpnModeAvailable)
				{
					_bestAvailableMenuItem.Header = "Best available";
					_bestAvailableMenuItem.Visibility = Visibility.Visible;
					_bestAvailableMenuItem.Icon = new Image
					{
						Source = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? ResourceFile.LightModeServerLocation.ToImageSource() : ResourceFile.DarkModeServerLocation.ToImageSource())
					};
					_favoriteMenuItem.Visibility = Visibility.Visible;
				}
				else
				{
					_favoriteMenuItem.Visibility = Visibility.Collapsed;
					_bestAvailableMenuItem.Visibility = Visibility.Collapsed;
				}
				break;
			case "connecting":
				_bestAvailableMenuItem.Visibility = Visibility.Collapsed;
				_connectingToItem.Header = "Connecting to:";
				_favoriteMenuItem.Visibility = Visibility.Collapsed;
				break;
			}
			SetUpConnectingLocationTrayMenuHeader(message);
		});
	}

	private void SetUpConnectingLocationTrayMenuHeader(string connectionStatus)
	{
		switch (_sdkMonitor.ConnectedVpnMode)
		{
		case VpnType.NextAiVPN:
			SetConnectingLocationNextAiVpn(connectionStatus);
			break;
		case VpnType.Streaming:
			SetConnectingLocationStreaming(connectionStatus);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void SetConnectingLocationStreaming(string connectionStatus)
	{
		switch (connectionStatus.ToLower())
		{
		case "connected":
		{
			string countryCode = _sdkMonitor.StreamingLocation.CountryCode;
			_connectingLocationMenuItem.Header = _sdkMonitor.StreamingLocation.FullLocationName;
			_connectingLocationMenuItem.Icon = new Image
			{
				Source = _countryFlagService.GetCountryFlagImageSource(countryCode),
				Opacity = 0.4
			};
			_connectingLocationMenuItem.Visibility = Visibility.Visible;
			break;
		}
		case "connecting":
			_connectingLocationMenuItem.Icon = new Image
			{
				Source = (IsDarkmode() ? ResourceFile.DarkModeDefault.ToImageSource() : ResourceFile.icontray_regular.ToImageSource())
			};
			_connectingLocationMenuItem.Visibility = Visibility.Visible;
			if (_sdkMonitor.NextAiVpnLocation != null && _sdkMonitor.NextAiVpnLocation.Id.Equals("bestavailable"))
			{
				_connectingLocationMenuItem.Header = "Best available";
			}
			break;
		case "disconnected":
			_connectingLocationMenuItem.Visibility = Visibility.Collapsed;
			break;
		}
	}

	private void SetConnectingLocationNextAiVpn(string connectionStatus)
	{
		string text;
		if (_sdkMonitor.NextAiVpnSdkManager.ActiveConnectionInformation == null)
		{
			text = _sdkMonitor.NextAiVpnLocation.CountryCode + " - " + _sdkMonitor.NextAiVpnLocation.City;
		}
		else
		{
			string countryCode = _sdkMonitor.NextAiVpnSdkManager.ActiveConnectionInformation.Location.CountryCode;
			string city = _sdkMonitor.NextAiVpnSdkManager.ActiveConnectionInformation.Location.City;
			text = countryCode + " - " + city;
		}
		_connectingLocationMenuItem.Header = text;
		switch (connectionStatus.ToLower())
		{
		case "connected":
		{
			string countryId = (text.ToLower().Equals("best available") ? string.Empty : _sdkMonitor.NextAiVpnSdkManager.ActiveConnectionInformation.Location.CountryCode);
			_connectingLocationMenuItem.Icon = new Image
			{
				Source = _countryFlagService.GetCountryFlagImageSource(countryId),
				Opacity = 0.4
			};
			_connectingLocationMenuItem.Visibility = Visibility.Visible;
			break;
		}
		case "connecting":
			_connectingLocationMenuItem.Icon = new Image
			{
				Source = (IsDarkmode() ? ResourceFile.DarkModeDefault.ToImageSource() : ResourceFile.icontray_regular.ToImageSource())
			};
			_connectingLocationMenuItem.Visibility = Visibility.Visible;
			if (_sdkMonitor.NextAiVpnLocation.Id.Equals("bestavailable"))
			{
				_connectingLocationMenuItem.Header = "Best available";
			}
			break;
		case "disconnected":
			_connectingLocationMenuItem.Visibility = Visibility.Collapsed;
			break;
		}
	}

	private bool IsDarkmode()
	{
		return StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark;
	}

	private async Task ConnectToBestAvailable()
	{
		ILocation bestAvailableLocation = _sdkMonitor.NextAiVpnSdkManager.Locations.FirstOrDefault((ILocation x) => x.Id.ToLower() == "bestavailable");
		await _sdkMonitor.DisconnectVPN();
		await ConnectVpn(bestAvailableLocation);
		_apiClient.SetIsBestAvailable("1");
	}

	private void SeeAllLocations_Click(object sender, RoutedEventArgs e)
	{
		DisplayLocation();
	}

	private void DisplayLocation()
	{
		_sdkMonitor.VpnExpandedWindow.Visibility = Visibility.Visible;
		_sdkMonitor.VpnExpandedWindow.ExpandedSideMenu.SetMenuOption(SideMenuOption.Location);
		_sdkMonitor.VpnExpandedWindow.WindowState = WindowState.Normal;
	}

	private void SettingsOutMenuItem_Click(object sender, RoutedEventArgs e)
	{
		_sdkMonitor.VpnExpandedWindow.Visibility = Visibility.Visible;
		_sdkMonitor.VpnExpandedWindow.ExpandedSideMenu.SetMenuOption(SideMenuOption.Settings);
		_sdkMonitor.VpnExpandedWindow.WindowState = WindowState.Normal;
	}

	private void SendFeedbackMenuItem_Click(object sender, RoutedEventArgs e)
	{
		_sdkMonitor.VpnExpandedWindow.ShowFeedbackWindow();
	}

	private void InnerHelpPrivacyPolicyMenuItem_Click(object sender, RoutedEventArgs e)
	{
		_sdkMonitor.BrowserLinksOpener.OpenBrowserLink(2);
	}

	private void InnerHelpTermsOfServiceMenuItem_Click(object sender, RoutedEventArgs e)
	{
		_sdkMonitor.BrowserLinksOpener.OpenBrowserLink(1);
	}

	private void InnerHelpCustomerSupportMenuItem_Click(object sender, RoutedEventArgs e)
	{
		_sdkMonitor.BrowserLinksOpener.OpenBrowserLink(3);
	}

	private void SignOutMenuItem_Click(object sender, RoutedEventArgs e)
	{
		_analyticsService.SendNotification("TrayMenu - Sign Out");
		HideSubscriptionExpiredWindow();
		SignOutCommonFunction();
	}

	private void HideSubscriptionExpiredWindow()
	{
		if (_sdkMonitor.VpnEntities.MainWindow.SubscriptionExpiredWindow != null && _sdkMonitor.VpnEntities.MainWindow.SubscriptionExpiredWindow.IsVisible)
		{
			_sdkMonitor.VpnEntities.MainWindow.SubscriptionExpiredWindow.Hide();
			_sdkMonitor.VpnEntities.MainWindow.SignIn.IsEnabled = true;
		}
	}

	private async void QuitOption_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			_analyticsService.SendNotification("TrayMenu - Quit");
			QuitCommonFunction();
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify("[ContextMenu Quit] - " + ex.Message);
		}
	}

	private async void ConnectDisconnectOption_Click(object sender, RoutedEventArgs e)
	{
		_ = 1;
		try
		{
			if (_connectDisconnectOption.Header.Equals("Connect"))
			{
				await ConnectVpn(_sdkMonitor.NextAiVpnLocation);
			}
			else if (_connectDisconnectOption.Header.Equals("Disconnect"))
			{
				await DisconnectVpn();
			}
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify("[ContextMenu Connect-Disconnect] - " + ex.Message);
			MessageBox.Show(ex.Message, SystemInfo.AppName, MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private async Task ConnectVpn(ILocation location)
	{
		_analyticsService.SendConnectNotification("TrayMenu - Connect");
		if (_sdkMonitor.ConnectedVpnMode != VpnType.NextAiVPN)
		{
			_sdkMonitor.ConnectedVpnMode = VpnType.NextAiVPN;
			_sdkMonitor.VpnExpandedWindow.ExpandedLocations.SetVpnMode(VpnType.NextAiVPN);
		}
		_sdkMonitor.SetLocation(location);
		await _sdkMonitor.ConnectToVPN(location);
	}

	private async Task DisconnectVpn()
	{
		_analyticsService.SendNotification("TrayMenu - Disconnect");
		await _sdkMonitor.DisconnectVPN();
		_apiClient.SaveConnectedTo("");
	}

	private void TaskbarIconTrayMouseDoubleClick(object sender, RoutedEventArgs e)
	{
		DisplayWindowsOnSystemTrayIconClick();
	}

	private void StyleChanged(NextAiVPN.Services.Persistence.Style arg1, NextAiVPN.Services.Persistence.Style arg2)
	{
		_styleService.SetTaskbarIconStyle(StyleModeDefiner.DefineSystemStyle(), TaskbarIcon, _sdkMonitor.ConnectionStatus);
	}
}
