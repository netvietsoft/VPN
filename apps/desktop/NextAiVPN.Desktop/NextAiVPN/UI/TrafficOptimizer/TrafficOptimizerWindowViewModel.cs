using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Common;
using NextAiVPN.Entities;

namespace NextAiVPN.UI.TrafficOptimizer;

public class TrafficOptimizerWindowViewModel : ViewModelBase
{
	private const int ListBixItemHeight = 18;

	private const int WindowDefaultHeight = 240;

	private int _windowHeight = 240;

	private Visibility _windowVisibility = Visibility.Collapsed;

	private Visibility _rectangleVisibility;

	private Visibility _appListVisibility;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	public ObservableCollection<TrafficOptimizerApp> AppList { get; set; } = new ObservableCollection<TrafficOptimizerApp>();

	public ICommand CloseButtonClickCommand { get; set; }

	public ICommand AddAppCommand { get; set; }

	public ICommand RemoveAppCommand { get; set; }

	public string Title { get; private set; } = "Traffic optimizer";

	public string Description { get; private set; } = "Select which apps get faster connections on VPN.";

	public int WindowHeight
	{
		get
		{
			return _windowHeight;
		}
		set
		{
			if (_windowHeight != value)
			{
				_windowHeight = value;
				OnPropertyChanged("WindowHeight");
			}
		}
	}

	public Visibility WindowVisibility
	{
		get
		{
			return _windowVisibility;
		}
		set
		{
			if (_windowVisibility != value)
			{
				_windowVisibility = value;
				LoadApps();
				SetVisibilities();
				OnPropertyChanged("WindowVisibility");
			}
		}
	}

	public Visibility RectangleVisibility
	{
		get
		{
			return _rectangleVisibility;
		}
		set
		{
			if (_rectangleVisibility != value)
			{
				_rectangleVisibility = value;
				OnPropertyChanged("RectangleVisibility");
			}
		}
	}

	public Visibility AppListVisibility
	{
		get
		{
			return _appListVisibility;
		}
		set
		{
			if (_appListVisibility != value)
			{
				_appListVisibility = value;
				OnPropertyChanged("AppListVisibility");
			}
		}
	}

	public TrafficOptimizerWindowViewModel(IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_appSettingsHelper = appSettingsHelper;
		_logger = logger;
		CloseButtonClickCommand = new ActionCommand(CloseButtonClickCommandExecute);
		AddAppCommand = new ActionCommand(AddAppCommandExecute);
		RemoveAppCommand = new ActionCommand(RemoveAppCommandExecute);
		LoadApps();
		SetVisibilities();
	}

	private void SetVisibilities()
	{
		if (AppList.Count > 0)
		{
			RectangleVisibility = Visibility.Collapsed;
			AppListVisibility = Visibility.Visible;
		}
		else
		{
			RectangleVisibility = Visibility.Visible;
			AppListVisibility = Visibility.Collapsed;
		}
	}

	private void RemoveAppCommandExecute(object obj)
	{
		if (obj is TrafficOptimizerApp app)
		{
			RemoveApp(app);
			SetVisibilities();
		}
	}

	private void AddAppCommandExecute(object obj)
	{
		AddApp();
	}

	private void AddApp()
	{
		if (AppList.Count >= 3)
		{
			return;
		}
		using (OpenFileDialog openFileDialog = new OpenFileDialog())
		{
			openFileDialog.InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory);
			openFileDialog.Filter = "Exe files (*.exe)|*.exe";
			openFileDialog.RestoreDirectory = true;
			openFileDialog.Multiselect = false;
			if (openFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
			{
				TrafficOptimizerApp app = new TrafficOptimizerApp().Parse(openFileDialog.FileName) as TrafficOptimizerApp;
				if (AppList.All((TrafficOptimizerApp x) => x.Path != app.Path))
				{
					AddApp(app);
				}
			}
		}
		SetVisibilities();
	}

	private void LoadApps()
	{
		try
		{
			AppList.Clear();
			string value = _appSettingsHelper.GetValue("TrafficOptimizerAppList");
			if (!string.IsNullOrEmpty(value))
			{
				string[] array = value.Split(';');
				foreach (string path in array)
				{
					AppList.Add(new TrafficOptimizerApp().Parse(path) as TrafficOptimizerApp);
				}
			}
			SetWindowHeight();
		}
		catch (Exception ex)
		{
			_logger?.Error(ex.Message, "LoadApps", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\TrafficOptimizer\\TrafficOptimizerWindowViewModel.cs", 235);
		}
	}

	private void AddApp(TrafficOptimizerApp app)
	{
		AppList.Add(app);
		SaveAppList();
		SetWindowHeight();
	}

	private void SaveAppList()
	{
		_appSettingsHelper.SetValue("TrafficOptimizerAppList", string.Join(";", AppList.Select((TrafficOptimizerApp x) => x.Path)));
	}

	private void RemoveApp(TrafficOptimizerApp app)
	{
		AppList.Remove(app);
		SaveAppList();
		SetWindowHeight();
	}

	private void SetWindowHeight()
	{
		switch (AppList.Count)
		{
		case 0:
			WindowHeight = 240;
			break;
		case 1:
			WindowHeight = 200;
			break;
		case 2:
		case 3:
			WindowHeight = 200 + (AppList.Count - 1) * 18;
			break;
		default:
			WindowHeight = 240;
			break;
		}
	}

	private void CloseButtonClickCommandExecute()
	{
		WindowVisibility = Visibility.Collapsed;
	}

	public void OnWindowClosing(object sender, CancelEventArgs e)
	{
		WindowVisibility = Visibility.Collapsed;
		e.Cancel = true;
	}
}
