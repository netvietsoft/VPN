using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.MessageBoxWindows;

public partial class MessageBoxWindow : Window, IComponentConnector
{
	private readonly MessageBoxWindowViewModel _viewModel;

	public new bool DialogResult => _viewModel.DialogResult;

	public MessageBoxWindow(MessageBoxWindowViewModel viewModel)
	{
		_viewModel = viewModel;
		_viewModel.SetMessageBoxWindow(this);
		InitializeComponent();
		base.DataContext = _viewModel;
	}
}
