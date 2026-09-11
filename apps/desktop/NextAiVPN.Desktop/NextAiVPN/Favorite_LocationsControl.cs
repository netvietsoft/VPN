using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using DynamicData.Binding;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN;

public partial class Favorite_LocationsControl : UserControl, IComponentConnector
{
	private SDKMonitor _sdkObject;

	private Image _target;

	private ImageSource _oldSource;

	private Storyboard _storyboard;

	private VPNWindowExpanded _vpnWindowExpanded;

	private ILocationHelper _locationHelper;

	private readonly List<string> _favoritesList = new List<string>();

	private readonly ObservableCollectionExtended<ILocation> _favoriteLocations = new ObservableCollectionExtended<ILocation>();

	private readonly ObservableCollectionExtended<ILocation> _bestAvailableLocation = new ObservableCollectionExtended<ILocation>();

	public bool IsFavorites;

	public bool IsBestAvailable;

	public Favorite_LocationsControl()
	{
		InitializeComponent();
		base.Loaded += OnStyleLoaded;
		base.Unloaded += OnStyleUnloaded;
		FavoritesList.ItemsSource = _favoriteLocations;
		BestAvailable.ItemsSource = _bestAvailableLocation;
	}

	public void RetrieveFavoritesFromConfig()
	{
		_favoritesList.Clear();
		string value = Utils.AppSettingsHelper.GetValue("FavoritesList");
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Substring(0, value.LastIndexOf(';'));
			List<string> list = value.Split(';').Reverse().Take(5)
				.Reverse()
				.ToList();
			ShowAddMoreSectionVisibility(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				if (i < 5 && list[i] != "")
				{
					_favoritesList.Add(list[i]);
				}
			}
		}
		else
		{
			AddMoreSection.Visibility = Visibility.Visible;
		}
	}

	public void GetSdkObject(SDKMonitor sdk)
	{
		_sdkObject = sdk;
		_locationHelper = _sdkObject.LocationHelper;
	}

	private void MainWindowStyleChanged(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style sysStyle)
	{
		StyleChange();
	}

	private void OnStyleLoaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged += MainWindowStyleChanged;
	}

	private void OnStyleUnloaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged -= MainWindowStyleChanged;
	}

	public void ShowSections()
	{
		AddMoreSection.Visibility = Visibility.Visible;
		secondSeparator.Visibility = Visibility.Visible;
	}

	public void GetVpnMainWindowExpanded(VPNWindowExpanded mainExpandedWindow)
	{
		_vpnWindowExpanded = mainExpandedWindow;
		AddMoreSection.Visibility = Visibility.Collapsed;
		secondSeparator.Visibility = Visibility.Collapsed;
	}

	public void LoadOnlyBestAvailable()
	{
		LocationsStackPanel.Visibility = Visibility.Collapsed;
		_favoriteLocations.Clear();
		_bestAvailableLocation.Clear();
		FavoritesList.SelectedIndex = -1;
		BestAvailable.SelectedIndex = -1;
		foreach (ILocation item in _sdkObject.NextAiVpnSdkManager.Locations.Where((ILocation x) => x.Id == "bestavailable"))
		{
			_bestAvailableLocation.Add(item);
		}
	}

	public void LoadFavoritesList()
	{
		RetrieveFavoritesFromConfig();
		_favoriteLocations.Clear();
		_bestAvailableLocation.Clear();
		FavoritesList.SelectedIndex = -1;
		BestAvailable.SelectedIndex = -1;
		IsFavoritesListEnabled(!_sdkObject.NextAiVpnSdkManager.IsConnecting);
		foreach (ILocation item in _sdkObject.NextAiVpnSdkManager.Locations.Where((ILocation x) => x.Id == "bestavailable"))
		{
			_bestAvailableLocation.Add(item);
		}
		List<BestLocation> list = new List<BestLocation>();
		foreach (string favorites in _favoritesList)
		{
			bool isCountry = false;
			string favorite = favorites;
			if (favorites.Length > 3)
			{
				isCountry = true;
				favorite = favorites.Substring(3, 3);
			}
			foreach (ILocation item2 in _sdkObject.NextAiVpnSdkManager.Locations.Where((ILocation x) => x.Id == favorite))
			{
				list.Add(GenerateFavoriteLocation(item2, isCountry));
			}
			isCountry = false;
		}
		List<IGrouping<string, BestLocation>> list2 = (from x in list
			group x by x.Country).ToList();
		List<ILocation> list3 = new List<ILocation>();
		foreach (IGrouping<string, BestLocation> item3 in list2)
		{
			foreach (BestLocation orderedLocationsWithoutCountry in GetOrderedLocationsWithoutCountries(item3))
			{
				list3.Add(orderedLocationsWithoutCountry.Location);
			}
		}
		ShowAddMoreSectionVisibility(list3.Count);
		_favoriteLocations.AddRange((from x in list3.Distinct()
			orderby x.CountryCode, x.City
			select x).ToList());
	}

	private void ShowAddMoreSectionVisibility(int count)
	{
		if (count < 5)
		{
			AddMoreSection.Visibility = Visibility.Visible;
			AddMore.Text = "Add new favorite";
		}
		else
		{
			AddMoreSection.Visibility = Visibility.Collapsed;
		}
	}

	private BestLocation GenerateFavoriteLocation(ILocation favoriteLocation, bool isCountry)
	{
		if (isCountry)
		{
			favoriteLocation = _locationHelper.GetBestPingCountryLocation(favoriteLocation.CountryCode);
		}
		return new BestLocation
		{
			ConnectionString = favoriteLocation.CountryCode + " - " + favoriteLocation.City,
			City = favoriteLocation.City,
			Country = favoriteLocation.Country,
			Location = favoriteLocation
		};
	}

	private static IEnumerable<BestLocation> GetOrderedLocationsWithoutCountries(IEnumerable<BestLocation> locations)
	{
		return from x in locations
			orderby x.City
			group x by x.City into x
			select x.First();
	}

	public List<string> CheckDuplicates(string[] favs)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < favs.Length; i++)
		{
			string fav = favs[i];
			if (fav.Length > 3)
			{
				fav = fav.Substring(3, 3);
			}
			foreach (ILocation item in _sdkObject.NextAiVpnSdkManager.Locations.Where((ILocation x) => x.Id == fav))
			{
				bool flag = false;
				foreach (string item2 in list)
				{
					if (item2 == item.CountryCode)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(item.CountryCode.ToString());
					list2.Add(item.Id.ToString());
				}
			}
		}
		return list2;
	}

	private async void FavoritesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		IsBestAvailable = false;
		IsFavorites = true;
		if (Utils.AppSettingsHelper.GetValue("IUnderstand").Equals("1"))
		{
			if (FavoritesList.SelectedIndex != -1)
			{
				await _sdkObject.DisconnectVPN();
				_sdkObject.SetLocation((ILocation)FavoritesList.SelectedItem);
				_sdkObject.ConnectFromFavorites = true;
				_sdkObject.ConnectedVpnMode = VpnType.NextAiVPN;
				_sdkObject.ConnectToVPN(_sdkObject.NextAiVpnLocation);
			}
		}
		else if (FavoritesList.SelectedIndex > -1)
		{
			ShowBeforeConnect();
		}
	}

	private async void BestAvailable_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		IsFavorites = false;
		IsBestAvailable = true;
		if (Utils.AppSettingsHelper.GetValue("IUnderstand").Equals("1"))
		{
			if (BestAvailable.SelectedIndex != -1)
			{
				await _sdkObject.DisconnectVPN();
				_sdkObject.SetLocation((ILocation)BestAvailable.SelectedItem);
				_sdkObject.ConnectedVpnMode = VpnType.NextAiVPN;
				_sdkObject.ConnectToVPN(_sdkObject.NextAiVpnLocation);
			}
		}
		else if (BestAvailable.SelectedIndex > -1)
		{
			ShowBeforeConnect();
		}
	}

	public async Task ConnectAnimationAsync(bool connected)
	{
		try
		{
			if (!connected)
			{
				ListBoxItem obj = null;
				if (IsBestAvailable)
				{
					obj = (ListBoxItem)BestAvailable.ItemContainerGenerator.ContainerFromIndex(BestAvailable.SelectedIndex);
				}
				else if (IsFavorites)
				{
					obj = (ListBoxItem)FavoritesList.ItemContainerGenerator.ContainerFromIndex(FavoritesList.SelectedIndex);
				}
				ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(obj);
				DataTemplate contentTemplate = contentPresenter.ContentTemplate;
				_target = (Image)contentTemplate.FindName("flagImage", contentPresenter);
				_oldSource = _target.Source;
				_target.Source = new BitmapImage(new Uri("/Assets/spinnerblue.png", UriKind.Relative));
				try
				{
					_storyboard = (Storyboard)_target.FindResource("spin");
					_storyboard.Begin();
					_storyboard.SetSpeedRatio(75.0);
				}
				catch (Exception exception)
				{
					Utils.Logger?.Error(exception, "ConnectAnimationAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Favorite_LocationsControl.xaml.cs", 378);
				}
			}
			else if (_storyboard != null)
			{
				_storyboard.Stop();
				_target.Source = new BitmapImage(new Uri("/Assets/check.png", UriKind.Relative));
				await Task.Delay(1500);
				_target.Source = _oldSource;
			}
			IsFavoritesListEnabled(!_sdkObject.NextAiVpnSdkManager.IsConnecting);
		}
		catch (Exception exception2)
		{
			Utils.Logger?.Error(exception2, "ConnectAnimationAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Favorite_LocationsControl.xaml.cs", 396);
		}
	}

	public void StyleChange()
	{
		LoadFavoritesList();
	}

	public async Task ErrorConnectedAsync()
	{
		try
		{
			if (_storyboard != null)
			{
				_storyboard.Stop();
			}
			if (_target != null)
			{
				_target.Source = new BitmapImage(new Uri("/Assets/error.png", UriKind.Relative));
				await Task.Delay(1500);
				_target.Source = _oldSource;
			}
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "ErrorConnectedAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Favorite_LocationsControl.xaml.cs", 440);
		}
	}

	public void IsFavoritesListEnabled(bool isEnabled)
	{
		FavoritesList.IsEnabled = isEnabled;
		BestAvailable.IsEnabled = isEnabled;
	}

	private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
	{
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(obj, i);
			if (child != null && child is childItem)
			{
				return (childItem)child;
			}
			childItem val = FindVisualChild<childItem>(child);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	private void ShowBeforeConnect()
	{
		_vpnWindowExpanded.Hide();
		_vpnWindowExpanded.BeforeConnectWindowWrapper.ShowDialog();
		_vpnWindowExpanded.Show();
	}

	private void AddMoreSection_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		_vpnWindowExpanded.ExpandedSideMenu.SetMenuOption(SideMenuOption.Location);
	}

	private async void BestAvailable_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		IsFavorites = false;
		IsBestAvailable = true;
		if (Utils.AppSettingsHelper.GetValue("IUnderstand").Equals("1"))
		{
			try
			{
				ILocation bestAvailableLocation = _sdkObject.NextAiVpnSdkManager.Locations.FirstOrDefault((ILocation x) => x.Id.ToLower() == "bestavailable");
				await InitiateConnection(bestAvailableLocation);
			}
			catch (Exception ex)
			{
				Utils.Logger.Error("Can't connect to the best available.\n" + ex.Message, "BestAvailable_OnMouseDown", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Favorite_LocationsControl.xaml.cs", 494);
				Utils.Logger.Information("Connecting to the last connected location.", "BestAvailable_OnMouseDown", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Favorite_LocationsControl.xaml.cs", 495);
				await InitiateConnection(_sdkObject.NextAiVpnLocation);
			}
		}
		else
		{
			ShowBeforeConnect();
		}
	}

	private async Task InitiateConnection(ILocation bestAvailableLocation)
	{
		await _sdkObject.DisconnectVPN();
		_sdkObject.SetLocation(bestAvailableLocation);
		_sdkObject.ConnectedVpnMode = VpnType.NextAiVPN;
		_sdkObject.ConnectToVPN(bestAvailableLocation);
	}
}
