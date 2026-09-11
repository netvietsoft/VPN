using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN.UI.AllLocations;

public partial class AllLocationListItem : UserControl, IComponentConnector, IStyleConnector
{
	private string _favoriteValue;

	private bool _isFavorite;

	private bool _isLocationsExpanded;

	public AllLocationsControl ParentControl { get; set; }

	public AllLocationListItem()
	{
		InitializeComponent();
		base.Loaded += AllLocationListItem_Loaded;
	}

	private void AllLocationListItem_Loaded(object sender, RoutedEventArgs e)
	{
		if (base.Tag is AllLocationsControl parentControl)
		{
			ParentControl = parentControl;
		}
	}

	private void AllLocationListItem_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		try
		{
			List<ILocation> list = null;
			if (base.DataContext is List<ILocation> l)
			{
				list = l;
			}
			else if (base.DataContext is IList<ILocation> il)
			{
				list = il.ToList();
			}
			else if (base.DataContext is System.Windows.Data.CollectionViewGroup cvg)
			{
				list = cvg.Items.OfType<ILocation>().ToList();
				if (list.Count == 0)
				{
					foreach (var it in cvg.Items)
					{
						if (it is IList<ILocation> sub) list.AddRange(sub);
					}
				}
			}
			else if (base.DataContext is IEnumerable<ILocation> en)
			{
				list = en.ToList();
			}

			if (list != null && list.Count > 0)
			{
				LocationItemHoverBorder.DataContext = list[0];
				if (list.Count > 1)
				{
					ExpanderGrid.Visibility = Visibility.Visible;
					InnerLocationsList.ItemsSource = new ObservableCollection<ILocation>(list);
					CityName.Text = $"{list.Count} Locations";
					CityName.Visibility = Visibility.Visible;
				}
				else
				{
					ExpanderGrid.Visibility = Visibility.Collapsed;
					InnerLocationsList.ItemsSource = null;
					CollapseInnerLocations();
					CityName.Text = list[0].City;
					CityName.Visibility = Visibility.Visible;
				}
			}
			else if (base.DataContext is ILocation loc)
			{
				LocationItemHoverBorder.DataContext = loc;
				ExpanderGrid.Visibility = Visibility.Collapsed;
				InnerLocationsList.ItemsSource = null;
				CollapseInnerLocations();
				CityName.Text = loc.City;
				CityName.Visibility = Visibility.Visible;
			}
			else
			{
				LocationItemHoverBorder.DataContext = null;
				ExpanderGrid.Visibility = Visibility.Collapsed;
				InnerLocationsList.ItemsSource = null;
				CollapseInnerLocations();
			}
		}
		catch (Exception ex)
		{
			Utils.Logger?.Error(ex.Message, "AllLocationListItem_OnDataContextChanged", "AllLocationListItem.xaml.cs", 56);
		}
	}

	private void Expander_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		_isLocationsExpanded = !_isLocationsExpanded;
		if (_isLocationsExpanded)
		{
			ExpandInnerLocations();
		}
		else
		{
			CollapseInnerLocations();
		}
		e.Handled = true;
	}

	private void CollapseInnerLocations()
	{
		InnerLocationsGrid.Visibility = Visibility.Collapsed;
		ExpanderImage.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
	}

	private void ExpandInnerLocations()
	{
		InnerLocationsGrid.Visibility = Visibility.Visible;
		ExpanderImage.Source = new BitmapImage(new Uri("/Assets/carretup.png", UriKind.Relative));
	}

	private void ExpanderHover_OnMouseEnter(object sender, MouseEventArgs e)
	{
		ExpanderHover.Fill = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush(GetExpanderBackgroundColor());
	}

	private static string GetExpanderBackgroundColor()
	{
		if (StyleModeDefiner.DefineAppStyle() != NextAiVPN.Services.Persistence.Style.Dark)
		{
			return "#F2F2F6";
		}
		return "#3A3B40";
	}

	private void ExpanderHover_OnMouseLeave(object sender, MouseEventArgs e)
	{
		ExpanderHover.Fill = Brushes.Transparent;
	}

	/// <summary>
	/// [VI] Giải quyết đối tượng ILocation an toàn từ DataContext hoặc HoverBorder
	/// [EN] Safely resolve ILocation object from DataContext or HoverBorder
	/// </summary>
	public ILocation ResolveLocation()
	{
		if (LocationItemHoverBorder?.DataContext is ILocation locBorder)
		{
			return locBorder;
		}
		if (base.DataContext is ILocation locDirect)
		{
			return locDirect;
		}
		if (base.DataContext is List<ILocation> { Count: > 0 } list)
		{
			return list[0];
		}
		if (base.DataContext is IList<ILocation> { Count: > 0 } ilist)
		{
			return ilist[0];
		}
		if (base.DataContext is System.Windows.Data.CollectionViewGroup cvg && cvg.Items.Count > 0)
		{
			if (cvg.Items[0] is ILocation cvgLoc) return cvgLoc;
			if (cvg.Items[0] is IList<ILocation> cvgList && cvgList.Count > 0) return cvgList[0];
		}
		if (base.DataContext is IEnumerable<ILocation> enumLoc)
		{
			return enumLoc.FirstOrDefault();
		}
		return null;
	}

	/// <summary>
	/// [VI] Cập nhật giao diện khi mục này được chọn (highlight viền cam + nền tối nổi bật)
	/// [EN] Update visual state when this item is selected (highlight orange border + distinct dark background)
	/// </summary>
	public void SetSelectedVisualState(bool isSelected, string selectedLocationId = null, string selectedCity = null)
	{
		try
		{
			if (LocationItemHoverBorder != null)
			{
				if (isSelected)
				{
					LocationItemHoverBorder.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#ECFDF5"));
					LocationItemHoverBorder.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981"));
					LocationItemHoverBorder.BorderThickness = new Thickness(1.5);
				}
				else
				{
					LocationItemHoverBorder.Background = Brushes.Transparent;
					LocationItemHoverBorder.BorderBrush = Brushes.Transparent;
					LocationItemHoverBorder.BorderThickness = new Thickness(1.5);
				}
			}

			// Cập nhật trạng thái cho từng node/thành phố con trong danh sách mở rộng (nếu có)
			if (InnerLocationsList != null && InnerLocationsList.Items.Count > 0)
			{
				foreach (var item in InnerLocationsList.Items)
				{
					if (InnerLocationsList.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem lbi)
					{
						Border innerBorder = FindChild<Border>(lbi, "InnerLocationItemHoverBorder") ?? FindVisualChild<Border>(lbi);
						if (innerBorder != null && item is ILocation childLoc)
						{
							bool isChildSelected = isSelected && (
								(!string.IsNullOrEmpty(selectedLocationId) && string.Equals(childLoc.Id, selectedLocationId, StringComparison.OrdinalIgnoreCase)) ||
								(string.IsNullOrEmpty(selectedLocationId) && !string.IsNullOrEmpty(selectedCity) && childLoc.City == selectedCity)
							);
							if (isChildSelected)
							{
								innerBorder.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#ECFDF5"));
								innerBorder.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981"));
								innerBorder.BorderThickness = new Thickness(1.5);
							}
							else
							{
								innerBorder.Background = Brushes.Transparent;
								innerBorder.BorderBrush = Brushes.Transparent;
								innerBorder.BorderThickness = new Thickness(1.5);
							}
						}
					}
				}
			}
		}
		catch { }
	}

	/// <summary>
	/// [VI] Xử lý chọn vị trí khi click chuột vào bất kỳ vị trí nào trên hàng
	/// [EN] Handle location selection on mouse click anywhere on the item row
	/// </summary>
	private void ExecuteLocationSelection(object sender, RoutedEventArgs e)
	{
		try
		{
			// Không kích hoạt nếu bấm vào nút Favorite (ngôi sao) hoặc Expander (mũi tên)
			if (e is MouseButtonEventArgs me && me.OriginalSource is DependencyObject depObj)
			{
				Image img = FindVisualParent<Image>(depObj);
				if (img != null && (img.Name == "Favorite" || img.Name == "ExpanderImage"))
				{
					return;
				}
				Ellipse ell = FindVisualParent<Ellipse>(depObj);
				if (ell != null && ell.Name == "ExpanderHover")
				{
					return;
				}
				Grid expGrid = FindVisualParent<Grid>(depObj);
				if (expGrid != null && expGrid.Name == "ExpanderGrid")
				{
					return;
				}
			}

			ILocation loc = ResolveLocation();

			// Nếu quốc gia có nhiều thành phố, tự động đóng/mở Accordion
			bool isMultiCity = false;
			if (base.DataContext is List<ILocation> { Count: > 1 } ||
			    base.DataContext is IList<ILocation> { Count: > 1 } ||
			    (InnerLocationsList?.ItemsSource is IEnumerable<ILocation> en && en.Count() > 1))
			{
				isMultiCity = true;
			}

			if (isMultiCity)
			{
				if (_isLocationsExpanded)
				{
					_isLocationsExpanded = false;
					CollapseInnerLocations();
				}
				else
				{
					CollapseOtherExpandedLocations();
					_isLocationsExpanded = true;
					ExpandInnerLocations();
				}
			}

			if (loc != null)
			{
				SetSelectedVisualState(true, loc.Id, loc.City);
				AllLocationsControl parent = GetParentAllLocationsControl();
				parent?.SelectLocationFast(loc);
				this.BringIntoView();
			}

			if (e is MouseButtonEventArgs mbe)
			{
				mbe.Handled = true;
			}
		}
		catch (Exception ex)
		{
			Utils.Logger?.Error(ex.Message, "ExecuteLocationSelection", "AllLocationListItem.cs", 180);
		}
	}

	/// <summary>
	/// [VI] Xử lý click trực tiếp vào nút Expander (mũi tên xổ xuống / thu gọn)
	/// [EN] Handle direct click on Expander button (expand / collapse caret)
	/// </summary>
	private void ExpanderGrid_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		try
		{
			_isLocationsExpanded = !_isLocationsExpanded;
			if (_isLocationsExpanded)
			{
				CollapseOtherExpandedLocations();
				ExpandInnerLocations();
			}
			else
			{
				CollapseInnerLocations();
			}
			e.Handled = true;
		}
		catch (Exception ex)
		{
			Utils.Logger?.Error(ex.Message, "ExpanderGrid_OnPreviewMouseLeftButtonDown");
		}
	}

	/// <summary>
	/// [VI] Tự động thu gọn các quốc gia khác đang mở rộng để danh sách luôn gọn gàng (Accordion UX)
	/// [EN] Automatically collapse other expanded countries to keep the list clean (Accordion UX)
	/// </summary>
	private void CollapseOtherExpandedLocations()
	{
		try
		{
			AllLocationsControl parent = GetParentAllLocationsControl();
			if (parent?.LocationsList == null) return;
			foreach (object item in (IEnumerable)parent.LocationsList.Items)
			{
				if (parent.LocationsList.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem lbi)
				{
					AllLocationListItem listItem = FindVisualChild<AllLocationListItem>(lbi);
					if (listItem != null && listItem != this && listItem._isLocationsExpanded)
					{
						listItem._isLocationsExpanded = false;
						listItem.CollapseInnerLocations();
					}
				}
			}
		}
		catch { }
	}

	private void CountryMainGrid_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		ExecuteLocationSelection(sender, e);
	}

	/// <summary>
	/// [VI] Bắt sự kiện nhấn chuột trái vào thành phố con để chọn nhanh vị trí tức thì (< 1ms)
	/// [EN] Handle mouse left click on inner city item to fast-select location instantly (< 1ms)
	/// </summary>
	private void InnerLocationItem_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		try
		{
			if (e.OriginalSource is DependencyObject depObj)
			{
				Image img = FindVisualParent<Image>(depObj);
				if (img != null && img.Name == "Favorite")
				{
					return;
				}
			}

			ILocation loc = (sender as FrameworkElement)?.DataContext as ILocation 
			                ?? (e.Source as FrameworkElement)?.DataContext as ILocation
			                ?? (e.OriginalSource as FrameworkElement)?.DataContext as ILocation
			                ?? FindVisualParent<ListBoxItem>(sender as DependencyObject)?.DataContext as ILocation
			                ?? FindVisualParent<ListBoxItem>(e.OriginalSource as DependencyObject)?.DataContext as ILocation;

			if (loc != null)
			{
				SetSelectedVisualState(true, loc.Id, loc.City);
				AllLocationsControl parent = GetParentAllLocationsControl();
				parent?.SelectLocationFast(loc);
				e.Handled = true; // [CRITICAL] Prevent bubbling to outer ListBox container!
			}
		}
		catch (Exception ex)
		{
			Utils.Logger?.Error(ex.Message, "InnerLocationItem_OnPreviewMouseLeftButtonDown", "AllLocationListItem.cs", 230);
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

	private AllLocationsControl GetParentAllLocationsControl()
	{
		if (ParentControl != null) return ParentControl;
		if (base.Tag is AllLocationsControl tagCtrl)
		{
			ParentControl = tagCtrl;
			return tagCtrl;
		}
		if (Application.Current?.MainWindow is VPNWindowExpanded vpnWin && vpnWin.ExpandedLocations?.AllLocationsControl != null)
		{
			ParentControl = vpnWin.ExpandedLocations.AllLocationsControl;
			return ParentControl;
		}
		return FindParentAllLocationsControl(this);
	}

	private static AllLocationsControl FindParentAllLocationsControl(DependencyObject child)
	{
		while (child != null)
		{
			if (child is AllLocationsControl ctrl)
			{
				return ctrl;
			}
			child = VisualTreeHelper.GetParent(child);
		}
		return null;
	}

	private bool _isUpdatingInnerSelection = false;

	private void InnerLocationsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_isUpdatingInnerSelection) return;
		e.Handled = true;
		try
		{
			_isUpdatingInnerSelection = true;
			if (InnerLocationsList.SelectedIndex != -1)
			{
				object selectedItem = InnerLocationsList.SelectedItem;
				if (selectedItem is ILocation location)
				{
					AllLocationsControl parent = GetParentAllLocationsControl();
					parent?.SelectLocationFast(location);
				}
			}
		}
		catch (Exception ex)
		{
			Utils.Logger?.Error(ex.Message, "InnerLocationsList_OnSelectionChanged", "AllLocationListItem.xaml.cs", 124);
		}
		finally
		{
			_isUpdatingInnerSelection = false;
		}
	}

	/// <summary>
	/// [VI] Xử lý click chuột trái vào ngôi sao Yêu thích trên hàng Quốc gia (thêm vào / xóa đi tức thì)
	/// [EN] Handle mouse left click on Country Favorite star (add to / remove from favorites instantly)
	/// </summary>
	private void Favorite_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		if (!(sender is Image image)) return;
		try
		{
			ILocation loc = ResolveLocation();
			if (loc == null) return;
			string favKey = GetLocationString(loc);
			bool isCurrentlyFav = Utils.FavoritesService.ContainFavorite(favKey) || Utils.FavoritesService.ContainFavorite(loc.Id);

			// Thêm vào hoặc xóa đi khỏi danh sách Yêu thích
			Utils.FavoritesService.SaveFavorites(favKey);
			bool isNowFav = !isCurrentlyFav;

			// Cập nhật biểu tượng ngôi sao ngay lập tức
			image.Source = new BitmapImage(new Uri(isNowFav ? "/Assets/favSelected.png" : IconHelper.GetIcon("favNormal"), UriKind.RelativeOrAbsolute));
			_isFavorite = isNowFav;

			// Đồng bộ số lượng trên Tab Favorites
			AllLocationsControl parent = GetParentAllLocationsControl();
			var expandedLocs = parent?.SdkMonitor?.VpnExpandedWindow?.ExpandedLocations
				?? (Application.Current?.MainWindow as VPNWindowExpanded)?.ExpandedLocations;
			if (expandedLocs != null)
			{
				expandedLocs.SetFavoriteLocationTabHeaderText();
				expandedLocs.FavoriteLocationsTabControl?.FetchFavoriteLocations();
			}
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "Favorite_OnPreviewMouseLeftButtonDown", "AllLocationListItem.cs", 380);
		}
	}

	/// <summary>
	/// [VI] Xử lý click chuột trái vào ngôi sao Yêu thích của thành phố con (thêm vào / xóa đi tức thì)
	/// [EN] Handle mouse left click on City Favorite star (add to / remove from favorites instantly)
	/// </summary>
	private void FavoriteCity_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		if (!(sender is Image image) || !(image.DataContext is ILocation location)) return;
		try
		{
			string favKey = GetLocationString(location);
			bool isCurrentlyFav = Utils.FavoritesService.ContainFavorite(favKey) || Utils.FavoritesService.ContainFavorite(location.Id);

			// Thêm vào hoặc xóa đi khỏi danh sách Yêu thích
			Utils.FavoritesService.SaveFavorites(favKey);
			bool isNowFav = !isCurrentlyFav;

			// Cập nhật biểu tượng ngôi sao thành phố con
			image.Source = new BitmapImage(new Uri(isNowFav ? "/Assets/favSelected.png" : IconHelper.GetIcon("favNormal"), UriKind.RelativeOrAbsolute));

			// Cập nhật trạng thái ngôi sao trên tiêu đề quốc gia cha
			AllLocationsControl parent = GetParentAllLocationsControl();
			var sdk = parent?.SdkMonitor ?? (Application.Current?.MainWindow as VPNWindowExpanded)?.SdkObject;
			bool hasInnerFavorites = Utils.FavoritesService.HasFavorite(location.CountryCode, sdk);
			SetHeaderFavoriteStatus(location, hasInnerFavorites);

			// Cập nhật số lượng trên Tab Favorites
			var expandedLocs = parent?.SdkMonitor?.VpnExpandedWindow?.ExpandedLocations
				?? (Application.Current?.MainWindow as VPNWindowExpanded)?.ExpandedLocations;
			if (expandedLocs != null)
			{
				expandedLocs.SetFavoriteLocationTabHeaderText();
				expandedLocs.FavoriteLocationsTabControl?.FetchFavoriteLocations();
			}
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "FavoriteCity_OnPreviewMouseLeftButtonDown", "AllLocationListItem.cs", 530);
		}
	}

	private void SetFavoriteCity(ILocation location, Image img)
	{
		// Method retained for backward compatibility
	}

	public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
	{
		if (parent == null)
		{
			return null;
		}
		T val = null;
		int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
		for (int i = 0; i < childrenCount; i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(parent, i);
			if (child as T == null)
			{
				val = FindChild<T>(child, childName);
				if (val != null)
				{
					break;
				}
				continue;
			}
			if (!string.IsNullOrEmpty(childName))
			{
				if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
				{
					val = (T)child;
					break;
				}
				continue;
			}
			val = (T)child;
			break;
		}
		return val;
	}

	private void SetFavoriteCountry(Image img, ILocation location)
	{
		if (img == null || location == null) return;
		bool isFav = Utils.FavoritesService.ContainFavorite(GetLocationString(location)) ||
		             Utils.FavoritesService.ContainFavorite(location.Id);
		img.Source = new BitmapImage(new Uri(isFav ? "/Assets/favSelected.png" : IconHelper.GetIcon("favNormal"), UriKind.RelativeOrAbsolute));
		_isFavorite = isFav;
	}

	private bool IsFavorite(Image img)
	{
		return img?.Source != null && img.Source.ToString().Contains("favSelected");
	}

	private string GetLocationString(ILocation location)
	{
		if (location == null) return string.Empty;
		return location.CountryCode + "_" + location.Id;
	}

	private void SetHeaderFavoriteStatus(ILocation loc, bool hasInnerFavorites)
	{
		try
		{
			AllLocationsControl parent = GetParentAllLocationsControl();
			if (parent?.LocationsList == null) return;

			foreach (object item in (IEnumerable)parent.LocationsList.Items)
			{
				if (!(parent.LocationsList.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem lbi))
				{
					continue;
				}
				AllLocationListItem allLocationListItem = FindVisualChild<AllLocationListItem>(lbi);
				if (allLocationListItem == null || !(allLocationListItem.FindName("CountryName") is TextBlock textBlock) || !textBlock.Text.Equals(loc.Country) || !(allLocationListItem.FindName("Favorite") is Image image))
				{
					continue;
				}
				if (hasInnerFavorites)
				{
					image.Source = new BitmapImage(new Uri("/Assets/favSelected.png", UriKind.RelativeOrAbsolute));
				}
				else
				{
					image.Source = new BitmapImage(new Uri(IconHelper.GetIcon("favNormal"), UriKind.RelativeOrAbsolute));
				}
			}
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "SetHeaderFavoriteStatus", "AllLocationListItem.cs", 440);
		}
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

	private void Favorite_OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is Image image)
		{
			_favoriteValue = image.Source?.ToString() ?? string.Empty;
			_isFavorite = IsFavorite(image);
			image.Source = new BitmapImage(new Uri(IconHelper.GetIcon("favHover"), UriKind.RelativeOrAbsolute));
		}
	}

	private void Favorite_OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is Image image)
		{
			if (!string.IsNullOrEmpty(_favoriteValue))
			{
				try
				{
					image.Source = new BitmapImage(new Uri(_favoriteValue, UriKind.RelativeOrAbsolute));
				}
				catch
				{
					image.Source = new BitmapImage(new Uri(IconHelper.GetIcon("favNormal"), UriKind.RelativeOrAbsolute));
				}
			}
			else
			{
				image.Source = new BitmapImage(new Uri(IconHelper.GetIcon("favNormal"), UriKind.RelativeOrAbsolute));
			}
		}
	}
}
