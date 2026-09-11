using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using NextAiVPN.UI.ToolsContextMenu;

namespace NextAiVPN;

public partial class SubscriptionExpiredWindow : Window, IComponentConnector
{
	private readonly SDKMonitor _sdk;

	private ToolsControlViewModel _toolsControlViewModel;

	public bool CloseApp;

	public SubscriptionExpiredWindow(SDKMonitor sdkObject)
	{
		InitializeComponent();
		_sdk = sdkObject;
		ConfigureToolsControl();
	}

	private void ConfigureToolsControl()
	{
		_toolsControlViewModel = new ToolsControlViewModel(_sdk);
		ToolsControlViewModel toolsControlViewModel = _toolsControlViewModel;
		toolsControlViewModel.OnSignOutAction = (Action)Delegate.Combine(toolsControlViewModel.OnSignOutAction, new Action(OnSignOutAction));
		_toolsControlViewModel.SetVisibility(isMainScreen: false);
		ToolsControl.DataContext = _toolsControlViewModel;
		ToolsControl.SetDependencies(_sdk.VpnEntities.StyleService);
	}

	private void OnSignOutAction()
	{
		Hide();
	}

	public void SetVisibleControl(string control)
	{
		MainSubscription.Visibility = Visibility.Visible;
		CompletePurchase.Visibility = Visibility.Collapsed;
		_sdk.TaskBarService.ContextMenuHide("Quit", "Sign Out");
	}

	private void Quit()
	{
		if (base.Visibility == Visibility.Visible)
		{
			Utils.MixpanelNotification.SendNotification("Subscription Expired - Quit");
			_sdk.TaskBarService.QuitCommonFunction();
			CloseApp = true;
			Close();
		}
	}

	private void SubscriptionExpiredWindow_OnClosed(object sender, EventArgs e)
	{
		Quit();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		WindowHeader.GetParent(this);
	}
}
