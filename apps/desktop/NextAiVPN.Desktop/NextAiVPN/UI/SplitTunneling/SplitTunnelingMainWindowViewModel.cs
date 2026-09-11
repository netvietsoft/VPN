using System.ComponentModel;
using System.Windows;

namespace NextAiVPN.UI.SplitTunneling;

public class SplitTunnelingMainWindowViewModel : ViewModelBase
{
	private Visibility _parentWindowVisibility = Visibility.Hidden;

	public bool IsNeededToShowPopUp => SplitTunnelingMainControlViewModel.IsNeededToShowPopUp;

	public Visibility ParentWindowVisibility
	{
		get
		{
			return _parentWindowVisibility;
		}
		set
		{
			if (_parentWindowVisibility != value)
			{
				_parentWindowVisibility = value;
				if (_parentWindowVisibility == Visibility.Visible)
				{
					SplitTunnelingMainControlViewModel.SetCounters();
				}
				OnPropertyChanged("ParentWindowVisibility");
			}
		}
	}

	public SplitTunnelingMainControlViewModel SplitTunnelingMainControlViewModel { get; private set; }

	public SplitTunnelingMainWindowViewModel(SplitTunnelingMainControlViewModel splitTunnelingMainControlViewModel)
	{
		SplitTunnelingMainControlViewModel = splitTunnelingMainControlViewModel;
		SplitTunnelingMainControlViewModel.SplitTunnelingMainWindowViewModel = this;
	}

	public void OnWindowClosing(object sender, CancelEventArgs e)
	{
		ParentWindowVisibility = Visibility.Hidden;
		e.Cancel = true;
	}
}
