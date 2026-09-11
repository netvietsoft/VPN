using System.Windows;
using System.Windows.Input;

namespace NextAiVPN.UI.NotifyBalloons;

public class NotifyBalloonViewModel : ViewModelBase
{
	private Visibility _userControlVisibility;

	public string Header { get; set; }

	public string Message { get; set; }

	public string ImageSource { get; set; }

	public int Width { get; set; } = 420;

	public int Height { get; set; } = 150;

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

	public NotifyBalloonViewModel()
	{
		CloseButtonClickCommand = new RelayCommand(CloseButtonClickCommandExecute);
	}

	private void CloseButtonClickCommandExecute(object obj)
	{
		UserControlVisibility = Visibility.Collapsed;
	}
}
