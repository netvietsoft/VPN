using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI;

public partial class LocationsListHeader : UserControl, IComponentConnector
{
	private bool _isSearchingLocation;

	private bool _locationOrdering;

	private bool _loadOrdering;

	private bool _pingOrdering;

	private bool _favoritesOrdering;

	public Action<string> SearchTextChangedFunc;

	public Action<LocationsOrder, bool> CountrySortFunc;

	public Action<LocationsOrder, bool> LoadSortFunc;

	public Action<LocationsOrder, bool> PingSortFunc;

	public Action<LocationsOrder, bool> FavoritesSortFunc;

	public LocationsListHeader()
	{
		InitializeComponent();
		LocationSortDirection.Visibility = Visibility.Visible;
	}

	public void SetAsStreamingHeader()
	{
		LoadStackPanel.Visibility = Visibility.Collapsed;
		FavoritesStackPanel.Visibility = Visibility.Collapsed;
		PingMs.IsEnabled = true;
		Country.IsEnabled = true;
	}

	public void SetAsFavoriteHeader()
	{
		LoadStackPanel.Visibility = Visibility.Collapsed;
		FavoritesStackPanel.Visibility = Visibility.Collapsed;
		PingMs.IsEnabled = true;
		Country.IsEnabled = true;
	}

	public void EnableSortingOptions()
	{
		Load.IsEnabled = true;
		Country.IsEnabled = true;
	}

	private void SearchEllipse_OnMouseEnter(object sender, MouseEventArgs e)
	{
		SearchEllipseHover.Fill = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark) ? "#3A3B40" : "#F2F2F6");
	}

	private void SearchEllipse_OnMouseLeave(object sender, MouseEventArgs e)
	{
		SearchEllipseHover.Fill = Brushes.Transparent;
	}

	private void SearchEllipse_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		ToggleSearchControl();
	}

	private void ToggleSearchControl()
	{
		_isSearchingLocation = !_isSearchingLocation;
		if (_isSearchingLocation)
		{
			ShowSearch();
		}
		else
		{
			HideSearch();
		}
	}

	private void ShowSearch()
	{
		LocationHeaderStackPanel.Visibility = Visibility.Collapsed;
		SearchStackPanel.Visibility = Visibility.Visible;
	}

	private void SearchClose_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		HideSearch();
	}

	private void HideSearch()
	{
		LocationHeaderStackPanel.Visibility = Visibility.Visible;
		SearchStackPanel.Visibility = Visibility.Collapsed;
		_isSearchingLocation = false;
		SearchTextBlock.Text = string.Empty;
	}

	private void SearchLocations_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		ToggleSearchControl();
	}

	private void SearchTextBlock_OnTextChanged(object sender, TextChangedEventArgs e)
	{
		SearchTextChangedFunc(SearchTextBlock.Text);
	}

	private void Country_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (Country.IsEnabled && sender is TextBlock)
		{
			_locationOrdering = !_locationOrdering;
			SetSortingArrowDirectionImage(LocationsOrder.Country, _locationOrdering);
			SetSortingArrowVisibility(LocationsOrder.Country);
			CountrySortFunc(LocationsOrder.Country, !_locationOrdering);
		}
	}

	private void Load_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (Load.IsEnabled && sender is TextBlock)
		{
			_loadOrdering = !_loadOrdering;
			SetSortingArrowDirectionImage(LocationsOrder.Load, _loadOrdering);
			SetSortingArrowVisibility(LocationsOrder.Load);
			LoadSortFunc(LocationsOrder.Load, !_loadOrdering);
		}
	}

	private void PingMs_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (PingMs.IsEnabled && sender is TextBlock)
		{
			_pingOrdering = !_pingOrdering;
			SetSortingArrowDirectionImage(LocationsOrder.Ping, _pingOrdering);
			SetSortingArrowVisibility(LocationsOrder.Ping);
			PingSortFunc(LocationsOrder.Ping, !_pingOrdering);
		}
	}

	private void SetSortingArrowVisibility(LocationsOrder sortingOrder)
	{
		switch (sortingOrder)
		{
		case LocationsOrder.Country:
			LocationSortDirection.Visibility = Visibility.Visible;
			PingSortDirection.Visibility = Visibility.Collapsed;
			FavoritesSortDirection.Visibility = Visibility.Collapsed;
			LoadSortDirection.Visibility = Visibility.Collapsed;
			break;
		case LocationsOrder.Ping:
			LocationSortDirection.Visibility = Visibility.Collapsed;
			PingSortDirection.Visibility = Visibility.Visible;
			FavoritesSortDirection.Visibility = Visibility.Collapsed;
			LoadSortDirection.Visibility = Visibility.Collapsed;
			break;
		case LocationsOrder.Load:
			LocationSortDirection.Visibility = Visibility.Collapsed;
			PingSortDirection.Visibility = Visibility.Collapsed;
			FavoritesSortDirection.Visibility = Visibility.Collapsed;
			LoadSortDirection.Visibility = Visibility.Visible;
			break;
		case LocationsOrder.Favorites:
			LocationSortDirection.Visibility = Visibility.Collapsed;
			PingSortDirection.Visibility = Visibility.Collapsed;
			FavoritesSortDirection.Visibility = Visibility.Visible;
			LoadSortDirection.Visibility = Visibility.Collapsed;
			break;
		default:
			throw new ArgumentOutOfRangeException("sortingOrder", sortingOrder, null);
		}
	}

	private void SetSortingArrowDirectionImage(LocationsOrder sortingOrder, bool isAscending)
	{
		switch (sortingOrder)
		{
		case LocationsOrder.Country:
			LocationSortDirection.Source = (isAscending ? new BitmapImage(new Uri("/Assets/carretup.png", UriKind.Relative)) : new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative)));
			PingSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			LoadSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			FavoritesSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			break;
		case LocationsOrder.Ping:
			PingSortDirection.Source = (isAscending ? new BitmapImage(new Uri("/Assets/carretup.png", UriKind.Relative)) : new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative)));
			LocationSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			LoadSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			FavoritesSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			break;
		case LocationsOrder.Load:
			LoadSortDirection.Source = (isAscending ? new BitmapImage(new Uri("/Assets/carretup.png", UriKind.Relative)) : new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative)));
			LocationSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			PingSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			FavoritesSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			break;
		case LocationsOrder.Favorites:
			FavoritesSortDirection.Source = (isAscending ? new BitmapImage(new Uri("/Assets/carretup.png", UriKind.Relative)) : new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative)));
			LocationSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			LoadSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			PingSortDirection.Source = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			break;
		default:
			throw new ArgumentOutOfRangeException("sortingOrder", sortingOrder, null);
		}
	}

	private void Favorites_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (Favorites.IsEnabled && sender is TextBlock)
		{
			_favoritesOrdering = !_favoritesOrdering;
			SetSortingArrowDirectionImage(LocationsOrder.Favorites, _favoritesOrdering);
			FavoritesSortFunc(LocationsOrder.Favorites, !_favoritesOrdering);
			SetSortingArrowVisibility(LocationsOrder.Favorites);
		}
	}
}
