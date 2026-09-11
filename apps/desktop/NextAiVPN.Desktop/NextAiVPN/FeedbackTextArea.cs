using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI;

namespace NextAiVPN;

public partial class FeedbackTextArea : UserControl, IViewModel, INotifyPropertyChanged, IComponentConnector
{
	private VPNWindowExpanded _expandedWindow;

	private readonly DoubleAnimation _doubleAnimation = new DoubleAnimation();

	private readonly RotateTransform _rotateTransform = new RotateTransform();

	private const string SendFeedbackBtnTitle = "Send feedback";

	private const string Placeholder = "Write your message here...";

	private bool _needToSendDiagnosticFile = true;

	public bool NeedToSendDiagnosticFile
	{
		get
		{
			return _needToSendDiagnosticFile;
		}
		set
		{
			if (_needToSendDiagnosticFile != value)
			{
				_needToSendDiagnosticFile = value;
				OnPropertyChanged("NeedToSendDiagnosticFile");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public FeedbackTextArea()
	{
		InitializeComponent();
		base.DataContext = this;
	}

	public void GetExpandedWindow(VPNWindowExpanded expanded)
	{
		_expandedWindow = expanded;
	}

	public bool IsKillSwitchOnDisconnected()
	{
		if (!_expandedWindow.SdkObject.NextAiVpnSdkManager.IsConnected)
		{
			return Utils.AppSettingsHelper.GetValue("KillSwitch").Equals("1");
		}
		return false;
	}

	private async void SendFeedbackBtn_Click(object sender, RoutedEventArgs e)
	{
		if (!IsKillSwitchOnDisconnected())
		{
			AnimateSpinner(animate: true);
			sendFeedbackBtn.IsEnabled = false;
			string message = ((FeedbackText.Text == "Write your message here...") ? "" : FeedbackText.Text);
			string text = await Utils.FeedbackService.SendFeedbackNotification(_expandedWindow.Score.ToString(), message, NeedToSendDiagnosticFile);
			if (text.Equals("1"))
			{
				Utils.Logger?.Information($"Feedback successfully sent. Score: {_expandedWindow.Score}", "SendFeedbackBtn_Click", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\FeedbackTextArea.xaml.cs", 72);
				ResetTextArea();
				Utils.FeedbackService.SaveLastFeedbackClosed(reset: true);
			}
			else if (text.Equals("-1") && (await Utils.FeedbackService.SendFeedbackNotification(_expandedWindow.Score.ToString(), message, NeedToSendDiagnosticFile)).Equals("1"))
			{
				ResetTextArea();
			}
			sendFeedbackBtn.IsEnabled = true;
			AnimateSpinner(animate: false);
		}
		else
		{
			sendFeedbackBtn.IsEnabled = true;
			AnimateSpinner(animate: false);
		}
	}

	private void FeedbackText_GotFocus(object sender, RoutedEventArgs e)
	{
		if (FeedbackText.Text == "Write your message here...")
		{
			FeedbackText.Text = "";
		}
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light)
		{
			BorderTextBox.BorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#059669");
			FeedbackText.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#000000");
		}
		else
		{
			FeedbackText.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
		}
	}

	private void FeedbackText_LostFocus(object sender, RoutedEventArgs e)
	{
		if (FeedbackText.Text.Length == 0)
		{
			FeedbackText.Text = "Write your message here...";
		}
		if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light)
		{
			BorderTextBox.BorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#848487");
			FeedbackText.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#C0C0C0");
		}
		else
		{
			FeedbackText.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
		}
	}

	private void FeedbackText_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (FeedbackText.Text != "Write your message here...")
		{
			string text = FeedbackText.Text;
			characterCount.Text = text.Length.ToString();
			if (StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light)
			{
				BorderTextBox.BorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#059669");
				return;
			}
			BorderTextBox.BorderBrush = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
			FeedbackText.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
		}
	}

	private void BtnCross_MouseDown(object sender, MouseButtonEventArgs e)
	{
		btnCross.Focus();
		Utils.FeedbackService.SaveLastFeedbackClosed(reset: false);
	}

	public void ResetTextArea()
	{
		FeedbackText.Text = "Write your message here...";
		characterCount.Text = "0";
		FeedbackText.Foreground = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#BFBFBF");
	}

	public void AnimateSpinner(bool animate)
	{
		_doubleAnimation.From = 0.0;
		_doubleAnimation.To = 360.0;
		_doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1L));
		_doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
		sendSpinner.RenderTransform = _rotateTransform;
		sendSpinner.RenderTransformOrigin = new Point(0.5, 0.5);
		if (animate)
		{
			sendSpinner.Visibility = Visibility.Visible;
			sendFeedbackBtn.Content = "";
			_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, _doubleAnimation);
		}
		else
		{
			sendFeedbackBtn.Content = "Send feedback";
			sendSpinner.Visibility = Visibility.Collapsed;
			_rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
		}
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(bool)e.OldValue && (bool)e.NewValue && FeedbackText.Text != "Write your message here...")
		{
			FeedbackText.SelectionStart = FeedbackText.Text.Length;
			FeedbackText.SelectionLength = 0;
			FeedbackText.CaretIndex = FeedbackText.Text.Length;
			Keyboard.Focus(FeedbackText);
			base.Dispatcher.Invoke(delegate
			{
				FocusManager.SetFocusedElement(this, FeedbackText);
			});
			FeedbackText.RaiseEvent(new RoutedEventArgs(UIElement.GotFocusEvent));
		}
		if ((bool)e.OldValue && !(bool)e.NewValue && FeedbackText.Text != "Write your message here...")
		{
			base.Dispatcher.Invoke(delegate
			{
				FocusManager.SetFocusedElement(this, btnCross);
			});
		}
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}
