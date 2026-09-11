using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;
using NextAiVPN.Streaming;
using NextAiVPN.UI.SplitTunneling;

namespace NextAiVPN.UI.Settings;

internal class ExpandedStreamingSettingsControlViewModel : ViewModelBase
{
	private readonly StreamingSplitTunnelingDomainsWindow _splitTunnelingDomainsWindow;

	private readonly IDisconnectMessageBoxHelper _disconnectMessageBoxHelper;

	private readonly NextAiVPN.Streaming.ISplitTunnelingRepository _splitTunnelingRepository;

	private readonly ToggleSwitch _splitTunnelingToggle;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private bool _isSplitTunnelingEnabled;

	private Visibility _manageButtonVisibility = Visibility.Hidden;

	private bool _isManageButtonActive = true;

	public string Title => "Split Tunneling";

	public string Description => "Route specific websites through streaming VPN servers. ";

	public string SplitTunnelingTextButtonText => "Manage";

	public bool IsSplitTunnelingEnabled
	{
		get
		{
			return _isSplitTunnelingEnabled;
		}
		set
		{
			if (_isSplitTunnelingEnabled != value)
			{
				_isSplitTunnelingEnabled = value;
				OnPropertyChanged("IsSplitTunnelingEnabled");
			}
		}
	}

	public Visibility ManageButtonVisibility
	{
		get
		{
			return _manageButtonVisibility;
		}
		set
		{
			if (_manageButtonVisibility != value)
			{
				_manageButtonVisibility = value;
				OnPropertyChanged("ManageButtonVisibility");
			}
		}
	}

	public bool IsManageButtonActive
	{
		get
		{
			return _isManageButtonActive;
		}
		set
		{
			if (_isManageButtonActive != value)
			{
				_isManageButtonActive = value;
				OnPropertyChanged("IsManageButtonActive");
			}
		}
	}

	public ICommand OpenSplitTunnelingSettings => new ActionCommand((Action)delegate
	{
		if (IsManageButtonActive && ManageButtonVisibility == Visibility.Visible)
		{
			ShowTunnelingDomainsWindowDialog();
		}
	});

	public ICommand SplitTunnelingToggleMouseDown => new RelayCommand(OnSplitTunnelingToggleMouseDown);

	public ICommand PreviewMouseWheelCommand => new RelayCommand(OnPreviewMouseWheel);

	public ExpandedStreamingSettingsControlViewModel(StreamingSplitTunnelingDomainsWindow splitTunnelingDomainsWindow, IDisconnectMessageBoxHelper disconnectMessageBoxHelper, NextAiVPN.Streaming.ISplitTunnelingRepository splitTunnelingRepository, ToggleSwitch splitTunnelingToggle, IAppSettingsHelper appSettingsHelper)
	{
		_splitTunnelingDomainsWindow = splitTunnelingDomainsWindow;
		_disconnectMessageBoxHelper = disconnectMessageBoxHelper;
		_splitTunnelingRepository = splitTunnelingRepository;
		_splitTunnelingToggle = splitTunnelingToggle;
		_appSettingsHelper = appSettingsHelper;
		RestoreSettings();
		GlobalEvents.StyleChanged += StyleChanged;
		GlobalEvents.LoggedOut += GlobalEventsOnLoggedOut;
	}

	private void GlobalEventsOnLoggedOut()
	{
		SetDefaults();
	}

	private void SetDefaults()
	{
		ManageButtonVisibility = Visibility.Hidden;
	}

	private void RestoreSettings()
	{
		SetManageButtonVisibility();
	}

	private void StyleChanged(NextAiVPN.Services.Persistence.Style arg1, NextAiVPN.Services.Persistence.Style arg2)
	{
	}

	private void SetManageButtonVisibility()
	{
		ManageButtonVisibility = ((!_appSettingsHelper.GetValue("IsStreamingSplitTunnelingEnabled").Equals("1")) ? Visibility.Hidden : Visibility.Visible);
	}

	private void OnSplitTunnelingToggleMouseDown(object parameter)
	{
		if (!(parameter is ToggleSwitch toggleSwitch))
		{
			return;
		}
		if (_disconnectMessageBoxHelper.ShowDisconnectMessageIfConnected(isStreaming: true))
		{
			toggleSwitch.Toggled1 = _appSettingsHelper.GetValue("IsStreamingSplitTunnelingEnabled").Equals("1");
			toggleSwitch.Dot_InitializeState();
			return;
		}
		_appSettingsHelper.SetValue("IsStreamingSplitTunnelingEnabled", toggleSwitch.Toggled1 ? "1" : "0");
		toggleSwitch.Dot_InitializeState();
		SetManageButtonVisibility();
		if (toggleSwitch.Toggled1)
		{
			ShowTunnelingDomainsWindowDialog();
		}
	}

	private void ShowTunnelingDomainsWindowDialog()
	{
		_splitTunnelingDomainsWindow.ShowDialog();
		Task.Run(async delegate
		{
			await UpdateSplitTunnelingToggleIfNeededAsync();
		});
	}

	private async Task UpdateSplitTunnelingToggleIfNeededAsync()
	{
		if (!((await _splitTunnelingRepository.GetItems())?.Any() ?? false) && _splitTunnelingToggle != null)
		{
			if (Application.Current.Dispatcher.CheckAccess())
			{
				UncheckSplitTunnelingToggle();
			}
			else
			{
				Application.Current.Dispatcher.BeginInvoke(new Action(UncheckSplitTunnelingToggle));
			}
		}
	}

	private void UncheckSplitTunnelingToggle()
	{
		_appSettingsHelper.SetValue("IsStreamingSplitTunnelingEnabled", "0");
		_splitTunnelingToggle.Toggled1 = false;
		_splitTunnelingToggle.Dot_InitializeState();
		ManageButtonVisibility = Visibility.Hidden;
	}

	private static void OnPreviewMouseWheel(object parameter)
	{
		if (parameter is MouseWheelEventArgs { Source: ScrollViewer source } e)
		{
			source.ScrollToVerticalOffset(source.VerticalOffset - (double)e.Delta);
			e.Handled = true;
		}
	}
}
