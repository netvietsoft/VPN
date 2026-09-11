using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.SplitTunneling;

public partial class SplitTunnelingDomainWindow : Window, IComponentConnector
{
	public SplitTunnelingDomainWindow(SplitTunnelingDomainWindowViewModel splitTunnelingDomainWindowViewModel)
	{
		InitializeComponent();
		base.Closing += splitTunnelingDomainWindowViewModel.OnWindowClosing;
		ScrollViewer.PreviewMouseWheel += splitTunnelingDomainWindowViewModel.OnScrollPreviewMouseWheel;
		base.DataContext = splitTunnelingDomainWindowViewModel;
	}
}
