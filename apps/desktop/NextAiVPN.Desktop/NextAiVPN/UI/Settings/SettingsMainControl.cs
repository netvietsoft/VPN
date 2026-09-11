using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.Settings;

public partial class SettingsMainControl : UserControl, IComponentConnector
{
	private int _selectedSettingsOption;

	public SettingsMainControl()
	{
		InitializeComponent();
		base.Loaded += OnStyleLoaded;
		base.Unloaded += OnStyleUnloaded;
		SelectGeneralOption();
	}

	private void StyleChanged(NextAiVPN.Services.Persistence.Style arg1, NextAiVPN.Services.Persistence.Style arg2)
	{
		switch (_selectedSettingsOption)
		{
		case 0:
			SelectGeneralOption();
			break;
		case 1:
			SelectAdvancedOption();
			break;
		}
	}

	private void OnStyleLoaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged += StyleChanged;
	}

	private void OnStyleUnloaded(object sender, RoutedEventArgs e)
	{
		GlobalEvents.StyleChanged -= StyleChanged;
	}

	private void GeneralSettingsBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_selectedSettingsOption = 0;
		SelectGeneralOption();
	}

	private void SelectGeneralOption()
	{
		SetElementButtonStyle(GetSelectedElementModesButtonForegroundBrush(StyleModeDefiner.DefineAppStyle()), GetSelectedElementModesButtonForegroundBrush(StyleModeDefiner.DefineAppStyle()), GeneralSettingsBorder, GeneralTextBlock);
		SetElementButtonStyle(GetDefaultElementModesButtonForegroundBrush(StyleModeDefiner.DefineAppStyle()), GetDefaultElementModesButtonBorderBrush(StyleModeDefiner.DefineAppStyle()), AdvancedSettingsBorder, AdvancedTextBlock);
	}

	private void AdvancedSettingsBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_selectedSettingsOption = 1;
		SelectAdvancedOption();
	}

	private void SelectAdvancedOption()
	{
		SetElementButtonStyle(GetSelectedElementModesButtonForegroundBrush(StyleModeDefiner.DefineAppStyle()), GetSelectedElementModesButtonForegroundBrush(StyleModeDefiner.DefineAppStyle()), AdvancedSettingsBorder, AdvancedTextBlock);
		SetElementButtonStyle(GetDefaultElementModesButtonForegroundBrush(StyleModeDefiner.DefineAppStyle()), GetDefaultElementModesButtonBorderBrush(StyleModeDefiner.DefineAppStyle()), GeneralSettingsBorder, GeneralTextBlock);
	}

	private void SetElementButtonStyle(SolidColorBrush foregroundBrush, SolidColorBrush borderBrush, Border border, TextBlock textBlock)
	{
		border.BorderBrush = borderBrush;
		textBlock.Foreground = foregroundBrush;
	}

	private SolidColorBrush GetSelectedElementModesButtonForegroundBrush(NextAiVPN.Services.Persistence.Style mode)
	{
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush((mode == NextAiVPN.Services.Persistence.Style.Dark) ? "#10B981" : "#059669");
	}

	private SolidColorBrush GetDefaultElementModesButtonForegroundBrush(NextAiVPN.Services.Persistence.Style mode)
	{
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush((mode == NextAiVPN.Services.Persistence.Style.Dark) ? "#E2E8F0" : "#475569");
	}

	private SolidColorBrush GetDefaultElementModesButtonBorderBrush(NextAiVPN.Services.Persistence.Style mode)
	{
		return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
	}
}
