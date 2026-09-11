using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace NextAiVPN;

public partial class NewUserControl : UserControl, IComponentConnector
{
	private MessageWindow _messageWindow;

	private SDKMonitor _sdk;

	public NewUserControl()
	{
		InitializeComponent();
	}

	public void GetMessageWindow(MessageWindow messageWindow, SDKMonitor sdk)
	{
		_messageWindow = messageWindow;
		_sdk = sdk;
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		_messageWindow.Close();
	}

	private void Link_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_sdk.BrowserLinksOpener.OpenBrowserLink(7);
	}
}
