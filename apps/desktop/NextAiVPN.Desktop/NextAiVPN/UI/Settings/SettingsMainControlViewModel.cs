using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.UI.Update;

namespace NextAiVPN.UI.Settings;

public class SettingsMainControlViewModel : ViewModelBase
{
	private readonly UpdateControlViewModel _updateControlViewModel;

	private double _mainGridOpacity = 1.0;

	private bool _mainGridIsEnabled = true;

	private Visibility _disablerRectangleVisibility = Visibility.Collapsed;

	private Visibility _controlVisibility = Visibility.Collapsed;

	private Visibility _advancedSettingsHeaderBorderVisibility;

	private Visibility _advancedSettingsControlVisibility = Visibility.Collapsed;

	private Visibility _generalSettingsControlVisibility = Visibility.Collapsed;

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

	public Visibility AdvancedSettingsHeaderBorderVisibility
	{
		get
		{
			return _advancedSettingsHeaderBorderVisibility;
		}
		set
		{
			if (_advancedSettingsHeaderBorderVisibility != value)
			{
				_advancedSettingsHeaderBorderVisibility = value;
				OnPropertyChanged("AdvancedSettingsHeaderBorderVisibility");
			}
		}
	}

	public Visibility AdvancedSettingsControlVisibility
	{
		get
		{
			return _advancedSettingsControlVisibility;
		}
		set
		{
			if (_advancedSettingsControlVisibility != value)
			{
				_advancedSettingsControlVisibility = value;
				OnPropertyChanged("AdvancedSettingsControlVisibility");
			}
		}
	}

	public Visibility GeneralSettingsControlVisibility
	{
		get
		{
			return _generalSettingsControlVisibility;
		}
		set
		{
			if (_generalSettingsControlVisibility != value)
			{
				_generalSettingsControlVisibility = value;
				OnPropertyChanged("GeneralSettingsControlVisibility");
			}
		}
	}

	public ICommand GeneralBorderClickCommand { get; private set; }

	public ICommand AdvancedBorderClickCommand { get; private set; }

	public SettingsMainControlViewModel(UpdateControlViewModel viewModel)
	{
		_updateControlViewModel = viewModel;
		GeneralBorderClickCommand = new ActionCommand(GeneralBorderClickCommandExecute);
		AdvancedBorderClickCommand = new ActionCommand(AdvancedBorderClickCommandExecute);
		GeneralBorderClickCommand.Execute(null);
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

	public void CheckUpdates()
	{
		_updateControlViewModel.CheckUpdates();
	}

	private void AdvancedBorderClickCommandExecute(object obj)
	{
		GeneralSettingsControlVisibility = Visibility.Collapsed;
		AdvancedSettingsControlVisibility = Visibility.Visible;
	}

	private void GeneralBorderClickCommandExecute(object obj)
	{
		GeneralSettingsControlVisibility = Visibility.Visible;
		AdvancedSettingsControlVisibility = Visibility.Collapsed;
	}
}
