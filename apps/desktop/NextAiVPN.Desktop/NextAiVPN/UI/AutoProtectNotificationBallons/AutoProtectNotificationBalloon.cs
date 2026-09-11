using System.Windows.Controls;
using System.Windows.Markup;

namespace NextAiVPN.UI.AutoProtectNotificationBallons;

public partial class AutoProtectNotificationBalloon : UserControl, IComponentConnector
{
	public AutoProtectNotificationBalloon(AutoProtectNotificationBalloonViewModel autoProtectNotificationViewModel)
	{
		InitializeComponent();
		base.DataContext = autoProtectNotificationViewModel;
	}
}
