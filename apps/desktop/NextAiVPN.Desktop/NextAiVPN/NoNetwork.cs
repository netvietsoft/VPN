using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using NextAiVPN.Enums;

namespace NextAiVPN;

public partial class NoNetwork : Window, IComponentConnector
{
	private readonly SDKMonitor _sdkManager;

	private readonly VPNWindowExpanded _windowExpanded;

	private bool _signOut;

	public NoNetwork(SDKMonitor sdk)
	{
		_sdkManager = sdk;
		_windowExpanded = sdk.VpnExpandedWindow;
		InitializeComponent();
		SetupNotifyIcon();
	}

	public void SetupNotifyIcon()
	{
		_sdkManager.TaskBarService.TaskbarIcon.Icon = ResourceFile.icontray_nonetwork;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		WindowHeader.GetParent(this);
	}

	private async void Window_Closing(object sender, CancelEventArgs e)
	{
		if (_signOut)
		{
			Utils.MixpanelNotification.SendNotification("TrayMenu - Sign Out");
			await _sdkManager.DisconnectVPN();
			_sdkManager.TaskBarService.SignOut = true;
		}
		else
		{
			e.Cancel = true;
			Hide();
			base.ShowInTaskbar = false;
		}
	}

	private void ExpandCollapse_MouseEnter(object sender, MouseEventArgs e)
	{
		ExpandCollapse.Opacity = 1.0;
	}

	private void ExpandCollapse_MouseLeave(object sender, MouseEventArgs e)
	{
		ExpandCollapse.Opacity = 1.0;
	}

	private void ExpandCollapse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		ShowExpanded();
		Hide();
	}

	private void ShowExpanded()
	{
		if (_windowExpanded.SdkObject.ButtonEnable || _windowExpanded.SdkObject.LocationsRefreshing)
		{
			_windowExpanded.ExpandedSideMenu.SetMenuOption(SideMenuOption.Location);
		}
		else
		{
			_windowExpanded.ExpandedSideMenu.LocationsSideMenu.IsEnabled = false;
			_windowExpanded.ExpandedSideMenu.SetMenuOption(SideMenuOption.Settings);
		}
		_windowExpanded.Show();
	}
}
