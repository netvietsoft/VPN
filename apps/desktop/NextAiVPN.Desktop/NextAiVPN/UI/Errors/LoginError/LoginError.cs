using System.Windows.Controls;
using System.Windows.Markup;

namespace NextAiVPN.UI.Errors.LoginError;

public partial class LoginError : UserControl, IComponentConnector
{
	private readonly ILoginErrorViewModel _loginErrorViewModel;

	public string ErrorMessage
	{
		get
		{
			return _loginErrorViewModel.ErrorMessage;
		}
		set
		{
			_loginErrorViewModel.ErrorMessage = value;
		}
	}

	public LoginError()
	{
		InitializeComponent();
		_loginErrorViewModel = new LoginErrorViewModel();
		base.DataContext = _loginErrorViewModel;
	}

	public void GetExpandedWindow(VPNWindowExpanded vpnWindowExpanded)
	{
		_loginErrorViewModel.GetExpandedWindow(vpnWindowExpanded);
	}
}
