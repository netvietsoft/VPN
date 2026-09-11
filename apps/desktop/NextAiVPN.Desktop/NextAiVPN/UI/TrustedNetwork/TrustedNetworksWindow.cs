using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace NextAiVPN.UI.TrustedNetwork;

public partial class TrustedNetworksWindow : Window, IComponentConnector, IStyleConnector
{
	private readonly TrustedNetworksWindowViewModel _viewModel;

	public TrustedNetworksWindow(TrustedNetworksWindowViewModel viewModel)
	{
		_viewModel = viewModel;
		InitializeComponent();
		WindowHeader.GetParent(this);
		_viewModel.SetParent(this);
		_viewModel.ShowTrustedNetworkPanels();
		base.DataContext = _viewModel;
	}

	private void RemoveNetwork_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		_viewModel.RemoveTrustedNetworkClickCommand.Execute(sender);
	}

	private void Scroll_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		ScrollViewer obj = (ScrollViewer)sender;
		obj.ScrollToVerticalOffset(obj.VerticalOffset - (double)e.Delta);
		e.Handled = true;
	}
}
