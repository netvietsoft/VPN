using System.Windows;
using System.Windows.Input;
using NextAiVPN.Enums;

namespace NextAiVPN.UI.Account.PopUp;

public class SubscriptionExpireSoonControlViewModel : ViewModelBase
{
	private Visibility _userControlVisibility = Visibility.Collapsed;

	public VPNWindowExpanded ExpandedWindow { get; set; }

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

	public ICommand CancelClickCommand { get; set; }

	public ICommand TurnOnClickCommand { get; set; }

	public SubscriptionExpireSoonControlViewModel()
	{
		CloseButtonClickCommand = new RelayCommand(CloseButtonClickCommandExecute);
		CancelClickCommand = new RelayCommand(CancelClickCommandExecute);
		TurnOnClickCommand = new RelayCommand(TurnOnClickCommandExecute);
	}

	private void TurnOnClickCommandExecute(object obj)
	{
		ExpandedWindow.ExpandedSideMenu.SetMenuOption(SideMenuOption.Account);
		Utils.MixpanelNotification.SendNotification("SubscriptionExpireSoonControl - Turn on auto-renewal");
		Close();
	}

	private void CancelClickCommandExecute(object obj)
	{
		Utils.MixpanelNotification.SendNotification("SubscriptionExpireSoonControl - Cancel");
		Close();
	}

	private void CloseButtonClickCommandExecute(object obj)
	{
		Utils.MixpanelNotification.SendNotification("SubscriptionExpireSoonControl - Close");
		Close();
	}

	private void Close()
	{
		UserControlVisibility = Visibility.Collapsed;
	}
}
