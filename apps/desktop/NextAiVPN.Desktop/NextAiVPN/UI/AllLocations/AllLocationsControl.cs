using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DynamicData.Binding;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN.UI.AllLocations;

public partial class AllLocationsControl : UserControl, IComponentConnector
{
	private DoubleAnimation _da;

	private RotateTransform _rt;

	private ICollectionView _collectionView;

	private readonly string _searchPlaceholder = "Search";

	private DispatcherTimer _pingLocationTimer;

	private readonly ObservableCollectionExtended<ILocation> _tempDestinations = new ObservableCollectionExtended<ILocation>();

	private IEnumerable<IList<ILocation>> _locations = new ObservableCollectionExtended<IList<ILocation>>();

	public SDKMonitor SdkMonitor { get; private set; }

	public bool IsSearching { get; private set; }

	public AllLocationsControl()
	{
		InitializeComponent();
		LocationsListHeader locationsListHeader = LocationsListHeader;
		locationsListHeader.SearchTextChangedFunc = (Action<string>)Delegate.Combine(locationsListHeader.SearchTextChangedFunc, new Action<string>(SearchTextChangedFunc));
		LocationsListHeader locationsListHeader2 = LocationsListHeader;
		locationsListHeader2.CountrySortFunc = (Action<LocationsOrder, bool>)Delegate.Combine(locationsListHeader2.CountrySortFunc, new Action<LocationsOrder, bool>(SortList));
		LocationsListHeader locationsListHeader3 = LocationsListHeader;
		locationsListHeader3.PingSortFunc = (Action<LocationsOrder, bool>)Delegate.Combine(locationsListHeader3.PingSortFunc, new Action<LocationsOrder, bool>(SortList));
		LocationsListHeader locationsListHeader4 = LocationsListHeader;
		locationsListHeader4.LoadSortFunc = (Action<LocationsOrder, bool>)Delegate.Combine(locationsListHeader4.LoadSortFunc, new Action<LocationsOrder, bool>(SortList));
		LocationsListHeader locationsListHeader5 = LocationsListHeader;
		locationsListHeader5.FavoritesSortFunc = (Action<LocationsOrder, bool>)Delegate.Combine(locationsListHeader5.FavoritesSortFunc, new Action<LocationsOrder, bool>(FavoritesSortFunc));
		SetPingLocationTimer();
	}

