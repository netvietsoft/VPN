using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using NextAiVPN.Enums;

namespace NextAiVPN;

public partial class MessageWindow : Window, IComponentConnector
{
	private readonly SDKMonitor _sdk;

	public MessageWindow(SDKMonitor sdkMonitor)
	{
		InitializeComponent();
		_sdk = sdkMonitor;
	}

	public void InitWindow()
	{
		NoSubscription.GetMessageWindow(this, _sdk);
		SubscriptionExpired.GetMessageWindow(this, _sdk);
		NewUser.GetMessageWindow(this, _sdk);
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		try
		{
			if (Application.Current?.Dispatcher == null || Application.Current.Dispatcher.HasShutdownStarted || Application.Current.Dispatcher.HasShutdownFinished)
			{
				return;
			}
			e.Cancel = true;
			base.Visibility = Visibility.Collapsed;
			_sdk?.TaskBarService?.HideShowTrayIcon(Visibility.Visible);
			if (_sdk?.VpnExpandedWindow != null && _sdk.VpnExpandedWindow.IsLoaded && !_sdk.VpnExpandedWindow.AllowClose)
			{
				try
				{
					_sdk.VpnExpandedWindow.Show();
				}
				catch { }
			}
		}
		catch { }
	}

	private void MessageWindow_OnLoaded(object sender, RoutedEventArgs e)
	{
		WindowHeader.GetParent(this);
	}
}
