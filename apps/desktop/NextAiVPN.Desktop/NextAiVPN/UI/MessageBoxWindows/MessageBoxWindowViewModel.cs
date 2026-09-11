using System.Windows;
using System.Windows.Input;

namespace NextAiVPN.UI.MessageBoxWindows;

public class MessageBoxWindowViewModel : ViewModelBase
{
	private MessageBoxWindow _messageBoxWindow;

	private Visibility _cancelButtonVisibility;

	private Visibility _okButtonVisibility;

	public bool DialogResult { get; set; }

	public string Header { get; set; }

	public string Description { get; set; }

	public string OkButtonText { get; set; }

	public string CancelButtonText { get; set; }

	public Visibility CancelButtonVisibility
	{
		get
		{
			return _cancelButtonVisibility;
		}
		set
		{
			_cancelButtonVisibility = value;
			OnPropertyChanged("CancelButtonVisibility");
		}
	}

	public Visibility OkButtonVisibility
	{
		get
		{
			return _okButtonVisibility;
		}
		set
		{
			_okButtonVisibility = value;
			OnPropertyChanged("OkButtonVisibility");
		}
	}

	public ICommand OkClickCommand { get; set; }

	public ICommand CancelClickCommand { get; set; }

	public ICommand CloseClickCommand { get; set; }

	public MessageBoxWindowViewModel()
	{
		OkClickCommand = new RelayCommand(OkClickCommandExecute);
		CloseClickCommand = new RelayCommand(CloseClickCommandExecute);
		CancelClickCommand = new RelayCommand(CancelClickCommandExecute);
	}

	public void SetMessageBoxWindow(MessageBoxWindow window)
	{
		_messageBoxWindow = window;
	}

	private void CancelClickCommandExecute(object obj)
	{
		DialogResult = false;
		Close();
	}

	private void CloseClickCommandExecute(object obj)
	{
		DialogResult = false;
		Close();
	}

	private void OkClickCommandExecute(object obj)
	{
		DialogResult = true;
		Close();
	}

	private void Close()
	{
		_messageBoxWindow.Close();
	}
}
