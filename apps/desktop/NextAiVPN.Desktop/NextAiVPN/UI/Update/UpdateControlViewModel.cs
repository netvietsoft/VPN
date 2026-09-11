using System;
using System.Windows;
using System.Windows.Controls;
using NextAiVPN.Common;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.Update;

public class UpdateControlViewModel : ViewModelBase
{
	private readonly IUpdateService _updateService;

	private readonly IBugsnagService _bugsnagService;

	private readonly IApiClient _apiClient;

	private readonly IAppLogger _logger;

	private VersionUpdateError _versionUpdateErrorWindow;

	private bool _isDownloading;

	private Visibility _controlVisibility = Visibility.Collapsed;

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

	public UpdateControlViewModel(IUpdateService updateService, IBugsnagService bugsnagService, IApiClient apiClient, IAppLogger logger)
	{
		_updateService = updateService;
		_bugsnagService = bugsnagService;
		_apiClient = apiClient;
		_logger = logger;
		_versionUpdateErrorWindow = new VersionUpdateError();
	}

	public async void CheckUpdates()
	{
		ControlVisibility = ((!(await _updateService.IsUpdateAvailable())) ? Visibility.Collapsed : Visibility.Visible);
	}

	public async void OnUpdateButtonClick(object sender, RoutedEventArgs e)
	{
		if (!InternetConnection.IsAvailable())
		{
			ShowErrorMessage("No network detected, please connect to internet in order to updated NextAiVPN.");
		}
		else
		{
			if (_isDownloading)
			{
				return;
			}
			try
			{
				if (sender is Button button)
				{
					Image image = ControlFinder.FindChild<Image>(button.Parent, "UpdateSpinnerImage");
					_isDownloading = true;
					image.Visibility = Visibility.Visible;
					Animation.SpinnerAnimation(animate: true, image);
					button.Visibility = Visibility.Visible;
					TextBlock textBlock = (TextBlock)button.Template.FindName("btnText", button);
					textBlock.Visibility = Visibility.Collapsed;
					int num = 0;
					object obj = default(object);
					try
					{
						_updateService.Update();
					}
					catch (Exception ex)
					{
						obj = ex;
						num = 1;
					}
					if (num == 1)
					{
						Exception ex2 = (Exception)obj;
						_isDownloading = false;
						button.Visibility = Visibility.Collapsed;
						textBlock.Visibility = Visibility.Visible;
						Animation.SpinnerAnimation(animate: false, image);
						ShowErrorMessage("There has been an error downloading the update installer, please try again.");
						string text = await _apiClient.GetLatestVersionNumber();
						_bugsnagService.Notify("[Update Now] - There was an error downloading the file version: " + text + " - Error: " + ex2.Message);
					}
				}
			}
			catch (Exception ex3)
			{
				_logger?.Error(ex3.Message, "OnUpdateButtonClick", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\Update\\UpdateControlViewModel.cs", 88);
			}
		}
	}

	private void ShowErrorMessage(string massage)
	{
		if (_versionUpdateErrorWindow != null && _versionUpdateErrorWindow.IsVisible)
		{
			_versionUpdateErrorWindow.Focus();
			return;
		}
		_versionUpdateErrorWindow = new VersionUpdateError();
		_versionUpdateErrorWindow.GetErrorMessage(massage);
		_versionUpdateErrorWindow.ShowDialog();
	}
}
