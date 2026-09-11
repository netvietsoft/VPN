using System.Windows;
using System.Windows.Input;
using NextAiVPN.Common;

namespace NextAiVPN.UI.Feedback.PopUp;

public class FeedbackPopUpControlViewModel : ViewModelBase, IFeedbackPopUpControlViewModel
{
	private Visibility _userControlVisibility = Visibility.Collapsed;

	private Visibility _buttonsVisibilityy = Visibility.Collapsed;

	private int _userControlHeight = 90;

	private readonly IAppSettingsHelper _appSettingsHelper;

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

	public Visibility ButtonsVisibility
	{
		get
		{
			return _buttonsVisibilityy;
		}
		set
		{
			_buttonsVisibilityy = value;
			OnPropertyChanged("ButtonsVisibility");
		}
	}

	public int UserControlHeight
	{
		get
		{
			return _userControlHeight;
		}
		set
		{
			_userControlHeight = value;
			OnPropertyChanged("UserControlHeight");
		}
	}

	public ICommand CloseButtonClickCommand { get; set; }

	public ICommand TextBlockLinkClickCommand { get; set; }

	public ICommand RemindMeLaterClickCommand { get; set; }

	public ICommand DontAskAgainClickCommand { get; set; }

	public FeedbackPopUpControlViewModel(IAppSettingsHelper appSettingsHelper)
	{
		_appSettingsHelper = appSettingsHelper;
		CloseButtonClickCommand = new RelayCommand(CloseButtonClickCommandExecute);
		TextBlockLinkClickCommand = new RelayCommand(TextBlockLinkClickCommandExecute);
		RemindMeLaterClickCommand = new RelayCommand(RemindMeLaterClickCommandExecute);
		DontAskAgainClickCommand = new RelayCommand(DontAskAgainClickCommandExecute);
	}

	private void DontAskAgainClickCommandExecute(object obj)
	{
		_appSettingsHelper.SetValue("IsNeedToShowFeedbackPopUp", "0");
		Close();
	}

	private void RemindMeLaterClickCommandExecute(object obj)
	{
		_appSettingsHelper.SetValue("IsNeedToShowFeedbackPopUp", "1");
		Close();
	}

	private void TextBlockLinkClickCommandExecute(object obj)
	{
		ExpandedWindow?.ShowFeedbackWindow();
		Close();
	}

	private void CloseButtonClickCommandExecute(object obj)
	{
		string value = _appSettingsHelper.GetValue("IsNeedToShowFeedbackPopUp");
		if (value.Equals("1"))
		{
			Close();
		}
		if (value.Equals(""))
		{
			_appSettingsHelper.SetValue("IsNeedToShowFeedbackPopUp", "1");
			UserControlHeight = 190;
			ButtonsVisibility = Visibility.Visible;
		}
		else
		{
			Close();
		}
	}

	private void Close()
	{
		UserControlVisibility = Visibility.Collapsed;
	}
}
