using System.Windows.Input;

namespace NextAiVPN.UI.Errors.LoginError;

public interface ILoginErrorViewModel
{
	string ErrorMessage { get; set; }

	ICommand LinkClickCommand { get; set; }

	void GetExpandedWindow(VPNWindowExpanded vpnWindowExpanded);
}
