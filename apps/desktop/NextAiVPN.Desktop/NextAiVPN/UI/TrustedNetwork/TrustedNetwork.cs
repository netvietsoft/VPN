using System.Windows.Controls;
using System.Windows.Markup;

namespace NextAiVPN.UI.TrustedNetwork;

public partial class TrustedNetwork : UserControl, IComponentConnector
{
	public TrustedNetwork(TrustedNetworkViewModel viewModel)
	{
		InitializeComponent();
		viewModel.Parent = this;
		base.DataContext = viewModel;
	}
}
