using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.MessageBoxWindows.ConflictDetected;

public partial class ConflictDetectedMessageBoxWindow : Window, IComponentConnector
{
	private readonly ConflictDetectedMessageBoxWindowViewModel _viewModel;

	public new bool DialogResult => _viewModel.DialogResult;

	public ConflictDetectedMessageBoxWindow(ConflictDetectedMessageBoxWindowViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		_viewModel.SetParent(this);
		base.DataContext = _viewModel;
	}
}
