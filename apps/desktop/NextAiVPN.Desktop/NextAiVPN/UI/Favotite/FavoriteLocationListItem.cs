using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN.UI.Favotite;

public partial class FavoriteLocationListItem : UserControl, IComponentConnector
{
	public FavoriteLocationsTab ParentControl { get; set; }

	public FavoriteLocationListItem()
	{
		InitializeComponent();
	}

	private void RemoveFavoriteLocation_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		HandleRemoveClick(sender);
	}

	private void RemoveFavoriteLocation_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		HandleRemoveClick(sender);
	}

	private void HandleRemoveClick(object sender)
	{
		if (sender is Image { DataContext: ILocation dataContext })
		{
			RemoveLocation(dataContext);
		}
		else if (sender is FrameworkElement fe && fe.DataContext is ILocation feLoc)
		{
			RemoveLocation(feLoc);
		}
		else if (base.DataContext is ILocation baseLoc)
		{
			RemoveLocation(baseLoc);
		}
	}

	/// <summary>
	/// [VI] Xóa vị trí khỏi danh sách yêu thích và cập nhật ngay giao diện liên quan
	/// [EN] Remove location from favorites and immediately update related UI
	/// </summary>
	private void RemoveLocation(ILocation location)
	{
		if (location == null) return;
		try
		{
			Utils.FavoritesService.SaveFavorites(location.CountryCode + "_" + location.Id);
			var expandedLocs = ParentControl?.SdkMonitor?.VpnExpandedWindow?.ExpandedLocations
				?? (Application.Current?.MainWindow as VPNWindowExpanded)?.ExpandedLocations;
			if (expandedLocs != null)
			{
				expandedLocs.SetFavoriteLocationTabHeaderText();
				expandedLocs.FavoriteLocationsTabControl?.FetchFavoriteLocations();
				expandedLocs.AllLocationsControl?.LocationsList?.Items?.Refresh();
			}
		}
		catch { }
	}

	/// <summary>
	/// [VI] Cập nhật hiệu ứng thị giác viền cam và nền tối khi mục yêu thích được chọn
	/// [EN] Update visual highlight (orange border + dark background) when favorite item is selected
	/// </summary>
	public void SetSelectedVisualState(bool isSelected)
	{
		try
		{
			if (LocationItemHoverBorder == null) return;
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
		catch { }
	}

	private void FavoriteLocationListItem_OnLoaded(object sender, RoutedEventArgs e)
	{
		if (base.Tag is FavoriteLocationsTab parentControl)
		{
			ParentControl = parentControl;
		}
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

	private void ExpanderHover_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		HandleRemoveClick(sender);
	}

	private void ExpanderHover_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		e.Handled = true;
		HandleRemoveClick(sender);
	}
}
