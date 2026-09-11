using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace NextAiVPN;

public partial class SubscriptionExpiredControl : UserControl, IComponentConnector
{
	private MessageWindow _messageWindow;

	private SDKMonitor _sdk;

	public SubscriptionExpiredControl()
	{
		InitializeComponent();
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		_messageWindow.Close();
	}

	public void GetMessageWindow(MessageWindow messageWin, SDKMonitor sdk)
	{
		_messageWindow = messageWin;
		_sdk = sdk;
	}

	private void Link_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_sdk.BrowserLinksOpener.OpenBrowserLink(7);
	}
}
