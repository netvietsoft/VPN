using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Entities;
using NextAiVPN.Services;

namespace NextAiVPN.UI.SplitTunneling;

public class SplitTunnelingAppWindowViewModel : ViewModelBase
{
	private readonly ISplitTunnelingService _splitTunnelingService;

	private int _windowHeight = 280;

	private int _scrollHeight = 147;

	private int _listBoxWidth;

	private ScrollBarVisibility _verticalScrollBarVisibility = ScrollBarVisibility.Hidden;

	private Visibility _parentWindowVisibility = Visibility.Hidden;

	private Visibility _rectangleVisibility;

	private Visibility _appListVisibility;

	private Thickness _removeAppItemMargin;

	public ObservableCollection<SplitTunnelingApp> AppList { get; set; } = new ObservableCollection<SplitTunnelingApp>();

	public SplitTunnelingMainControlViewModel SplitTunnelingMainControlViewModel { get; set; }

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

	public int ScrollHeight
	{
		get
		{
			return _scrollHeight;
		}
		set
		{
			if (_scrollHeight != value)
			{
				_scrollHeight = value;
				OnPropertyChanged("ScrollHeight");
			}
		}
	}

	public int ListBoxWidth
	{
		get
		{
			return _listBoxWidth;
		}
		set
		{
			if (_listBoxWidth != value)
			{
				_listBoxWidth = value;
				OnPropertyChanged("ListBoxWidth");
			}
		}
	}

	public ScrollBarVisibility VerticalScrollBarVisibility
	{
		get
		{
			return _verticalScrollBarVisibility;
		}
		set
		{
			if (_verticalScrollBarVisibility != value)
			{
				_verticalScrollBarVisibility = value;
				OnPropertyChanged("VerticalScrollBarVisibility");
			}
		}
	}

	public string Title { get; private set; } = "Apps";

	public string Description { get; private set; } = "Select the apps you’d like to route through";

	public Visibility ParentWindowVisibility
	{
		get
		{
			return _parentWindowVisibility;
		}
		set
		{
			if (_parentWindowVisibility != value)
			{
				_parentWindowVisibility = value;
				if (_parentWindowVisibility == Visibility.Visible)
				{
					GetAppList();
					SetVisibilities();
				}
				OnPropertyChanged("ParentWindowVisibility");
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

	public Thickness RemoveAppItemMargin
	{
		get
		{
			return _removeAppItemMargin;
		}
		set
		{
			if (_removeAppItemMargin != value)
			{
				_removeAppItemMargin = value;
				OnPropertyChanged("RemoveAppItemMargin");
			}
		}
	}

	public ICommand DoneButtonClickCommand { get; set; }

	public ICommand AddAppClickCommand { get; set; }

	public ICommand RemoveAppClickCommand { get; set; }

	public SplitTunnelingAppWindowViewModel(ISplitTunnelingService splitTunnelingService)
	{
		_splitTunnelingService = splitTunnelingService;
		DoneButtonClickCommand = new ActionCommand(DoneButtonClickCommandExecute);
		AddAppClickCommand = new ActionCommand(AddAppClickCommandExecute);
		RemoveAppClickCommand = new ActionCommand(RemoveAppClickCommandExecute);
		GetAppList();
		SetVisibilities();
	}

	private void RemoveAppClickCommandExecute(object obj)
	{
		if (obj is SplitTunnelingApp splitTunnelingApp)
		{
			if (AppList.Contains(splitTunnelingApp))
			{
				AppList.Remove(splitTunnelingApp);
			}
			_splitTunnelingService.Remove(splitTunnelingApp.Path);
			SetAppCountToMainWindow();
			SetVisibilities();
		}
	}

	private void GetAppList()
	{
		AppList.Clear();
		foreach (string item in _splitTunnelingService.GetList().ToList())
		{
			AppList.Add(new SplitTunnelingApp().Parse(item));
		}
	}

	private void SetVisibilities()
	{
		if (AppList.Count > 0)
		{
			RectangleVisibility = Visibility.Collapsed;
			AppListVisibility = Visibility.Visible;
			WindowScaling();
			ShowHideVerticalScrollBar();
		}
		else
		{
			RectangleVisibility = Visibility.Visible;
			AppListVisibility = Visibility.Collapsed;
			WindowHeight = 265;
		}
	}

	private void ShowHideVerticalScrollBar()
	{
		VerticalScrollBarVisibility = ((AppList.Count > 6) ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden);
	}

	private void WindowScaling()
	{
		RemoveAppItemMargin = new Thickness(0.0);
		ListBoxWidth = 273;
		switch (AppList.Count)
		{
		case 0:
			WindowHeight = 265;
			return;
		case 1:
			WindowHeight = 223;
			return;
		case 2:
			WindowHeight = 248;
			return;
		case 3:
			WindowHeight = 268;
			return;
		case 4:
			WindowHeight = 292;
			return;
		case 5:
			WindowHeight = 318;
			return;
		case 6:
			WindowHeight = 342;
			return;
		}
		RemoveAppItemMargin = new Thickness(-15.0, 0.0, 0.0, 0.0);
		WindowHeight = 342;
		ListBoxWidth = 257;
	}

	private void AddAppClickCommandExecute()
	{
		AddApp();
	}

	private void AddApp()
	{
		using (OpenFileDialog openFileDialog = new OpenFileDialog())
		{
			openFileDialog.InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory);
			openFileDialog.Filter = "Exe files (*.exe)|*.exe";
			openFileDialog.RestoreDirectory = true;
			openFileDialog.Multiselect = false;
			if (openFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
			{
				SplitTunnelingApp app = new SplitTunnelingApp().Parse(openFileDialog.FileName);
				if (AppList.All((SplitTunnelingApp x) => x.Path != app.Path))
				{
					AppList.Add(app);
					_splitTunnelingService.Add(openFileDialog.FileName);
					SetAppCountToMainWindow();
				}
			}
		}
		SetVisibilities();
	}

	private void SetAppCountToMainWindow()
	{
		SplitTunnelingMainControlViewModel.AppCounterControlViewModel.CountText = AppList.Count.ToString();
	}

	private void DoneButtonClickCommandExecute()
	{
		Close();
	}

	private void Close()
	{
		SetAppCountToMainWindow();
		ParentWindowVisibility = Visibility.Hidden;
	}

	public void OnWindowClosing(object sender, CancelEventArgs e)
	{
		Close();
		e.Cancel = true;
	}

	public void OnScrollPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		ScrollViewer obj = (ScrollViewer)sender;
		obj.ScrollToVerticalOffset(obj.VerticalOffset - (double)e.Delta);
		e.Handled = true;
	}
}
