using System;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.ThemeAppearance;

public class ThemeAppearanceControlViewModel : ViewModelBase
{
	private readonly SDKMonitor _sdkMonitor;

	private SolidColorBrush _lightColorBorderBrush;

	private SolidColorBrush _darkColorBorderBrush;

	public SolidColorBrush LightColorBorderBrush
	{
		get
		{
			return _lightColorBorderBrush;
		}
		set
		{
			if (value != _lightColorBorderBrush)
			{
				_lightColorBorderBrush = value;
				OnPropertyChanged("LightColorBorderBrush");
			}
		}
	}

	public SolidColorBrush DarkColorBorderBrush
	{
		get
		{
			return _darkColorBorderBrush;
		}
		set
		{
			if (value != _darkColorBorderBrush)
			{
				_darkColorBorderBrush = value;
				OnPropertyChanged("DarkColorBorderBrush");
			}
		}
	}

	public ICommand LightModeButtonClickCommand { get; private set; }

	public ICommand DarkModeButtonClickCommand { get; private set; }

	public ICommand LoadedCommand { get; private set; }

	public ThemeAppearanceControlViewModel(SDKMonitor sdkMonitor)
	{
		_sdkMonitor = sdkMonitor;
		LightModeButtonClickCommand = new ActionCommand(LightModeButtonClickCommandExecute);
		DarkModeButtonClickCommand = new ActionCommand(DarkModeButtonClickCommandExecute);
		LoadedCommand = new ActionCommand(LoadedCommandExecute);
	}

	private void LoadedCommandExecute()
	{
		SetSwitcherBorder(StyleModeDefiner.DefineAppStyle());
	}

	private void DarkModeButtonClickCommandExecute()
	{
		SwitchStyle(Style.Dark);
	}

	private void LightModeButtonClickCommandExecute()
	{
		SwitchStyle(Style.Light);
	}

	public void SwitchStyle(Style style)
	{
		_sdkMonitor.VpnExpandedWindow.PreferencesService.Set("ThemeAppearance", style.ToString());
		_sdkMonitor.VpnEntities.MainWindow.SetApplicationStyles(style, Style.Skip);
		_sdkMonitor.VpnExpandedWindow.Mainpanel.StyleChange(style);
		_sdkMonitor.VpnExpandedWindow.Mainpanel.PrivateFavoriteLocationControl.StyleChange();
		_sdkMonitor.VpnExpandedWindow.AdvanceViewLogs.StyleChange();
		GlobalEvents.RaiseStyleChanged(style, Style.Skip);
		SetSwitcherBorder(style);
	}

	private void SetSwitcherBorder(Style style)
	{
		switch (style)
		{
		case Style.Dark:
			DarkColorBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#10B981");
			LightColorBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00FFFFFF");
			break;
		case Style.Light:
			LightColorBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#059669");
			DarkColorBorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#00FFFFFF");
			break;
		default:
			throw new ArgumentOutOfRangeException("style", style, null);
		case Style.Skip:
			break;
		}
	}
}
