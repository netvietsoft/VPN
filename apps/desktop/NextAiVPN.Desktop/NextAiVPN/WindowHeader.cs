using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using NextAiVPN.UI.NotifyBalloons;

namespace NextAiVPN;

public partial class WindowHeader : UserControl, IComponentConnector
{
	private Window _parentWindow = new Window();

	public WindowHeader()
	{
		InitializeComponent();
	}

	public void GetParent(Window window)
	{
		_parentWindow = window;
	}

	private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
	{
		if (_parentWindow.GetType() == typeof(SignInWindow) && _parentWindow is SignInWindow signInWindow)
		{
			signInWindow.IsHidden = true;
		}
		if (_parentWindow.GetType() == typeof(SignUpWindow) && _parentWindow is SignUpWindow signUpWindow)
		{
			signUpWindow.IsHidden = true;
		}
		SystemCommands.MinimizeWindow(_parentWindow);
	}

	private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
	{
		SystemCommands.MaximizeWindow(_parentWindow);
	}

	private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
	{
		SystemCommands.RestoreWindow(_parentWindow);
	}

	private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
	{
		if (_parentWindow.GetType() == typeof(VPNWindowExpanded) && _parentWindow is VPNWindowExpanded vPNWindowExpanded)
		{
			try
			{
				vPNWindowExpanded.SdkObject?.TaskBarService?.TaskbarIcon?.ShowCustomBalloon(GetBalloonForHidingWindow(), PopupAnimation.Slide, 4000);
			}
			catch { }
			vPNWindowExpanded.Hide();
			vPNWindowExpanded.WindowState = WindowState.Minimized;
			vPNWindowExpanded.WasClosed = true;
			if (vPNWindowExpanded.SdkObject?.TaskBarService?.TaskbarIcon != null)
			{
				vPNWindowExpanded.SdkObject.TaskBarService.TaskbarIcon.Visibility = Visibility.Visible;
			}
			e.Handled = true;
			return;
		}
		if (_parentWindow.GetType() == typeof(MainWindow) && _parentWindow is MainWindow mainWindow)
		{
			mainWindow.IsHidden = true;
			mainWindow.Hide();
			mainWindow.WindowState = WindowState.Minimized;
			try
			{
				mainWindow.GetSdk()?.TaskBarService?.TaskbarIcon?.ShowCustomBalloon(GetBalloonForHidingWindow(), PopupAnimation.Slide, 4000);
			}
			catch { }
			e.Handled = true;
			return;
		}
		if (_parentWindow.GetType() == typeof(SubscriptionExpiredWindow))
		{
			Application.Current.Shutdown();
			return;
		}
		SystemCommands.CloseWindow(_parentWindow);
	}

	public void SetConnectedStatus(bool isConnected, string ip = "")
	{
		UpdateStatus("Protected", ip, isConnected ? "connected" : "disconnected");
	}

	public void SetConnectingStatus()
	{
		UpdateStatus("Connecting", null, "connecting");
	}

	public void UpdateStatus(string status, string ip = null, string state = "connected")
	{
		base.Dispatcher.Invoke(() =>
		{
			if (state == "connected")
			{
				StatusDot.Fill = (System.Windows.Media.Brush)FindResource("BrushEmerald500");
				StatusPillText.Text = !string.IsNullOrEmpty(ip) ? $"Protected - {ip}" : $"Protected - {status}";
			}
			else if (state == "connecting")
			{
				StatusDot.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(217, 119, 6));
				StatusPillText.Text = "Connecting to Secure Gateway...";
			}
			else
			{
				StatusDot.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 38, 38));
				StatusPillText.Text = "Not Connected (Offline)";
			}
		});
	}

	private void NotificationBellBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (_parentWindow is VPNWindowExpanded vpnWindow)
		{
			vpnWindow.ExpandedSideMenu?.SetMenuOption(Enums.SideMenuOption.Notifications);
		}
	}

	private void UserAvatarPill_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (_parentWindow is VPNWindowExpanded vpnWindow)
		{
			vpnWindow.ExpandedSideMenu?.SetMenuOption(Enums.SideMenuOption.Account);
		}
	}

	private static NotifyBalloon GetBalloonForHidingWindow()
	{
		return new NotifyBalloon(new NotifyBalloonViewModel
		{
			Header = "NextAiVPN still running",
			Message = "Note that although you are closing the application, it will still be running and can be accessed through the system menu.",
			ImageSource = "/Assets/Notification-hiding.png"
		});
	}
}
