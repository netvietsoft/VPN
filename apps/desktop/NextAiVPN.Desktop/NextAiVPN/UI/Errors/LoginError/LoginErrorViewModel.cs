using System.Windows.Input;

namespace NextAiVPN.UI.Errors.LoginError;

internal class LoginErrorViewModel : ViewModelBase, ILoginErrorViewModel
{
	private VPNWindowExpanded _vpnExpandedWindow;

	public string ErrorMessage { get; set; }

	public ICommand LinkClickCommand { get; set; }

	public LoginErrorViewModel()
	{
		LinkClickCommand = new RelayCommand(LinkClickCommandExecute);
	}

	public void GetExpandedWindow(VPNWindowExpanded vpnWindowExpanded)
	{
		_vpnExpandedWindow = vpnWindowExpanded;
	}

	private void LinkClickCommandExecute(object obj)
	{
		_vpnExpandedWindow.ShowProblemReportWindow(ErrorMessage);
	}
}
