using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.MessageBoxWindows.AutoRenewal;

public partial class AutoRenewalMessageBoxWindow : Window, IComponentConnector
{
	private readonly AutoRenewalMessageBoxWindowViewModel _viewModel;

	public new bool DialogResult => _viewModel.DialogResult;

	public AutoRenewalMessageBoxWindow(AutoRenewalMessageBoxWindowViewModel viewModel)
	{
		_viewModel = viewModel;
		_viewModel.SetMessageBoxWindow(this);
		InitializeComponent();
		base.DataContext = _viewModel;
	}
}
