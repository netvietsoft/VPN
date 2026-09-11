using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.Errors.ConnectionError;

public class ConnectionErrorControlViewModel : ViewModelBase
{
	private readonly SDKMonitor _sdkMonitor;

	private Visibility _controlVisibility = Visibility.Collapsed;

	private Visibility _descriptionPrivateModeVisibility = Visibility.Collapsed;

	private Visibility _descriptionStreamingModeVisibility = Visibility.Collapsed;

	private SolidColorBrush _mainGridBackGroundColor;

	public Visibility ControlVisibility
	{
		get
		{
			return _controlVisibility;
		}
		set
		{
			if (value != _controlVisibility)
			{
				_controlVisibility = value;
				OnPropertyChanged("ControlVisibility");
			}
		}
	}

	public Visibility DescriptionPrivateModeVisibility
	{
		get
		{
			return _descriptionPrivateModeVisibility;
		}
		set
		{
			if (value != _descriptionPrivateModeVisibility)
			{
				_descriptionPrivateModeVisibility = value;
				OnPropertyChanged("DescriptionPrivateModeVisibility");
			}
		}
	}

	public Visibility DescriptionStreamingModeVisibility
	{
		get
		{
			return _descriptionStreamingModeVisibility;
		}
		set
		{
			if (value != _descriptionStreamingModeVisibility)
			{
				_descriptionStreamingModeVisibility = value;
				OnPropertyChanged("DescriptionStreamingModeVisibility");
			}
		}
	}

	public SolidColorBrush MainGridBackGroundColor
	{
		get
		{
			return _mainGridBackGroundColor;
		}
		set
		{
			if (_mainGridBackGroundColor != value)
			{
				_mainGridBackGroundColor = value;
				OnPropertyChanged("MainGridBackGroundColor");
			}
		}
	}

	public ICommand CustomerSupportTextBlockMouseDownCommand { get; private set; }

	public ICommand MainGridMouseDownCommand { get; private set; }

	public ConnectionErrorControlViewModel(SDKMonitor sdkMonitor)
	{
		_sdkMonitor = sdkMonitor;
		CustomerSupportTextBlockMouseDownCommand = new ActionCommand(CustomerSupportTextBlockMouseDownCommandExecute);
		MainGridMouseDownCommand = new ActionCommand(MainGridMouseDownCommandExecute);
		MainGridBackGroundColor = GetGridBackgroundBrush(StyleModeDefiner.DefineAppStyle());
		VpnModeChangeEvent.OnVpnModeChanged = (Action<VpnType>)Delegate.Combine(VpnModeChangeEvent.OnVpnModeChanged, new Action<VpnType>(OnVpnModeChanged));
		GlobalEvents.StyleChanged += StyleChanged;
	}

	private void StyleChanged(NextAiVPN.Services.Persistence.Style arg1, NextAiVPN.Services.Persistence.Style arg2)
	{
		MainGridBackGroundColor = GetGridBackgroundBrush(arg1);
	}

	private SolidColorBrush GetGridBackgroundBrush(NextAiVPN.Services.Persistence.Style mode)
	{
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush((mode == NextAiVPN.Services.Persistence.Style.Dark) ? "#00FFFFFF" : "#F55B5B");
	}

	private void OnVpnModeChanged(VpnType obj)
	{
		switch (obj)
		{
		case VpnType.NextAiVPN:
			DescriptionPrivateModeVisibility = Visibility.Visible;
			DescriptionStreamingModeVisibility = Visibility.Collapsed;
			break;
		case VpnType.Streaming:
			DescriptionStreamingModeVisibility = Visibility.Visible;
			DescriptionPrivateModeVisibility = Visibility.Collapsed;
			break;
		default:
			throw new ArgumentOutOfRangeException("obj", obj, null);
		}
	}

	private void MainGridMouseDownCommandExecute()
	{
		_sdkMonitor.VpnExpandedWindow.Mainpanel.HideFavoriteControlsIfVisible();
	}

	private void CustomerSupportTextBlockMouseDownCommandExecute()
	{
		_sdkMonitor.BrowserLinksOpener.OpenBrowserLink(8);
	}
}
