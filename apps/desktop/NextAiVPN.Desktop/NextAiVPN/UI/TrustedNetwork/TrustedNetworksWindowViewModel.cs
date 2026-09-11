using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.MessageBoxWindows;
using NextAiVPN.UI.NotifyBalloons;

namespace NextAiVPN.UI.TrustedNetwork;

public class TrustedNetworksWindowViewModel : ViewModelBase
{
	private TrustedNetworksWindow _trustedNetworksWindow;

	private readonly ITrustedNetworkService _trustedNetworkService;

	private readonly INetworkService _networkService;

	private readonly IBugsnagService _bugsnagService;

	private readonly IAppLogger _logger;

	private readonly VPNWindowExpanded _expandedVpnWindow;

	private string _connectedNetwork;

	private int _scrollViewHeight = 270;

	private Visibility _trustedNetworksScrollViewerVisibility;

	private Visibility _noneTrustedNetworksVisibility;

	private Visibility _currentTrustedNetworkStackPanelVisibility;

	private Visibility _noWiFiNetworkVisibility = Visibility.Hidden;

	public ObservableCollection<NextAiVPN.Entities.TrustedNetwork> TrustedNetworksCollection { get; set; } = new ObservableCollection<NextAiVPN.Entities.TrustedNetwork>();

	public string ConnectedNetwork
	{
		get
		{
			return _connectedNetwork;
		}
		set
		{
			_connectedNetwork = value;
			OnPropertyChanged("ConnectedNetwork");
		}
	}

	public int ScrollViewHeight
	{
		get
		{
			return _scrollViewHeight;
		}
		set
		{
			_scrollViewHeight = value;
			OnPropertyChanged("ScrollViewHeight");
		}
	}

	public Visibility TrustedNetworksScrollViewerVisibility
	{
		get
		{
			return _trustedNetworksScrollViewerVisibility;
		}
		set
		{
			_trustedNetworksScrollViewerVisibility = value;
			OnPropertyChanged("TrustedNetworksScrollViewerVisibility");
		}
	}

	public Visibility NoneTrustedNetworksVisibility
	{
		get
		{
			return _noneTrustedNetworksVisibility;
		}
		set
		{
			_noneTrustedNetworksVisibility = value;
			OnPropertyChanged("NoneTrustedNetworksVisibility");
		}
	}

	public Visibility CurrentTrustedNetworkStackPanelVisibility
	{
		get
		{
			return _currentTrustedNetworkStackPanelVisibility;
		}
		set
		{
			_currentTrustedNetworkStackPanelVisibility = value;
			OnPropertyChanged("CurrentTrustedNetworkStackPanelVisibility");
		}
	}

	public Visibility NoWiFiNetworkVisibility
	{
		get
		{
			return _noWiFiNetworkVisibility;
		}
		set
		{
			_noWiFiNetworkVisibility = value;
			OnPropertyChanged("NoWiFiNetworkVisibility");
		}
	}

	public ICommand AddTrustedNetworkClickCommand { get; set; }

	public ICommand RemoveTrustedNetworkClickCommand { get; set; }

	public TrustedNetworksWindowViewModel(ITrustedNetworkService trustedNetworkService, INetworkService networkService, VPNWindowExpanded expandedVpnWindow, IBugsnagService bugsnagService, IAppLogger logger)
	{
		_expandedVpnWindow = expandedVpnWindow;
		_trustedNetworkService = trustedNetworkService;
		_networkService = networkService;
		_bugsnagService = bugsnagService;
		_logger = logger;
		ConnectedNetwork = _networkService.GetConnectedNetworkName();
		AddTrustedNetworkClickCommand = new RelayCommand(AddTrustedNetworkClickCommandExecute);
		RemoveTrustedNetworkClickCommand = new RelayCommand(RemoveTrustedNetworkClickCommandExecute);
		InitializeTrustedNetworks();
	}

	public void SortTrustedNetworks()
	{
		if (_trustedNetworksWindow != null)
		{
			_trustedNetworksWindow?.Dispatcher.Invoke(delegate
			{
				TrustedNetworksCollection.Clear();
				InitializeTrustedNetworks();
			});
		}
	}

	public void AddTrustedNetwork(string networkName)
	{
		ConnectedNetwork = _networkService.GetConnectedNetworkName();
		if (!ConnectedNetwork.Equals(networkName))
		{
			TrustedNetworksCollection.Add(new NextAiVPN.Entities.TrustedNetwork
			{
				IsConnected = false,
				NetworkName = networkName
			});
		}
		else
		{
			TrustedNetworksCollection.Insert(0, new NextAiVPN.Entities.TrustedNetwork
			{
				IsConnected = true,
				NetworkName = networkName
			});
		}
		ShowTrustedNetworkPanels();
		CurrentTrustedNetworkStackPanelVisibility = Visibility.Collapsed;
	}

