using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DynamicData;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;
using NextAiVPN.Streaming.Entities;

namespace NextAiVPN.UI.Streaming.Locations;

public class StreamingLocationsViewModel : ViewModelBase
{
	private readonly SDKMonitor _sdkManager;

	private const string _searchPlaceholder = "Search";

	private string _searchString = "";

	private ObservableCollection<IStreamingLocation> _locations = new ObservableCollection<IStreamingLocation>();

	private bool _pingOrderedToggle;

	private bool _countryOrderedToggle;

	private BitmapImage _locationsBitmapImageSource = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));

	private BitmapImage _pingsBitmapImageSource = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));

	public string SearchString
	{
		get
		{
			return _searchString;
		}
		set
		{
			_searchString = value;
			if (_searchString.Equals("Search"))
			{
				OnPropertyChanged("SearchString");
				return;
			}
			Locations = ((!string.IsNullOrEmpty(_searchString)) ? new ObservableCollection<IStreamingLocation>(_sdkManager.StreamingSdk.Locations.Where((StreamingLocation x) => x.SearchName.ToLower().Contains(_searchString.ToLower()))) : new ObservableCollection<IStreamingLocation>(_sdkManager.StreamingSdk.Locations));
			OnPropertyChanged("SearchString");
		}
	}

	public ObservableCollection<IStreamingLocation> Locations
	{
		get
		{
			return _locations;
		}
		set
		{
			if (value != _locations)
			{
				_locations = value;
				OnPropertyChanged("Locations");
			}
		}
	}

	public ICommand SortListCommand { get; set; }

	public ICommand SelectedItemChangedCommand { get; set; }

	public ICommand ConnectToCommand { get; set; }

	public BitmapImage LocationsBitmapImageSource
	{
		get
		{
			return _locationsBitmapImageSource;
		}
		set
		{
			if (value != _locationsBitmapImageSource)
			{
				_locationsBitmapImageSource = value;
				OnPropertyChanged("LocationsBitmapImageSource");
			}
		}
	}

	public BitmapImage PingsBitmapImageSource
	{
		get
		{
			return _pingsBitmapImageSource;
		}
		set
		{
			if (value != _pingsBitmapImageSource)
			{
				_pingsBitmapImageSource = value;
				OnPropertyChanged("PingsBitmapImageSource");
			}
		}
	}

	public StreamingLocationsViewModel(SDKMonitor sdkManager)
	{
		_sdkManager = sdkManager;
		SortListCommand = new ActionCommand(SortListCommandExecute);
		SelectedItemChangedCommand = new ActionCommand(SelectedItemChangedCommandExecute);
		ConnectToCommand = new ActionCommand(ConnectToCommandExecute);
	}

	public async Task SetLocations()
	{
		await _sdkManager.VpnExpandedWindow.Dispatcher.Invoke((Func<Task>)async delegate
		{
			Locations.Clear();
			ShowHideNoLocationControl(_sdkManager.StreamingSdk.Locations.Count != 0);
			if (_sdkManager.StreamingSdk.Locations.Count > 0)
			{
				Locations.AddRange(_sdkManager.StreamingSdk.Locations.OrderBy((StreamingLocation x) => x.Country));
				foreach (IStreamingLocation location in Locations)
				{
					if (!location.PingMs.HasValue)
					{
						await location.Ping();
					}
				}
			}
		});
	}

	public void ShowHideNoLocationControl(bool neededToShow)
	{
		if (neededToShow)
		{
			_sdkManager.VpnExpandedWindow.Mainpanel.EnableConnectButton();
			ShowLocationList();
			ShowFavoriteControl();
			_sdkManager.VpnExpandedWindow.Mainpanel.NotificationControlViewModel.UserControlVisibility = Visibility.Hidden;
		}
		else
		{
			_sdkManager.VpnExpandedWindow.Mainpanel.DisableConnectButton();
			HideLocationList();
			HideFavoriteControl();
		}
	}

	public void HideFavoriteControl()
	{
		_sdkManager.VpnExpandedWindow.Mainpanel.ConnectBtnPanel.Visibility = Visibility.Collapsed;
	}

	public void ShowFavoriteControl()
	{
		_sdkManager.VpnExpandedWindow.Mainpanel.ConnectBtnPanel.Visibility = Visibility.Visible;
		if (_sdkManager.ConnectedVpnMode == VpnType.Streaming && _sdkManager.StreamingSdk.Locations.Count > 0)
		{
			_sdkManager.SetStreamingLocation(_sdkManager.StreamingLocation);
			_sdkManager.SetFlagToLocation(_sdkManager.StreamingLocation.CountryCode);
		}
	}

	private void HideLocationList()
	{
		_sdkManager.VpnExpandedWindow.ExpandedLocations.StreamingLocations.NoStreamingLocationsControl.Visibility = Visibility.Visible;
		_sdkManager.VpnExpandedWindow.ExpandedLocations.StreamingLocations.LocationsList.Visibility = Visibility.Collapsed;
	}

	private void ShowLocationList()
	{
		_sdkManager.VpnExpandedWindow.ExpandedLocations.StreamingLocations.NoStreamingLocationsControl.Visibility = Visibility.Collapsed;
		_sdkManager.VpnExpandedWindow.ExpandedLocations.StreamingLocations.LocationsList.Visibility = Visibility.Visible;
	}

	private async void ConnectToCommandExecute(object obj)
	{
		if (obj is IStreamingLocation loc)
		{
			if (_sdkManager.StreamingSdk.IsConnected)
			{
				await _sdkManager.DisconnectVPN();
			}
			_sdkManager.StreamingLocation = loc;
			_sdkManager.ConnectedVpnMode = VpnType.Streaming;
			await _sdkManager.ConnectToVPN(VpnType.Streaming);
		}
	}

	private void SelectedItemChangedCommandExecute(object obj)
	{
	}

	private void SortListCommandExecute(object obj)
	{
		SortLocations(obj.ToString());
	}

	private void SortLocations(string sortingFilter)
	{
		if (sortingFilter.ToLower() == "ping")
		{
			OrderByPing();
			SetSortingArrowDirectionImage("ping", _pingOrderedToggle);
		}
		else
		{
			OrderByCountryName();
			SetSortingArrowDirectionImage("country", _countryOrderedToggle);
		}
	}

	private void SetSortingArrowDirectionImage(string sortingName, bool isAscending)
	{
		string text = sortingName.ToLower();
		if (!(text == "country"))
		{
			if (text == "ping")
			{
				PingsBitmapImageSource = (isAscending ? new BitmapImage(new Uri("/Assets/carretup.png", UriKind.Relative)) : new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative)));
				LocationsBitmapImageSource = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
			}
		}
		else
		{
			LocationsBitmapImageSource = (isAscending ? new BitmapImage(new Uri("/Assets/carretup.png", UriKind.Relative)) : new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative)));
			PingsBitmapImageSource = new BitmapImage(new Uri("/Assets/carretdown.png", UriKind.Relative));
		}
	}

	private void OrderByCountryName()
	{
		if (_countryOrderedToggle)
		{
			_countryOrderedToggle = false;
			Locations = new ObservableCollection<IStreamingLocation>(Locations.OrderBy((IStreamingLocation x) => x.Country));
		}
		else
		{
			_countryOrderedToggle = true;
			Locations = new ObservableCollection<IStreamingLocation>(Locations.OrderByDescending((IStreamingLocation x) => x.Country));
		}
	}

	private void OrderByPing()
	{
		if (_pingOrderedToggle)
		{
			_pingOrderedToggle = false;
			Locations = new ObservableCollection<IStreamingLocation>(Locations.OrderBy((IStreamingLocation x) => x.PingMs));
		}
		else
		{
			_pingOrderedToggle = true;
			Locations = new ObservableCollection<IStreamingLocation>(Locations.OrderByDescending((IStreamingLocation x) => x.PingMs));
		}
	}

	public void SearchTextChangedFunc(string obj)
	{
		SearchString = obj;
	}

	public void PingSortFunc(LocationsOrder arg1, bool arg2)
	{
		SortLocations(arg1.ToString());
	}

	public void CountrySortFunc(LocationsOrder arg1, bool arg2)
	{
		SortLocations(arg1.ToString());
	}
}
