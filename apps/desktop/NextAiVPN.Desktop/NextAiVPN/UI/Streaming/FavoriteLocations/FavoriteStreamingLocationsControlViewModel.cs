using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Services.Persistence;
using NextAiVPN.Streaming.Entities;

namespace NextAiVPN.UI.Streaming.FavoriteLocations;

public class FavoriteStreamingLocationsControlViewModel : ViewModelBase
{
	private readonly SDKMonitor _sdkManager;

	private ObservableCollection<StreamingLocation> _locations = new ObservableCollection<StreamingLocation>();

	private Visibility _userControlVisibility = Visibility.Collapsed;

	public ObservableCollection<StreamingLocation> Locations
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

	public Visibility UserControlVisibility
	{
		get
		{
			return _userControlVisibility;
		}
		set
		{
			if (value != _userControlVisibility)
			{
				_userControlVisibility = value;
				OnPropertyChanged("UserControlVisibility");
			}
		}
	}

	public ICommand SelectedFavoritesListItemClickedCommand { get; set; }

	public FavoriteStreamingLocationsControlViewModel(SDKMonitor sdkManager)
	{
		_sdkManager = sdkManager;
		SelectedFavoritesListItemClickedCommand = new ActionCommand(SelectedFavoritesListItemClickedCommandExecute);
		UpdateLocations();
	}

	public void UpdateLocations()
	{
		Locations = new ObservableCollection<StreamingLocation>(_sdkManager.StreamingSdk.Locations.Where((StreamingLocation x) => x.Id != _sdkManager.StreamingLocation.Id));
	}

	private async void SelectedFavoritesListItemClickedCommandExecute(object obj)
	{
		if (obj is IStreamingLocation loc)
		{
			if (_sdkManager.StreamingSdk.IsConnected)
			{
				await _sdkManager.DisconnectVPN();
			}
			UserControlVisibility = Visibility.Collapsed;
			_sdkManager.StreamingLocation = loc;
			_sdkManager.ConnectedVpnMode = VpnType.Streaming;
			await _sdkManager.ConnectToVPN(VpnType.Streaming);
		}
	}
}
