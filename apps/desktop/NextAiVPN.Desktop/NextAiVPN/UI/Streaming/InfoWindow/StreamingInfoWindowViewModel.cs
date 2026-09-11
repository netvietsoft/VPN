using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.Streaming.InfoWindow;

public class StreamingInfoWindowViewModel : ViewModelBase
{
	private readonly SDKMonitor _sdkMonitor;

	private Visibility _windowVisibility = Visibility.Collapsed;

	public Visibility WindowVisibility
	{
		get
		{
			return _windowVisibility;
		}
		set
		{
			if (value != _windowVisibility)
			{
				_windowVisibility = value;
				OnPropertyChanged("WindowVisibility");
			}
		}
	}

	public ICommand CloseCommand { get; set; }

	public ICommand ActivateCommand { get; set; }

	public StreamingInfoWindowViewModel(SDKMonitor sdkMonitor)
	{
		_sdkMonitor = sdkMonitor;
		CloseCommand = new ActionCommand(CloseCommandExecute);
		ActivateCommand = new ActionCommand(ActivateCommandExecute);
	}

	public void OnClosing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		Close();
	}

	private void ActivateCommandExecute()
	{
		if (!_sdkMonitor.NextAiVpnSdkManager.IsConnected && !_sdkMonitor.StreamingSdk.IsConnected)
		{
			_sdkMonitor.VpnExpandedWindow.ExpandedLocations.SetVpnMode(VpnType.Streaming);
		}
		Close();
	}

	private void CloseCommandExecute()
	{
		Close();
	}

	private void Close()
	{
		WindowVisibility = Visibility.Collapsed;
	}
}
