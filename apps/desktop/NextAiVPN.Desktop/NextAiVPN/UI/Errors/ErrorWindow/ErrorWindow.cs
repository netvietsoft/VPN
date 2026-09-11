using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.Errors.ErrorWindow;

public partial class ErrorWindow : Window, IComponentConnector
{
	public ErrorWindow(IErrorWindowViewModel viewModel)
	{
		InitializeComponent();
		base.DataContext = viewModel;
		viewModel.GetParentWindow(this);
	}
}
