using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN;

public partial class ToggleSwitch : UserControl, IComponentConnector
{
	private readonly Thickness _leftSide = new Thickness(0.0, 15.0, 93.0, 16.0);

	private readonly Thickness _rightSide = new Thickness(0.0, 15.0, 19.0, 16.0);

	private bool _toggled;

	public bool Toggled1
	{
		get
		{
			return _toggled;
		}
		set
		{
			_toggled = value;
			Dot_InitializeState();
		}
	}

	public ToggleSwitch()
	{
		_toggled = false;
		InitializeComponent();
	}

	public void ToggleAnimate(Thickness position)
	{
		ThicknessAnimation animation = new ThicknessAnimation
		{
			Duration = TimeSpan.FromSeconds(0.5),
			To = position,
			EasingFunction = new QuarticEase()
		};
		Dot.BeginAnimation(FrameworkElement.MarginProperty, animation);
	}

	public void Dot_InitializeState()
	{
		if (_toggled)
		{
			ToggleSwitchOn();
		}
		else
		{
			ToggleSwitchOff();
		}
	}

	private void Dot_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
	{
		if (!_toggled)
		{
			ToggleSwitchOn();
		}
		else
		{
			ToggleSwitchOff();
		}
	}

	private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!_toggled)
		{
			ToggleSwitchOn();
		}
		else
		{
			ToggleSwitchOff();
		}
	}

	private void ToggleSwitchOn()
	{
		_toggled = true;
		ToggleAnimate(_rightSide);
		SolidColorBrush solidColorBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush(GetSwitchOnBackGroundColor());
		Back.Fill = solidColorBrush;
		StatusLabel.Content = "ON";
		StatusLabel.Foreground = solidColorBrush;
	}

	private void ToggleSwitchOff()
	{
		_toggled = false;
		ToggleAnimate(_leftSide);
		SolidColorBrush solidColorBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#94A3B8");
		Back.Fill = solidColorBrush;
		StatusLabel.Content = "OFF";
		StatusLabel.Foreground = solidColorBrush;
	}

	private static string GetSwitchOnBackGroundColor()
	{
		if (StyleModeDefiner.DefineAppStyle() != NextAiVPN.Services.Persistence.Style.Light)
		{
			return "#10B981"; // Emerald-500
		}
		return "#059669"; // Emerald-600
	}
}
