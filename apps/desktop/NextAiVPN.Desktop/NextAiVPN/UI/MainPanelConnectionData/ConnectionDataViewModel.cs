using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.MessageBoxWindows;

namespace NextAiVPN.UI.MainPanelConnectionData;

internal class ConnectionDataViewModel : ViewModelBase
{
	private readonly SDKMonitor _sdk;

	private Visibility _controlVisibility = Visibility.Collapsed;

	private Visibility _shuffleIpVisibility;

	private string _ipAddress = "--.--.--.--";

	private string _ipAddressInfoImageSource = IconHelper.GetIcon("InfoGray");

	private string _networkUsageDownloadMb = "0 MB";

	private string _networkUsageUploadMb = "0 MB";

	private string _protocol;

	public Visibility ControlVisibility
	{
		get
		{
			return _controlVisibility;
		}
		set
		{
			if (_controlVisibility != value)
			{
				_controlVisibility = value;
				OnPropertyChanged("ControlVisibility");
			}
		}
	}

	public Visibility ShuffleIpVisibility
	{
		get
		{
			return _shuffleIpVisibility;
		}
		set
		{
			if (_shuffleIpVisibility != value)
			{
				_shuffleIpVisibility = value;
				OnPropertyChanged("ShuffleIpVisibility");
			}
		}
	}

	public string IpAddress
	{
		get
		{
			return _ipAddress;
		}
		set
		{
			if (_ipAddress != value)
			{
				_ipAddress = value;
				OnPropertyChanged("IpAddress");
			}
		}
	}

	public string IpAddressInfoImageSource
	{
		get
		{
			return _ipAddressInfoImageSource;
		}
		set
		{
			if (_ipAddressInfoImageSource != value)
			{
				_ipAddressInfoImageSource = value;
				OnPropertyChanged("IpAddressInfoImageSource");
			}
		}
	}

	public string NetworkUsageDownloadMb
	{
		get
		{
			return _networkUsageDownloadMb;
		}
		set
		{
			if (_networkUsageDownloadMb != value)
			{
				_networkUsageDownloadMb = value;
				OnPropertyChanged("NetworkUsageDownloadMb");
			}
		}
	}

	public string NetworkUsageUploadMb
	{
		get
		{
			return _networkUsageUploadMb;
		}
		set
		{
			if (_networkUsageUploadMb != value)
			{
				_networkUsageUploadMb = value;
				OnPropertyChanged("NetworkUsageUploadMb");
			}
		}
	}

	public string Protocol
	{
		get
		{
			if (_protocol != null)
			{
				return _protocol.ToUpper();
			}
			return _protocol;
		}
		set
		{
			if (_protocol != value)
			{
				_protocol = value;
				OnPropertyChanged("Protocol");
			}
		}
	}

	public ICommand ShuffleIpClickCommand { get; set; }

	public ICommand IpAddressInfoClickCommand { get; set; }

	public ConnectionDataViewModel(SDKMonitor sdk)
	{
		_sdk = sdk;
		ShuffleIpClickCommand = new ActionCommand(ShuffleIpClickCommandExecute);
		IpAddressInfoClickCommand = new ActionCommand(IpAddressInfoClickCommandExecute);
	}

	private void IpAddressInfoClickCommandExecute()
	{
		_sdk.VpnExpandedWindow.Mainpanel.IpAddressRotationDisclaimer.ViewModel.ControlVisibility = Visibility.Visible;
	}

	private async void ShuffleIpClickCommandExecute()
	{
		MessageBoxWindow messageBoxWindow = new MessageBoxWindow(new MessageBoxWindowViewModel
		{
			Header = "Shuffle your IP address?",
			Description = "Your VPN will briefly disconnect while switching to a new IP.",
			CancelButtonText = "Cancel",
			OkButtonText = "Change IP"
		});
		messageBoxWindow.ShowDialog();
		if (messageBoxWindow.DialogResult)
		{
			await _sdk.ShuffleIp();
		}
	}
}
