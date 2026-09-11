using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NextAiVPN.Entities;
using NextAiVPN.Services.Persistence;
using Serilog.Events;

namespace NextAiVPN;

public partial class ExpandedLogsView : UserControl, IComponentConnector
{
	private SDKMonitor _sdkObject;

	private bool _logsLoaded;

	private ObservableCollection<LogInformation> _logInfoList = new ObservableCollection<LogInformation>();

	private string _logsEnabled;

	private bool _isSqlLogs = true;

	public ExpandedLogsView()
	{
		InitializeComponent();
		base.Loaded += OnStyleLoaded;
		base.Unloaded += OnStyleUnloaded;
		Utils.Logger.LogSubject.Subscribe(OnLogEvent);
		logsText.DataContext = _logInfoList;
		_logsEnabled = Utils.AppSettingsHelper.GetValue("LocalEnabled");
	}

	public void LoadConfiguration()
	{
		if (_logsEnabled == "1")
		{
			LogsSwitch.Toggled1 = true;
		}
		else
		{
			LogsSwitch.Toggled1 = false;
			logsScroll.Visibility = Visibility.Collapsed;
		}
		if (!HasLogs())
		{
			SetDeleteLogsButtonImageSource("trashEmpty", isEnabled: false);
		}
		else
		{
			SetDeleteLogsButtonImageSource("trashLogs", isEnabled: true);
		}
		StyleChange();
	}

	public bool HasLogs()
	{
		return _logInfoList.Count > 0;
	}

	public void StyleChange()
	{
		LogsSwitch.Dot_InitializeState();
	}

	public void GetSdk(SDKMonitor sdk)
	{
		_sdkObject = sdk;
	}

	private void MainWindowStyleChanged(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style sysStyle)
	{
		LogsSwitch.Dot_InitializeState();
	}

	private void OnStyleLoaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged += MainWindowStyleChanged;
	}

	private void OnStyleUnloaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged -= MainWindowStyleChanged;
	}

	private async void OnLogEvent(LogEvent e)
	{
		try
		{
			if (!(_logsEnabled == "1"))
			{
				return;
			}
			base.Dispatcher.Invoke(delegate
			{
				if (nodatalabel.IsVisible)
				{
					nodatalabel.Visibility = Visibility.Collapsed;
				}
				if (deleteLogsImage.Source.ToString().Contains("trashEmpty"))
				{
					nodatalabel.Visibility = Visibility.Collapsed;
				}
				LogInformation logInformation = new LogInformation
				{
					LogDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture),
					LogInfo = e.RenderMessage()
				};
				_logInfoList.Add(logInformation);
				if (_isSqlLogs)
				{
					SqliteDataAccess.Insert(logInformation);
				}
				UpdateScrollBar(logsText);
				if (!DeleteLogsGrid.IsEnabled)
				{
					SetDeleteLogsButtonImageSource("trashLogs", isEnabled: true);
				}
				if (_logInfoList.Count > 0 && deleteLogsImage.Source.ToString().Contains("trashEmpty"))
				{
					SetDeleteLogsButtonImageSource("trashLogs", isEnabled: true);
				}
			});
		}
		catch (Exception ex)
		{
			_isSqlLogs = false;
			LogError($"{"ExpandedLogsView"}.{"OnLogEvent"}()\n{ex.Message}");
		}
	}

	private static void LogError(string message)
	{
		Utils.Logger.Error(message, "LogError", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\ExpandedLogsView.xaml.cs", 149);
	}

	private void SetDeleteLogsButtonImageSource(string icoName, bool isEnabled)
	{
		deleteLogsImage.Source = new BitmapImage(new Uri(IconHelper.GetIcon(icoName), UriKind.Relative));
		DeleteLogsGrid.IsEnabled = isEnabled;
	}

	private void UpdateScrollBar(ListBox listBox)
	{
		if (_sdkObject.VpnExpandedWindow != null && listBox != null && _sdkObject.VpnExpandedWindow.AdvanceViewLogs.IsVisible)
		{
			((ScrollViewer)VisualTreeHelper.GetChild((Border)VisualTreeHelper.GetChild(listBox, 0), 0)).ScrollToBottom();
		}
	}

	public async void ReloadLogs()
	{
		try
		{
			base.Dispatcher.Invoke(delegate
			{
				_logInfoList.Clear();
				_logInfoList = new ObservableCollection<LogInformation>(SqliteDataAccess.GetAll());
				logsText.DataContext = _logInfoList;
			});
		}
		catch (Exception ex)
		{
			LogError($"{"ExpandedLogsView"}.{"ReloadLogs"}()\n{ex.Message}");
		}
	}

	public void ClearLogs()
	{
		deleteLogsImage.Source = new BitmapImage(new Uri(IconHelper.GetIcon("trashEmpty"), UriKind.Relative));
		_sdkObject.SetClearLogs();
		if (_logsEnabled == "1")
		{
			nodatalabel.Visibility = Visibility.Visible;
		}
		SetDeleteLogsButtonImageSource("trashEmpty", isEnabled: false);
		SqliteDataAccess.DeleteAll();
		ReloadLogs();
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		base.DataContext = _sdkObject;
	}

	private void Deletelogsgrid_MouseEnter(object sender, MouseEventArgs e)
	{
		deleteEllipse.Fill = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? "#EAEAEA" : "#3A3B40");
	}

	private void Deletelogsgrid_MouseLeave(object sender, MouseEventArgs e)
	{
		deleteEllipse.Fill = Brushes.Transparent;
	}

	private async void DeleteLogsImage_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (deleteLogsImage.Source.ToString().Contains("trashLogs.png"))
		{
			DeleteLogsMessageWindow deleteLogsMessageWindow = new DeleteLogsMessageWindow();
			deleteLogsMessageWindow.Owner = _sdkObject.VpnExpandedWindow;
			deleteLogsMessageWindow.ShowDialog();
			if (deleteLogsMessageWindow.Action)
			{
				ClearLogs();
			}
		}
	}

	private async void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!_logsLoaded)
		{
			await Task.Run((Action)ReloadLogs);
			_logsLoaded = true;
		}
		if (!LogsSwitch.Toggled1)
		{
			EnableDeleteLogsButton(condition: true);
		}
	}

	private void LogsSwitch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		Utils.VpnConfigurationSaver.SaveLogsEnabledConfiguration(LogsSwitch.Toggled1);
		if (LogsSwitch.Toggled1)
		{
			_logsEnabled = "1";
			logsScroll.Visibility = Visibility.Visible;
			EnableDeleteLogsButton(logsText.Items.Count == 0);
		}
		else
		{
			_logsEnabled = "0";
			SetDeleteLogsButtonImageSource("trashEmpty", isEnabled: false);
			logsScroll.Visibility = Visibility.Collapsed;
			nodatalabel.Visibility = Visibility.Collapsed;
		}
	}

	private void EnableDeleteLogsButton(bool condition)
	{
		if (condition)
		{
			SetDeleteLogsButtonImageSource("trashEmpty", isEnabled: false);
			nodatalabel.Visibility = Visibility.Visible;
		}
		else
		{
			SetDeleteLogsButtonImageSource("trashLogs", isEnabled: true);
			nodatalabel.Visibility = Visibility.Collapsed;
		}
	}
}
