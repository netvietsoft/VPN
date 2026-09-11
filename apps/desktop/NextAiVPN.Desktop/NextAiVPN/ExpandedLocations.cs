using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.Streaming.InfoWindow;

namespace NextAiVPN;

public partial class ExpandedLocations : UserControl, INotifyPropertyChanged, IComponentConnector
{
	private SDKMonitor _sdkObject;

	private ICollectionView _collectionView;

	private readonly object _locker = new object();

	public StreamingInfoWindow StreamingInformationWindow;

	public event PropertyChangedEventHandler PropertyChanged;

	public ExpandedLocations()
	{
		InitializeComponent();
		base.Loaded += OnStyleLoaded;
		base.Unloaded += OnStyleUnloaded;
	}

	public void GetSdkObject(SDKMonitor sdk)
	{
		_sdkObject = sdk;
		_collectionView = CollectionViewSource.GetDefaultView(_sdkObject.NextAiVpnSdkManager.Locations);
		_collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Country"));
		_collectionView.SortDescriptions.Add(new SortDescription("Country", ListSortDirection.Ascending));
		_collectionView.SortDescriptions.Add(new SortDescription("City", ListSortDirection.Ascending));
	}

	private void MainWindowStyleChanged(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style sysStyle)
	{
		try
		{
			switch (_sdkObject.ConnectedVpnMode)
			{
			case VpnType.NextAiVPN:
				SetPrivateModeTab();
				switch (_sdkObject.ConnectionStatus?.ToLowerInvariant() ?? "disconnected")
				{
				case "connected":
				case "disconnected":
				case "error":
				case "connecting":
				default:
					break;
				}
				break;
			case VpnType.Streaming:
				SetStreamingModeTab();
				if (_sdkObject.StreamingLocation != null)
				{
					switch (_sdkObject.ConnectionStatus?.ToLowerInvariant() ?? "disconnected")
					{
					case "connected":
						_sdkObject.VpnExpandedWindow?.ExpandedLocations?.StreamingLocations?.SetCountryFlagConnected(_sdkObject.StreamingLocation.Country);
						break;
					case "disconnected":
						_sdkObject.VpnExpandedWindow?.ExpandedLocations?.StreamingLocations?.SetCountryFlag(_sdkObject.StreamingLocation.Country, _sdkObject.StreamingLocation.CountryCode);
						break;
					case "connecting":
						_sdkObject.VpnExpandedWindow?.ExpandedLocations?.StreamingLocations?.SetCountryFlagSpinnerAnimation(_sdkObject.StreamingLocation.Country);
						break;
					case "error":
					default:
						break;
					}
				}
				break;
			default:
				break;
			}
		}
		catch { }
	}

