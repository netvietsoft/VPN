using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows.Threading;
using NextAiVPN.Enums;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN.UI.Favotite;

public partial class FavoriteLocationsTab : UserControl, IComponentConnector
{
	private DoubleAnimation _da;

	private RotateTransform _rt;

	public IFavoriteLocationsServices FavoriteLocationsServices;

	private ObservableCollection<ILocation> _favoriteLocationList = new ObservableCollection<ILocation>();

	private bool _isSearchingLocation;

	private const string _noItemsAdded = "No favorite added";

	private const string _noItemsFound = "No servers found matching that name";

	public SDKMonitor SdkMonitor { get; private set; }

	public string NoServersFoundMessage
	{
		get
		{
			if (FavoriteLocationCount <= 0)
			{
				return "No favorite added";
			}
			return "No servers found matching that name";
		}
	}

	public int FavoriteLocationCount => _favoriteLocationList.Count;

	public FavoriteLocationsTab()
	{
		base.DataContext = this;
		InitializeComponent();
		LocationsListHeader.SetAsFavoriteHeader();
		LocationsListHeader locationsListHeader = LocationsListHeader;
		locationsListHeader.SearchTextChangedFunc = (Action<string>)Delegate.Combine(locationsListHeader.SearchTextChangedFunc, new Action<string>(SearchTextChangedFunc));
		LocationsListHeader locationsListHeader2 = LocationsListHeader;
		locationsListHeader2.CountrySortFunc = (Action<LocationsOrder, bool>)Delegate.Combine(locationsListHeader2.CountrySortFunc, new Action<LocationsOrder, bool>(CountrySortFunc));
		LocationsListHeader locationsListHeader3 = LocationsListHeader;
		locationsListHeader3.PingSortFunc = (Action<LocationsOrder, bool>)Delegate.Combine(locationsListHeader3.PingSortFunc, new Action<LocationsOrder, bool>(PingSortFunc));
	}

	private void PingSortFunc(LocationsOrder arg1, bool arg2)
	{
		if (arg2)
		{
			LocationsList.ItemsSource = _favoriteLocationList.OrderBy((ILocation x) => x.PingMs).Distinct();
		}
		else
		{
			LocationsList.ItemsSource = _favoriteLocationList.OrderByDescending((ILocation x) => x.PingMs).Distinct();
		}
	}

	private void CountrySortFunc(LocationsOrder arg1, bool arg2)
	{
		if (arg2)
		{
			LocationsList.ItemsSource = (from x in _favoriteLocationList
				orderby x.Country, x.City
				select x).Distinct();
		}
		else
		{
			LocationsList.ItemsSource = (from x in _favoriteLocationList
				orderby x.Country descending, x.City
				select x).Distinct();
		}
	}

	private void SearchTextChangedFunc(string obj)
	{
		if (string.IsNullOrEmpty(obj))
		{
			_isSearchingLocation = false;
			LocationsList.ItemsSource = _favoriteLocationList.OrderBy((ILocation x) => x.Country).Distinct();
			if (SdkMonitor.NextAiVpnSdkManager.IsConnected)
			{
				SetCountryFlagImageStatusAfterSearching();
			}
			return;
		}
		_isSearchingLocation = true;
		ObservableCollection<ILocation> observableCollection = new ObservableCollection<ILocation>();
		foreach (ILocation favoriteLocation in _favoriteLocationList)
		{
			if (favoriteLocation.Country.ToLower().Contains(obj))
			{
				observableCollection.Add(favoriteLocation);
			}
			if (favoriteLocation.City.ToLower().Contains(obj))
			{
				observableCollection.Add(favoriteLocation);
			}
		}
		LocationsList.ItemsSource = observableCollection.OrderBy((ILocation x) => x.Country).Distinct();
		if (SdkMonitor.NextAiVpnSdkManager.IsConnected)
		{
			SetCountryFlagImageStatusAfterSearching();
		}
	}

	public void GetSdk(SDKMonitor sdkMonitor)
	{
		SdkMonitor = sdkMonitor;
	}

	public void FetchFavoriteLocations()
	{
		_favoriteLocationList = new ObservableCollection<ILocation>((from x in FavoriteLocationsServices.GetFavoriteLocations()
			orderby x.Country
			select x).Distinct());
		if (_isSearchingLocation)
		{
			SearchTextChangedFunc(LocationsListHeader.SearchTextBlock.Text);
		}
		else
		{
			LocationsList.ItemsSource = _favoriteLocationList;
		}
		if (SdkMonitor.NextAiVpnSdkManager.IsConnected)
		{
			SetCountryFlagImageStatusAfterSearching();
		}
		if (SdkMonitor.NextAiVpnSdkManager.IsConnecting)
		{
			SetCountryFlagImageStatusAfterSearching();
		}
	}

