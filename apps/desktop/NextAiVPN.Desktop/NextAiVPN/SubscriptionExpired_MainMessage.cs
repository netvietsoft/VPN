using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN;

public partial class SubscriptionExpired_MainMessage : UserControl, IComponentConnector
{
	private IBrowserLinksOpener _browserLinksOpener;

	public SubscriptionExpired_MainMessage()
	{
		InitializeComponent();
		ResubscribeBtn.AddHandler(UIElement.MouseLeftButtonUpEvent, new RoutedEventHandler(ResubscribeBtn_MouseUp), handledEventsToo: true);
	}

	public void ResubscribeBtn_MouseUp(object sender, RoutedEventArgs e)
	{
		Utils.Api.SetIsPurchaseStarted("1");
		_browserLinksOpener.OpenBrowserLink(7);
		Application.Current.Shutdown();
	}

	public void SetInstances(IBrowserLinksOpener browserLinksOpener)
	{
		_browserLinksOpener = browserLinksOpener;
	}

	private void ToS_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		_browserLinksOpener.OpenBrowserLink(1);
	}

	private void PP_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		_browserLinksOpener.OpenBrowserLink(2);
	}
}