	public void SetParent(TrustedNetworksWindow trustedNetworksWindow)
	{
		_trustedNetworksWindow = trustedNetworksWindow;
	}

	public void ShowTrustedNetworkPanels()
	{
		_trustedNetworksWindow?.Dispatcher.Invoke(delegate
		{
			ShowCurrentTrustedNetworkPanel();
			ShowNoneTrustedNetworkPanel();
			ShowTrustedNetworkScroll();
			ShowNoWiFiNetworkPanel();
		});
	}

	public bool IsContainNetwork(string networkName)
	{
		return TrustedNetworksCollection.Any((NextAiVPN.Entities.TrustedNetwork x) => x.NetworkName.Equals(networkName));
	}

	private void InitializeTrustedNetworks()
	{
		try
		{
			List<string> list = _trustedNetworkService.GetNetworks().ToList();
			if (list.Count <= 0)
			{
				return;
			}
			foreach (string item in list)
			{
				AddTrustedNetwork(item);
			}
		}
		catch (Exception ex)
		{
			_bugsnagService.Notify("ExpandedGeneralSettings_OnLoaded - " + ex.Message);
		}
	}

	private void RemoveTrustedNetworkClickCommandExecute(object obj)
	{
		if (obj is Image image)
		{
			try
			{
				NextAiVPN.Entities.TrustedNetwork network = (NextAiVPN.Entities.TrustedNetwork)image.DataContext;
				RemoveTrustedNetwork(network);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex.Message, "RemoveTrustedNetworkClickCommandExecute", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\TrustedNetwork\\TrustedNetworksWindowViewModel.cs", 248);
			}
		}
	}

	private void AddTrustedNetworkClickCommandExecute(object obj)
	{
		if (!IsContainNetwork(ConnectedNetwork))
		{
			AddTrustedNetwork(ConnectedNetwork);
			_trustedNetworkService.Add(ConnectedNetwork);
		}
	}

	private void RemoveTrustedNetwork(NextAiVPN.Entities.TrustedNetwork network)
	{
		if (network == null || string.IsNullOrEmpty(network.NetworkName))
		{
			return;
		}
		MessageBoxWindow messageBoxWindow = new MessageBoxWindow(new MessageBoxWindowViewModel
		{
			Header = "Delete trusted network?",
			Description = "By clicking delete, you will remove this network\nfrom your trusted Wi-Fi list.",
			CancelButtonText = "Cancel",
			OkButtonText = "Delete"
		});
		messageBoxWindow.ShowDialog();
		if (messageBoxWindow.DialogResult)
		{
			if (_expandedVpnWindow.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.NetworkDetectionNotificationToggle.Toggled1 && ConnectedNetwork.Equals(network.NetworkName))
			{
				_expandedVpnWindow.SdkObject.TaskBarService.TaskbarIcon.ShowCustomBalloon(new NotifyBalloon(new NotifyBalloonViewModel
				{
					Height = 110,
					Width = 400,
					ImageSource = IconHelper.GetIcon("NewUserControlIcon"),
					Message = "Your auto-protect has been disabled",
					Header = "VPN is disconnected"
				}), PopupAnimation.Slide, 5000);
			}
			TrustedNetworksCollection.Remove(network);
			_trustedNetworkService.Remove(network.NetworkName);
			ShowTrustedNetworkPanels();
		}
	}

	private void ShowCurrentTrustedNetworkPanel()
	{
		ConnectedNetwork = _networkService.GetConnectedNetworkName();
		if (ConnectedNetwork.ToLower().Contains("NextAiVpn".ToLower()) || ConnectedNetwork.ToLower().Equals("WireGuard Tunnel".ToLower()) || string.IsNullOrEmpty(ConnectedNetwork))
		{
			CurrentTrustedNetworkStackPanelVisibility = Visibility.Collapsed;
		}
		else
		{
			CurrentTrustedNetworkStackPanelVisibility = (IsContainNetwork(ConnectedNetwork) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private void ShowNoneTrustedNetworkPanel()
	{
		NoneTrustedNetworksVisibility = ((TrustedNetworksCollection.Count != 0) ? Visibility.Collapsed : Visibility.Visible);
	}

	private void ShowNoWiFiNetworkPanel()
	{
		NoWiFiNetworkVisibility = ((!string.IsNullOrEmpty(ConnectedNetwork)) ? Visibility.Collapsed : Visibility.Visible);
	}

	private void ShowTrustedNetworkScroll()
	{
		if (TrustedNetworksCollection.Count != 0)
		{
			TrustedNetworksScrollViewerVisibility = Visibility.Visible;
			ScrollViewHeight = 340;
		}
		else
		{
			TrustedNetworksScrollViewerVisibility = Visibility.Visible;
			ScrollViewHeight = 270;
		}
	}
}
