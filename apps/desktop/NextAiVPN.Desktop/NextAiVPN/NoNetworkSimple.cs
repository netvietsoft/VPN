using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN;

public partial class NoNetworkSimple : Window, IComponentConnector
{
	private bool closeApp = true;

	public NoNetworkSimple()
	{
		InitializeComponent();
		NoNetworkNextAiVPN.ExpandCollapse.Visibility = Visibility.Collapsed;
	}

	public void SetClosingState(bool state)
	{
		closeApp = state;
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (closeApp)
		{
			Application.Current.Shutdown();
		}
	}

	private void NoNetworkSimple_OnLoaded(object sender, RoutedEventArgs e)
	{
		WindowHeader.GetParent(this);
	}
}
