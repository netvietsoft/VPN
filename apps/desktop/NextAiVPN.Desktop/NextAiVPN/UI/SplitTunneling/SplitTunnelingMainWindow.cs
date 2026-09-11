using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.SplitTunneling;

public partial class SplitTunnelingMainWindow : Window, IComponentConnector
{
	private readonly SplitTunnelingMainWindowViewModel _mainWindowViewModel;

	public bool IsNeededToShowPopUp => _mainWindowViewModel.IsNeededToShowPopUp;

	public SplitTunnelingMainWindow(SplitTunnelingMainWindowViewModel mainWindowViewModel)
	{
		InitializeComponent();
		_mainWindowViewModel = mainWindowViewModel;
		base.Closing += _mainWindowViewModel.OnWindowClosing;
		SplitTunnelingMain.DataContext = _mainWindowViewModel.SplitTunnelingMainControlViewModel;
		SplitTunnelingMain.AppCounterControl.DataContext = _mainWindowViewModel.SplitTunnelingMainControlViewModel.AppCounterControlViewModel;
		SplitTunnelingMain.DomainCounterControl.DataContext = _mainWindowViewModel.SplitTunnelingMainControlViewModel.DomainCounterControlViewModel;
		base.DataContext = _mainWindowViewModel;
	}
}
