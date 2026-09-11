using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN;

public partial class SubscriptionExpired_CompletePurchase : UserControl, IComponentConnector
{
	private IBrowserLinksOpener _browserLinksOpener;

	public SubscriptionExpired_CompletePurchase()
	{
		InitializeComponent();
		CompletePurchase.AddHandler(UIElement.MouseLeftButtonUpEvent, new RoutedEventHandler(CompletePurchase_MouseUp), handledEventsToo: true);
	}

	public void SetInstances(IBrowserLinksOpener browserLinksOpener)
	{
		_browserLinksOpener = browserLinksOpener;
	}

	public void CompletePurchase_MouseUp(object sender, RoutedEventArgs e)
	{
		Utils.Api.SetIsPurchaseStarted("1");
		_browserLinksOpener.OpenBrowserLink(7);
		Application.Current.Shutdown();
	}

	private void CustomerSupportLink_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		_browserLinksOpener.OpenBrowserLink(3);
	}
}
