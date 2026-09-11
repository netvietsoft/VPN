using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NextAiVPN.Services;

namespace NextAiVPN.UI.Preferences;

internal class PreferencesControlViewModel : ViewModelBase
{
	private readonly IStartUpService _startUpService;

	private readonly IVpnConfigurationSaver _vpnConfigurationSaver;

	private readonly IAnalyticsService _analyticsService;

	private double _mainGridOpacity = 1.0;

	private bool _mainGridIsEnabled = true;

	private Visibility _disablerRectangleVisibility = Visibility.Collapsed;

	public double MainGridOpacity
	{
		get
		{
			return _mainGridOpacity;
		}
		set
		{
			if (_mainGridOpacity != value)
			{
				_mainGridOpacity = value;
				OnPropertyChanged("MainGridOpacity");
			}
		}
	}

	public bool MainGridIsEnabled
	{
		get
		{
			return _mainGridIsEnabled;
		}
		set
		{
			if (_mainGridIsEnabled != value)
			{
				_mainGridIsEnabled = value;
				OnPropertyChanged("MainGridIsEnabled");
			}
		}
	}

	public Visibility DisablerRectangleVisibility
	{
		get
		{
			return _disablerRectangleVisibility;
		}
		set
		{
			if (_disablerRectangleVisibility != value)
			{
				_disablerRectangleVisibility = value;
				OnPropertyChanged("DisablerRectangleVisibility");
			}
		}
	}

	public PreferencesControlViewModel(IStartUpService startUpService, IVpnConfigurationSaver vpnConfigurationSaver, IAnalyticsService analyticsService)
	{
		_startUpService = startUpService;
		_vpnConfigurationSaver = vpnConfigurationSaver;
		_analyticsService = analyticsService;
	}

	public void StartUpToggle_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		ToggleSwitch startUpToggle = sender as ToggleSwitch;
		if (startUpToggle == null)
		{
			return;
		}
		Task.Run(delegate
		{
			_vpnConfigurationSaver.SaveStartupConfiguration(startUpToggle.Toggled1);
			if (startUpToggle.Toggled1)
			{
				_startUpService.AddStartUp();
				_analyticsService.SendNotification("Settings - Startup_ON");
			}
			else
			{
				_startUpService.RemoveStartUp();
				_analyticsService.SendNotification("Settings - Startup_OFF");
			}
		});
	}

	public void EnableControl()
	{
		MainGridOpacity = 1.0;
		MainGridIsEnabled = true;
		DisablerRectangleVisibility = Visibility.Collapsed;
	}

	public void DisableControl()
	{
		MainGridOpacity = 0.4;
		MainGridIsEnabled = false;
		DisablerRectangleVisibility = Visibility.Visible;
	}
}
