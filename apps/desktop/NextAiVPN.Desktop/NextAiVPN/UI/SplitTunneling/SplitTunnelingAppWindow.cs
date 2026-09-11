using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.SplitTunneling;

public partial class SplitTunnelingAppWindow : Window, IComponentConnector
{
	public SplitTunnelingAppWindow(SplitTunnelingAppWindowViewModel appWindowViewModel)
	{
		InitializeComponent();
		base.Closing += appWindowViewModel.OnWindowClosing;
		ScrollViewer.PreviewMouseWheel += appWindowViewModel.OnScrollPreviewMouseWheel;
		base.DataContext = appWindowViewModel;
	}
}
