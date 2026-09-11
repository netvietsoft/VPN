using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.SplitTunneling;

public partial class StreamingSplitTunnelingDomainsWindow : Window, IComponentConnector
{
	public StreamingSplitTunnelingDomainsWindow(StreamingSplitTunnelingDomainsWindowViewModel domainsWindowViewModel)
	{
		InitializeComponent();
		base.Closing += domainsWindowViewModel.OnWindowClosing;
		ScrollViewer.PreviewMouseWheel += domainsWindowViewModel.OnScrollPreviewMouseWheel;
		base.DataContext = domainsWindowViewModel;
	}
}
