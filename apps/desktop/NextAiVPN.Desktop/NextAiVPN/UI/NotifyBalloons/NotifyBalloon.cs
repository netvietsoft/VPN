using System.Windows.Controls;
using System.Windows.Markup;

namespace NextAiVPN.UI.NotifyBalloons;

public partial class NotifyBalloon : UserControl, IComponentConnector
{
	public NotifyBalloon(NotifyBalloonViewModel viewModel)
	{
		InitializeComponent();
		base.DataContext = viewModel;
	}
}