	private void SetPingLocationTimer()
	{
		_pingLocationTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMinutes(15L)
		};
		_pingLocationTimer.Tick += _pingLocationTimer_Tick;
	}

	private async void _pingLocationTimer_Tick(object sender, EventArgs e)
	{
		await PingLocations();
	}

	private void FavoritesSortFunc(LocationsOrder arg1, bool arg2)
	{
		string[] array = Utils.AppSettingsHelper.GetValue("FavoritesList").Split(';');
		if (array.Length != 0)
		{
			List<string> list = new List<string>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!string.IsNullOrEmpty(text))
				{
					string text2 = text;
					if (text2.Length > 3)
					{
						text2 = text.Substring(3, 3);
					}
					list.Add(text2);
				}
			}
			_tempDestinations.Clear();
			if (arg2)
			{
				LoadFavoritesToTemp(list);
				LoadNotFavoritesToTemp(list);
			}
			else
			{
				LoadNotFavoritesToTemp(list);
				LoadFavoritesToTemp(list);
			}
			_collectionView = CollectionViewSource.GetDefaultView(from x in _tempDestinations
				where !x.Id.Equals("bestavailable")
				group x by x.Country into x
				select x.ToList());
			_collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Country"));
			LocationsList.ItemsSource = _collectionView;
		}
		else
		{
			_collectionView = CollectionViewSource.GetDefaultView(SdkMonitor.NextAiVpnSdkManager.Locations);
			_collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Country"));
			_collectionView.SortDescriptions.Add(new SortDescription("Country", ListSortDirection.Ascending));
			_collectionView.SortDescriptions.Add(new SortDescription("City", ListSortDirection.Ascending));
			LocationsList.ItemsSource = _collectionView;
		}
	}

	private void SearchTextChangedFunc(string obj)
	{
		Search(obj);
	}

	public void GetSdk(SDKMonitor sdkMonitor)
	{
		SdkMonitor = sdkMonitor;
		BindLocations();
	}

	private bool _isUpdatingSelection = false;

	private void LocationsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_isUpdatingSelection) return;
		try
		{
			if (LocationsList.SelectedIndex == -1)
			{
				return;
			}
			object selectedItem = LocationsList.SelectedItem;
			ILocation targetLoc = null;
			if (selectedItem is ILocation loc)
			{
				targetLoc = loc;
			}
			else if (selectedItem is List<ILocation> { Count: > 0 } listLoc)
			{
				targetLoc = listLoc[0];
			}
			else if (selectedItem is IList<ILocation> { Count: > 0 } ilistLoc)
			{
				targetLoc = ilistLoc[0];
			}
			else if (selectedItem is System.Windows.Data.CollectionViewGroup cvg && cvg.Items.Count > 0)
			{
				if (cvg.Items[0] is ILocation cvgLoc) targetLoc = cvgLoc;
				else if (cvg.Items[0] is IList<ILocation> cvgList && cvgList.Count > 0) targetLoc = cvgList[0];
			}
			else if (selectedItem is IEnumerable<ILocation> enumLoc)
			{
				targetLoc = enumLoc.FirstOrDefault();
			}

			if (targetLoc != null)
			{
				SelectLocationFast(targetLoc);
			}
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "LocationsList_OnSelectionChanged", "AllLocationsControl.xaml.cs", 162);
		}
	}

	/// <summary>
	/// [VI] Chọn nhanh vị trí kết nối, cập nhật giao diện MainPanel bên phải NGAY LẬP TỨC trong < 1ms
	/// [EN] Fast-select location, update right MainPanel UI INSTANTLY in < 1ms
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
			// 1. Cập nhật dữ liệu cấu hình trong SDKMonitor
			SdkMonitor.SetLocation(loc);

			// 2. Cập nhật trực tiếp lên UI MainPanel bên phải NGAY LẬP TỨC (< 1ms)
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

			// 3. Đồng bộ highlight thị giác trên toàn bộ danh sách LocationsList
			UpdateAllItemsVisualSelection(loc);

			// 4. Báo Backend CMS ngầm cập nhật upstream proxy đã chọn (không chặn UI thread)
			_ = Task.Run(async () =>
			{
				try
				{
					await NextAiVPN.Services.NextAiLocationService.SelectUpstreamProxyAsync(loc.Id);
				}
				catch { }
			});

			// 5. Nếu VPN đang ở trạng thái Connected thì mới gọi reconnect ngầm
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
			Utils.Logger?.Error(ex.Message, "SelectLocationFast");
		}
	}

	/// <summary>
	/// [VI] Đồng bộ highlight thị giác cho vị trí đã chọn trên toàn bộ danh sách
	/// [EN] Synchronize visual highlight for selected location across the entire list
	/// </summary>
	public void UpdateAllItemsVisualSelection(ILocation activeLoc)
	{
		if (activeLoc == null || LocationsList == null) return;
		Dispatcher.InvokeAsync(() =>
		{
			_isUpdatingSelection = true;
			try
			{
				foreach (object item in (IEnumerable)LocationsList.Items)
				{
					if (LocationsList.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem lbi)
					{
						AllLocationListItem listItem = FindVisualChild<AllLocationListItem>(lbi);
						if (listItem != null)
						{
							ILocation itemLoc = listItem.ResolveLocation();
							bool isSelected = itemLoc != null && itemLoc.Country == activeLoc.Country;
							listItem.SetSelectedVisualState(isSelected, activeLoc.Id, activeLoc.City);
							lbi.IsSelected = isSelected;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utils.Logger?.Error(ex.Message, "UpdateAllItemsVisualSelection");
			}
			finally
			{
				_isUpdatingSelection = false;
			}
		});
	}

	public void InitiateVpnConnection(ILocation location)
	{
		SdkMonitor.ConnectedVpnMode = VpnType.NextAiVPN;
		SdkMonitor.SetLocation(location);
		SdkMonitor.ConnectToVPN(location);
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

	private void LocationsList_OnLoaded(object sender, RoutedEventArgs e)
	{
		foreach (object item in (IEnumerable)LocationsList.Items)
		{
			if (LocationsList.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement frameworkElement && frameworkElement.FindName("AllLocationListItem") is AllLocationListItem allLocationListItem)
			{
				allLocationListItem.ParentControl = this;
			}
		}
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
				AllLocationListItem allLocationListItem = FindVisualChild<AllLocationListItem>(parent);
				if (allLocationListItem == null || !(allLocationListItem.FindName("CountryName") is TextBlock textBlock) || !textBlock.Text.Equals(SdkMonitor.NextAiVpnLocation.Country))
				{
					continue;
				}
				ListBox listBox = allLocationListItem.FindName("InnerLocationsList") as ListBox;
				FindCountryFlagByCityName(status, listBox);
				if (allLocationListItem.FindName("CountryFlag") is Image image)
				{
					if (!IsSearching)
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
			Utils.Logger.Error(exception, "SetCountryFlagImageStatus", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\AllLocations\\AllLocationsControl.xaml.cs", 260);
		}
	}

	private void AnimateFlagImage(ImageSource imageSource, Image image, ConnectionStatus status)
	{
		image.Source = imageSource;
		SpinnerAnimation(status == ConnectionStatus.Connecting, image);
	}

	private void FindCountryFlagByCityName(ConnectionStatus status, ListBox listBox)
	{
		foreach (object item in (IEnumerable)listBox.Items)
		{
			if (!(listBox.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem parent))
			{
				continue;
			}
			TextBlock textBlock = FindVisualChild<TextBlock>(parent, "CityName");
			if (textBlock != null && textBlock.Text.Equals(SdkMonitor.NextAiVpnLocation.City))
			{
				Image image = FindVisualChild<Image>(parent, "CountryFlag");
				if (image != null)
				{
					BitmapImage imageSource = ((status == ConnectionStatus.Disconnected) ? null : new BitmapImage(new Uri(GetImageSourceString(status), UriKind.Relative)));
					AnimateFlagImage(imageSource, image, status);
					break;
				}
			}
		}
	}

	private static T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
	{
		if (parent == null)
		{
			return null;
		}
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);
			if (child is T val && val.Name == name)
			{
				return val;
			}
			T val2 = FindVisualChild<T>(child, name);
			if (val2 != null)
			{
				return val2;
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

	public async Task PingLocations()
	{
		_pingLocationTimer.Stop();
		await PingLocationList(SdkMonitor.NextAiVpnSdkManager.Locations);
		IEnumerable<ILocation> locations = SdkMonitor.NextAiVpnSdkManager.Locations.Where((ILocation x) => !x.PingMs.HasValue || x.PingMs == 0);
		await PingLocationList(locations);
		BindPingedLocations();
		if (!LocationsListHeader.SearchTextBlock.Text.Equals(string.Empty))
		{
			Search(LocationsListHeader.SearchTextBlock.Text);
		}
		_pingLocationTimer.Start();
	}

	private async Task PingLocationList(IEnumerable<ILocation> locations)
	{
		foreach (ILocation location in locations)
		{
			try
			{
				if (location != null && !location.PingMs.HasValue)
				{
					await location.Ping();
				}
			}
			catch (Exception exception)
			{
				Utils.Logger.Error(exception, "PingLocationList", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\AllLocations\\AllLocationsControl.xaml.cs", 385);
			}
		}
	}

	private void BindPingedLocations()
	{
		Dispatcher.Invoke(() =>
		{
			try
			{
				_locations = from x in SdkMonitor.NextAiVpnSdkManager.Locations
					where !x.Id.Equals("bestavailable")
					orderby x.Country, x.PingMs
					group x by x.Country into x
					select x.ToList();
				_collectionView = CollectionViewSource.GetDefaultView(_locations);
				_collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Country"));
				_collectionView.SortDescriptions.Add(new SortDescription("Country", ListSortDirection.Ascending));
				_collectionView.SortDescriptions.Add(new SortDescription("City", ListSortDirection.Ascending));
				LocationsList.ItemsSource = _collectionView;
			}
			catch (Exception ex)
			{
				Utils.Logger?.Error(ex.Message, "BindPingedLocations");
			}
		});
	}

	private void BindLocations()
	{
		Dispatcher.Invoke(() =>
		{
			try
			{
				_locations = from x in SdkMonitor.NextAiVpnSdkManager.Locations
					where !x.Id.Equals("bestavailable")
					orderby x.Country
					group x by x.Country into x
					select x.ToList();
				_collectionView = CollectionViewSource.GetDefaultView(_locations);
				_collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Country"));
				_collectionView.SortDescriptions.Add(new SortDescription("Country", ListSortDirection.Ascending));
				_collectionView.SortDescriptions.Add(new SortDescription("City", ListSortDirection.Ascending));
				LocationsList.ItemsSource = _collectionView;
			}
			catch (Exception ex)
			{
				Utils.Logger?.Error(ex.Message, "BindLocations");
			}
		});
	}

	private void SortList(LocationsOrder arg1, bool arg2)
	{
		_collectionView.GroupDescriptions.Clear();
		_collectionView.SortDescriptions.Clear();
		ListSortDirection direction = ((!arg2) ? ListSortDirection.Descending : ListSortDirection.Ascending);
		string propertyName = ((arg1 == LocationsOrder.Ping) ? "PingMs" : arg1.ToString());
		LocationsList.Items.SortDescriptions.Clear();
		LocationsList.Items.SortDescriptions.Add(new SortDescription(propertyName, direction));
		_collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Country"));
		_collectionView.SortDescriptions.Add(new SortDescription("Country", ListSortDirection.Ascending));
		_collectionView.SortDescriptions.Add(new SortDescription("City", ListSortDirection.Ascending));
	}

	private void LoadFavoritesToTemp(List<string> list)
	{
		foreach (string fav in list)
		{
			ILocation location = SdkMonitor.NextAiVpnSdkManager.Locations.Where((ILocation x) => x.Id == fav).First();
			if (location != null && !_tempDestinations.Contains(location))
			{
				_tempDestinations.Add(location);
			}
		}
	}

	private void LoadNotFavoritesToTemp(List<string> list)
	{
		foreach (ILocation location in SdkMonitor.NextAiVpnSdkManager.Locations)
		{
			if (!list.Contains(location.Id) && location.Id != "bestavailable")
			{
				_tempDestinations.Add(location);
			}
		}
	}

	private void SetLocations(ObservableCollectionExtended<ILocation> locations)
	{
		_collectionView = CollectionViewSource.GetDefaultView(locations);
		_collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Country"));
		_collectionView.SortDescriptions.Add(new SortDescription("Country", ListSortDirection.Ascending));
		_collectionView.SortDescriptions.Add(new SortDescription("City", ListSortDirection.Ascending));
		LocationsList.ItemsSource = _collectionView;
	}

	private void Search(string searchString)
	{
		try
		{
			_collectionView.GroupDescriptions.Clear();
			_collectionView.SortDescriptions.Clear();
			if (searchString.Length == 0 || searchString == _searchPlaceholder)
			{
				IsSearching = false;
				BindLocations();
				return;
			}
			IsSearching = true;
			_tempDestinations.Clear();
			foreach (ILocation location in SdkMonitor.NextAiVpnSdkManager.Locations)
			{
				if (!location.Id.Equals("bestavailable") && location.SearchName.ToUpper().Contains(searchString.ToUpper()))
				{
					_tempDestinations.Add(location);
				}
			}
			_locations = from x in _tempDestinations
				group x by x.Country into x
				select x.ToList();
			_collectionView = CollectionViewSource.GetDefaultView(_locations);
			_collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Country"));
			_collectionView.SortDescriptions.Add(new SortDescription("Country", ListSortDirection.Ascending));
			_collectionView.SortDescriptions.Add(new SortDescription("City", ListSortDirection.Ascending));
			LocationsList.ItemsSource = _collectionView;
			if (SdkMonitor.NextAiVpnSdkManager.IsConnected)
			{
				SetCountryFlagImageStatusAfterSearching();
			}
		}
		catch (Exception exception)
		{
			Utils.Logger.Error(exception, "Search", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\AllLocations\\AllLocationsControl.xaml.cs", 513);
		}
	}

	internal async void SetCountryFlagImageStatusAfterSearching()
	{
		try
		{
			LocationsList.Dispatcher.InvokeAsync(delegate
			{
				foreach (object item in (IEnumerable)LocationsList.Items)
				{
					if (LocationsList.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem parent)
					{
						AllLocationListItem allLocationListItem = FindVisualChild<AllLocationListItem>(parent);
						if (allLocationListItem != null && allLocationListItem.FindName("CountryName") is TextBlock textBlock && textBlock.Text.Equals(SdkMonitor.NextAiVpnLocation.Country) && allLocationListItem.FindName("CountryFlag") is Image image)
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
			Utils.Logger.Error(exception, "SetCountryFlagImageStatusAfterSearching", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\AllLocations\\AllLocationsControl.xaml.cs", 579);
		}
	}

	private void LocationsList_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (!e.Handled)
		{
			ScrollViewer scrollViewer = GetScrollViewer(LocationsList);
			if (scrollViewer != null)
			{
				scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (double)e.Delta);
				e.Handled = true;
			}
		}
	}

	private ScrollViewer GetScrollViewer(DependencyObject depObj)
	{
		if (depObj is ScrollViewer)
		{
			return depObj as ScrollViewer;
		}
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
			ScrollViewer scrollViewer = GetScrollViewer(child);
			if (scrollViewer != null)
			{
				return scrollViewer;
			}
		}
		return null;
	}
}
