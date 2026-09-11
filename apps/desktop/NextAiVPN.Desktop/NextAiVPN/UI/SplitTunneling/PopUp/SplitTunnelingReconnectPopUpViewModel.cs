using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace NextAiVPN.UI.SplitTunneling.PopUp;

public class SplitTunnelingReconnectPopUpViewModel : ViewModelBase
{
	public SDKMonitor SdkMonitor;

	private Visibility _userControlVisibility = Visibility.Collapsed;

	public Visibility UserControlVisibility
	{
		get
		{
			return _userControlVisibility;
		}
		set
		{
			_userControlVisibility = value;
			OnPropertyChanged("UserControlVisibility");
		}
	}

	public ICommand CloseButtonClickCommand { get; set; }

	public ICommand TextBlockLinkClickCommand { get; set; }

	public SplitTunnelingReconnectPopUpViewModel()
	{
		CloseButtonClickCommand = new RelayCommand(CloseButtonClickCommandExecute);
		TextBlockLinkClickCommand = new RelayCommand(TextBlockLinkClickCommandExecute);
	}

	private void TextBlockLinkClickCommandExecute(object obj)
	{
		SdkMonitor.DisconnectVPN();
		Thread.Sleep(1500);
		SdkMonitor.ConnectToVPN(SdkMonitor.NextAiVpnLocation);
		Close();
	}

	private void CloseButtonClickCommandExecute(object obj)
	{
		Close();
	}

	private void Close()
	{
		UserControlVisibility = Visibility.Collapsed;
	}
}
