using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.MessageBoxWindows.RunAsAdmin;

public partial class RunAsAdminMessageBoxWindow : Window, IComponentConnector
{
	private readonly RunAsAdminMessageBoxWindowViewModel _viewModel;

	public new bool DialogResult => _viewModel.DialogResult;

	public RunAsAdminMessageBoxWindow(RunAsAdminMessageBoxWindowViewModel viewModel)
	{
		_viewModel = viewModel;
		_viewModel.SetMessageBoxWindow(this);
		InitializeComponent();
		base.DataContext = _viewModel;
	}
}