	private void OnStyleLoaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged += MainWindowStyleChanged;
	}

	private void OnStyleUnloaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged -= MainWindowStyleChanged;
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

	public DependencyObject FindChild(DependencyObject o, Type childType)
	{
		DependencyObject result = null;
		if (o != null)
		{
			int childrenCount = VisualTreeHelper.GetChildrenCount(o);
			for (int i = 0; i < childrenCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(o, i);
				if (child.GetType() != childType)
				{
					result = FindChild(child, childType);
					continue;
				}
				result = child;
				break;
			}
		}
		return result;
	}

	public static Visual GetDescendantByType(Visual element, Type type)
	{
		if (element == null)
		{
			return null;
		}
		if (element.GetType() == type)
		{
			return element;
		}
		Visual visual = null;
		if (element is FrameworkElement)
		{
			(element as FrameworkElement).ApplyTemplate();
		}
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
		{
			visual = GetDescendantByType(VisualTreeHelper.GetChild(element, i) as Visual, type);
			if (visual != null)
			{
				break;
			}
		}
		return visual;
	}

	public void SetDisablerRectangleVisibility(Visibility visibility)
	{
		base.Dispatcher.Invoke(delegate
		{
			DisablerRectangle.Visibility = visibility;
		});
	}

	public void EnableControl()
	{
		base.Dispatcher.Invoke(delegate
		{
			MainGrid.Opacity = 1.0;
			SetDisablerRectangleVisibility(Visibility.Collapsed);
		});
	}

	public void DisableControl()
	{
		base.Dispatcher.Invoke(delegate
		{
			MainGrid.Opacity = 0.4;
			SetDisablerRectangleVisibility(Visibility.Visible);
		});
	}

	private void PrivateModeBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!_sdkObject.StreamingSdk.IsConnected)
		{
			SetVpnMode(VpnType.NextAiVPN);
		}
	}

	private void StreamingModeBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!_sdkObject.NextAiVpnSdkManager.IsConnecting && !_sdkObject.NextAiVpnSdkManager.IsConnected && !_sdkObject.StreamingSdk.IsConnected)
		{
			SetVpnMode(VpnType.Streaming);
		}
	}

	public void SetVpnMode(VpnType vpnType, bool skipValidation = false)
	{
		lock (_locker)
		{
			if (vpnType == VpnType.Streaming && !skipValidation)
			{
				_sdkObject.ValidateVpnModes(showStreamingMessageBox: true);
			}
			_sdkObject.SetVpnMode(vpnType);
			SelectVpnMode(vpnType);
			SetLocationListsVisibility(vpnType);
			SetFavoriteLocation(vpnType);
			RefreshLocationList(vpnType);
			SetShuffleIpControlVisibility(vpnType);
			_sdkObject.ConnectedVpnMode = vpnType;
		}
	}

	private void SetShuffleIpControlVisibility(VpnType vpnType)
	{
		try
		{
			switch (vpnType)
			{
			case VpnType.NextAiVPN:
				_sdkObject.VpnExpandedWindow.Mainpanel.ConnectionDataViewModel.ShuffleIpVisibility = Visibility.Visible;
				break;
			case VpnType.Streaming:
				_sdkObject.VpnExpandedWindow.Mainpanel.ConnectionDataViewModel.ShuffleIpVisibility = Visibility.Collapsed;
				break;
			default:
				throw new ArgumentOutOfRangeException("vpnType", vpnType, null);
			}
		}
		catch (Exception exception)
		{
			Utils.Logger.Error(exception, "SetShuffleIpControlVisibility", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\ExpandedLocations.xaml.cs", 314);
		}
	}

	private void RefreshLocationList(VpnType vpnType)
	{
		try
		{
			switch (vpnType)
			{
			case VpnType.NextAiVPN:
				if (_sdkObject.NextAiVpnSdkManager.IsConnected)
				{
					AllLocationsControl.SetCountryFlagImageStatusAfterSearching();
				}
				break;
			case VpnType.Streaming:
				StreamingLocations.StreamingLocationsViewModel.Locations = new ObservableCollection<IStreamingLocation>(_sdkObject.StreamingSdk.Locations);
				break;
			default:
				throw new ArgumentOutOfRangeException("vpnType", vpnType, null);
			}
		}
		catch (Exception ex)
		{
			Utils.Logger.Error(ex.Message, "RefreshLocationList", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\ExpandedLocations.xaml.cs", 340);
		}
	}

	private void SetFavoriteLocation(VpnType vpnType)
	{
		try
		{
			switch (vpnType)
			{
			case VpnType.NextAiVPN:
				if (_sdkObject.NextAiVpnLocation != null)
				{
					if (_sdkObject.NextAiVpnSdkManager.IsConnected && _sdkObject.NextAiVpnSdkManager.ActiveConnectionInformation != null)
					{
						_sdkObject.SetLocation(_sdkObject.NextAiVpnSdkManager.ActiveConnectionInformation.Location);
						_sdkObject.SetFlagToLocation(_sdkObject.NextAiVpnSdkManager.ActiveConnectionInformation.Location.CountryCode);
					}
					else
					{
						_sdkObject.SetLocation(_sdkObject.NextAiVpnLocation);
						_sdkObject.SetFlagToLocation(_sdkObject.NextAiVpnLocation.CountryCode);
					}
				}
				break;
			case VpnType.Streaming:
				if (_sdkObject.StreamingSdk.Locations.Count > 0)
				{
					_sdkObject.SetStreamingLocation(_sdkObject.StreamingLocation);
					_sdkObject.SetFlagToLocation(_sdkObject.StreamingLocation.CountryCode);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException("vpnType", vpnType, null);
			}
		}
		catch (Exception ex)
		{
			Utils.Logger.Error(ex.Message, "SetFavoriteLocation", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\ExpandedLocations.xaml.cs", 382);
		}
	}

	private void SetLocationListsVisibility(VpnType vpnType)
	{
		switch (vpnType)
		{
		case VpnType.NextAiVPN:
			AllLocationsGrid.Visibility = Visibility.Visible;
			StreamingLocations.Visibility = Visibility.Collapsed;
			FavoriteLocationsTabGrid.Visibility = Visibility.Collapsed;
			break;
		case VpnType.Streaming:
			StreamingLocations.Visibility = Visibility.Visible;
			AllLocationsGrid.Visibility = Visibility.Collapsed;
			FavoriteLocationsTabGrid.Visibility = Visibility.Collapsed;
			break;
		default:
			throw new ArgumentOutOfRangeException("vpnType", vpnType, null);
		}
	}

	private void SelectVpnMode(VpnType vpnType)
	{
		switch (vpnType)
		{
		case VpnType.NextAiVPN:
			SetPrivateModeTab();
			break;
		case VpnType.Streaming:
			SetStreamingModeTab();
			break;
		default:
			throw new ArgumentOutOfRangeException("vpnType", vpnType, null);
		}
	}

	private void SetStreamingModeTab()
	{
		SetActivePillTab(StreamingModeBorder, StreamingModeTextBlock, (PrivateModeBorder, PrivateModeTextBlock), (FavoriteLocationsBorder, FavoriteLocationsTextBlock));
	}

	private void SetPrivateModeTab()
	{
		SetActivePillTab(PrivateModeBorder, PrivateModeTextBlock, (StreamingModeBorder, StreamingModeTextBlock), (FavoriteLocationsBorder, FavoriteLocationsTextBlock));
	}

	private void SetActivePillTab(Border activeBorder, TextBlock activeText, params (Border border, TextBlock text)[] inactiveTabs)
	{
		if (activeBorder != null)
		{
			activeBorder.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#059669"));
			activeBorder.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#059669"));
			activeBorder.BorderThickness = new Thickness(1);
		}
		if (activeText != null)
		{
			activeText.Foreground = Brushes.White;
			activeText.FontWeight = FontWeights.Bold;
		}

		if (inactiveTabs != null)
		{
			foreach (var (inactiveBorder, inactiveText) in inactiveTabs)
			{
				if (inactiveBorder != null)
				{
					inactiveBorder.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8FAFC"));
					inactiveBorder.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E2E8F0"));
					inactiveBorder.BorderThickness = new Thickness(1);
				}
				if (inactiveText != null)
				{
					inactiveText.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#334155"));
					inactiveText.FontWeight = FontWeights.SemiBold;
				}
			}
		}
	}

	private void StreamingInfo_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!_sdkObject.NextAiVpnSdkManager.IsConnecting)
		{
			StreamingInformationWindow.Owner = _sdkObject.VpnExpandedWindow;
			StreamingInformationWindow?.ShowDialog();
		}
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	private void FavoriteLocations_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!_sdkObject.StreamingSdk.IsConnected)
		{
			SetFavoriteLocationTabHeaderText();
			if (_sdkObject.ConnectedVpnMode != VpnType.NextAiVPN)
			{
				SetVpnMode(VpnType.NextAiVPN);
			}
			ShowFavoriteList();
			SetFavoriteLocationsTab();
		}
	}

	private void ShowFavoriteList()
	{
		FavoriteLocationsTabGrid.Visibility = Visibility.Visible;
		StreamingLocations.Visibility = Visibility.Collapsed;
		AllLocationsGrid.Visibility = Visibility.Collapsed;
	}

	private void SetFavoriteLocationsTab()
	{
		SetActivePillTab(FavoriteLocationsBorder, FavoriteLocationsTextBlock, (PrivateModeBorder, PrivateModeTextBlock), (StreamingModeBorder, StreamingModeTextBlock));
	}

	public void SetFavoriteLocationTabHeaderText()
	{
		FavoriteLocationsTabControl.FetchFavoriteLocations();
		int favoriteLocationCount = FavoriteLocationsTabControl.FavoriteLocationCount;
		if (favoriteLocationCount <= 0)
		{
			FavoriteLocationsTextBlock.Text = "Favorites";
			return;
		}
		FavoriteLocationsTextBlock.Text = $"Favorites ({favoriteLocationCount})";
	}
}