	private async void SetCountryFlagImageStatusAfterSearching()
	{
		try
		{
			LocationsList.Dispatcher.InvokeAsync(delegate
			{
				foreach (object item in (IEnumerable)LocationsList.Items)
				{
					if (LocationsList.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem parent)
					{
						FavoriteLocationListItem favoriteLocationListItem = FindVisualChild<FavoriteLocationListItem>(parent);
						if (favoriteLocationListItem != null && favoriteLocationListItem.FindName("CountryName") is TextBlock textBlock && textBlock.Text.Equals(SdkMonitor.NextAiVpnLocation.Country) && favoriteLocationListItem.FindName("CountryFlag") is Image image)
						{
							AnimateFlagImage(new BitmapImage(new Uri(GetImageSourceString(ConnectionStatus.Connected), UriKind.Relative)), image, ConnectionStatus.Connected);
							if (image.DataContext is ILocation location)
							{
								if (location.CityCode == SdkMonitor.NextAiVpnLocation.CityCode)
								{
									AnimateFlagImage(new BitmapImage(new Uri(GetImageSourceString(ConnectionStatus.Connected), UriKind.Relative)), image, ConnectionStatus.Connected);
								}
								else
								{
									AnimateFlagImage(new BitmapImage(new Uri(GetImageSourceString(ConnectionStatus.Disconnected), UriKind.Relative)), image, ConnectionStatus.Disconnected);
								}
							}
						}
					}
				}
			}, DispatcherPriority.Background);
		}
		catch (Exception exception)
		{
			Utils.Logger.Error(exception, "SetCountryFlagImageStatusAfterSearching", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Favotite\\FavoriteLocationsTab.xaml.cs", 204);
		}
	}

	private void SpinnerAnimation(bool animate, Image image)
	{
		_da = new DoubleAnimation();
		_rt = new RotateTransform();
		_da.From = 0.0;
		_da.To = 360.0;
		_da.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_da.RepeatBehavior = RepeatBehavior.Forever;
		image.RenderTransform = _rt;
		image.RenderTransformOrigin = new Point(0.5, 0.5);
		if (animate)
		{
			_rt.BeginAnimation(RotateTransform.AngleProperty, _da);
			return;
		}
		_rt.BeginAnimation(RotateTransform.AngleProperty, null);
		_rt = null;
		_da = null;
	}

	private void FavoriteLocationListItem_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		try
		{
			if (e.OriginalSource is DependencyObject depObj)
			{
				Image img = FindVisualParent<Image>(depObj);
				if (img != null && img.Name == "RemoveFavoriteLocation")
				{
					return;
				}
				System.Windows.Shapes.Ellipse ell = FindVisualParent<System.Windows.Shapes.Ellipse>(depObj);
				if (ell != null && ell.Name == "ExpanderHover")
				{
					return;
				}
			}

			if (sender is FavoriteLocationListItem { DataContext: var dataContext } && dataContext is ILocation location)
			{
				SelectLocationFast(location);
				e.Handled = true;
			}
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "FavoriteLocationListItem_OnMouseDown", "FavoriteLocationsTab.xaml.cs", 244);
		}
	}

	private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
	{
		while (child != null)
		{
			if (child is T parent)
			{
				return parent;
			}
			child = VisualTreeHelper.GetParent(child);
		}
		return null;
	}

	/// <summary>
	/// [VI] Chọn nhanh vị trí yêu thích, cập nhật MainPanel bên phải NGAY LẬP TỨC trong < 1ms
	/// [EN] Fast-select favorite location, update right MainPanel UI INSTANTLY in < 1ms
	/// </summary>
	public void SelectLocationFast(ILocation loc)
	{
		if (loc == null) return;
		if (SdkMonitor == null)
		{
			SdkMonitor = (Application.Current?.MainWindow as VPNWindowExpanded)?.SdkObject;
		}
		if (SdkMonitor == null) return;
		try
		{
			SdkMonitor.SetLocation(loc);
			if (SdkMonitor.VpnExpandedWindow?.Mainpanel != null)
			{
				var mainPanel = SdkMonitor.VpnExpandedWindow.Mainpanel;
				mainPanel.Dispatcher.Invoke(() =>
				{
					if (mainPanel.Location != null)
					{
						mainPanel.Location.Text = loc.Country;
					}
					if (mainPanel.City != null)
					{
						mainPanel.City.Text = loc.City;
					}
					if (mainPanel.locationImage != null)
					{
						mainPanel.locationImage.Source = NextAiVPN.Converters.FlagConverterHelper.GetFlagByCountryCode(loc.CountryCode);
					}
				});
			}

			// Cập nhật viền highlight cam cho mục trong tab Favorites
			UpdateFavoriteVisualSelection(loc);

			_ = Task.Run(async () =>
			{
				try
				{
					await NextAiVPN.Services.NextAiLocationService.SelectUpstreamProxyAsync(loc.Id);
				}
				catch { }
			});

			if (SdkMonitor.NextAiVpnSdkManager != null && SdkMonitor.NextAiVpnSdkManager.IsConnected)
			{
				_ = Task.Run(async () =>
				{
					try
					{
						await SdkMonitor.ConnectToVPN(loc);
					}
					catch { }
				});
			}
		}
		catch (Exception ex)
		{
			Utils.Logger?.Error(ex.Message, "FavoriteLocationsTab.SelectLocationFast");
		}
	}

	/// <summary>
	/// [VI] Cập nhật hiệu ứng thị giác cho vị trí yêu thích đang chọn
	/// [EN] Update visual highlight state for currently selected favorite location
	/// </summary>
	public void UpdateFavoriteVisualSelection(ILocation activeLoc)
	{
		if (activeLoc == null || LocationsList == null) return;
		Dispatcher.InvokeAsync(() =>
		{
			try
			{
				foreach (object item in (IEnumerable)LocationsList.Items)
				{
					if (LocationsList.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem lbi)
					{
						FavoriteLocationListItem listItem = FindVisualChild<FavoriteLocationListItem>(lbi);
						if (listItem != null && listItem.DataContext is ILocation loc)
						{
							bool isSelected = string.Equals(loc.Id, activeLoc.Id, StringComparison.OrdinalIgnoreCase);
							listItem.SetSelectedVisualState(isSelected);
							lbi.IsSelected = isSelected;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utils.Logger?.Error(ex.Message, "UpdateFavoriteVisualSelection");
			}
		});
	}

	public void InitiateVpnConnection(ILocation location)
	{
		SdkMonitor.ConnectedVpnMode = VpnType.NextAiVPN;
		SdkMonitor.SetLocation(location);
		SdkMonitor.ConnectToVPN(location);
	}

	private void LocationsList_OnLoaded(object sender, RoutedEventArgs e)
	{
		FetchFavoriteLocations();
	}

	public async void SetCountryFlagImageStatus(ConnectionStatus status)
	{
		try
		{
			foreach (object item in (IEnumerable)LocationsList.Items)
			{
				if (!(LocationsList.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem parent))
				{
					continue;
				}
				FavoriteLocationListItem favoriteLocationListItem = FindVisualChild<FavoriteLocationListItem>(parent);
				if (favoriteLocationListItem != null && favoriteLocationListItem.FindName("CountryName") is TextBlock textBlock && textBlock.Text.Equals(SdkMonitor.NextAiVpnLocation.Country) && favoriteLocationListItem.FindName("CityName") is TextBlock textBlock2 && textBlock2.Text.Equals(SdkMonitor.NextAiVpnLocation.City) && favoriteLocationListItem.FindName("CountryFlag") is Image image)
				{
					if (!_isSearchingLocation)
					{
						AnimateFlagImage(new BitmapImage(new Uri(GetImageSourceString(status), UriKind.Relative)), image, status);
						break;
					}
					if (image.DataContext is ILocation location && location.CityCode == SdkMonitor.NextAiVpnLocation.CityCode)
					{
						AnimateFlagImage(new BitmapImage(new Uri(GetImageSourceString(status), UriKind.Relative)), image, status);
						break;
					}
				}
			}
		}
		catch (Exception exception)
		{
			Utils.Logger.Error(exception, "SetCountryFlagImageStatus", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Favotite\\FavoriteLocationsTab.xaml.cs", 318);
		}
	}

	private void AnimateFlagImage(ImageSource imageSource, Image image, ConnectionStatus status)
	{
		image.Source = imageSource;
		SpinnerAnimation(status == ConnectionStatus.Connecting, image);
	}

	private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
	{
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);
			if (child != null && child is T)
			{
				return (T)child;
			}
			T val = FindVisualChild<T>(child);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	private string GetImageSourceString(ConnectionStatus status)
	{
		return status switch
		{
			ConnectionStatus.Connecting => IconHelper.GetIcon("spinnerblue_whitebg"), 
			ConnectionStatus.Connected => IconHelper.GetIcon("success_green"), 
			ConnectionStatus.Disconnected => "/Resources/Flags/" + SdkMonitor.NextAiVpnLocation.CountryCode + ".png", 
			_ => IconHelper.GetIcon("success_green"), 
		};
	}
}
