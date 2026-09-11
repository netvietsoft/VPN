using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.NoBrowserFound;

public partial class NoBrowserFoundWindow : Window, IComponentConnector
{
	private readonly NoBrowserFoundWindowViewModel _viewModel;

	public new bool DialogResult
	{
		get
		{
			return _viewModel.DialogResult;
		}
		set
		{
			_viewModel.DialogResult = value;
		}
	}

	public NoBrowserFoundWindow(NoBrowserFoundWindowViewModel viewModel)
	{
		_viewModel = viewModel;
		_viewModel.SetParentWindow(this);
		base.DataContext = _viewModel;
		InitializeComponent();
	}
}
