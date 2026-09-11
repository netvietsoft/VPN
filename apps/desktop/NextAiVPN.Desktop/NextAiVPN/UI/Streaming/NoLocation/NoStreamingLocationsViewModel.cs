using System;
using System.Windows;
using System.Windows.Controls;
using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.Streaming.NoLocation;

internal class NoStreamingLocationsViewModel : ViewModelBase
{
	private readonly SDKMonitor _sdkMonitor;

	private readonly IAppLogger _logger;

	private int _triesCount;

	public NoStreamingLocationsViewModel(SDKMonitor sdkMonitor, IAppLogger logger)
	{
		_sdkMonitor = sdkMonitor;
		_logger = logger;
	}

	public async void OnUpdateButtonClick(object sender, RoutedEventArgs e)
	{
		if (!InternetConnection.IsAvailable())
		{
			return;
		}
		try
		{
			if (!(sender is Button button))
			{
				return;
			}
			Image spinnerImage = ControlFinder.FindChild<Image>(button.Parent, "SpinnerImage");
			spinnerImage.Visibility = Visibility.Visible;
			Animation.SpinnerAnimation(animate: true, spinnerImage);
			button.Content = string.Empty;
			try
			{
				if (_triesCount > 1)
				{
					_sdkMonitor.VpnExpandedWindow.Mainpanel.NotificationControlViewModel.UserControlVisibility = Visibility.Visible;
				}
				_triesCount++;
				await _sdkMonitor.GetStreamingLocations();
				SetButtonDefault(button, spinnerImage, "no streaming locations");
			}
			catch (Exception ex)
			{
				SetButtonDefault(button, spinnerImage, ex.Message);
			}
		}
		catch (Exception ex2)
		{
			_logger?.Error(ex2.Message, "OnUpdateButtonClick", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Streaming\\NoLocation\\NoStreamingLocationsViewModel.cs", 54);
		}
	}

	private void SetButtonDefault(Button button, Image spinnerImage, string message)
	{
		button.Content = "Try again";
		spinnerImage.Visibility = Visibility.Collapsed;
		Animation.SpinnerAnimation(animate: false, spinnerImage);
		_logger?.Error(message, "SetButtonDefault", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Streaming\\NoLocation\\NoStreamingLocationsViewModel.cs", 63);
	}
}
