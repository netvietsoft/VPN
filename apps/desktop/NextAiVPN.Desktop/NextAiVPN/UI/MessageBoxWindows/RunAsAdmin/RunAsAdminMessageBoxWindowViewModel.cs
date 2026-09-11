using System.Windows.Input;

namespace NextAiVPN.UI.MessageBoxWindows.RunAsAdmin;

public class RunAsAdminMessageBoxWindowViewModel : ViewModelBase
{
	private RunAsAdminMessageBoxWindow _messageBoxWindow;

	public bool DialogResult { get; set; }

	public string Header { get; set; }

	public string Description { get; set; }

	public ICommand OkClickCommand { get; set; }

	public ICommand CancelClickCommand { get; set; }

	public ICommand CloseClickCommand { get; set; }

	public RunAsAdminMessageBoxWindowViewModel()
	{
		Header = "Run as an administrator";
		Description = "Click OK to relaunch the app as an administrator";
		OkClickCommand = new RelayCommand(OkClickCommandClickCommandExecute);
		CloseClickCommand = new RelayCommand(CloseClickCommandExecute);
		CancelClickCommand = new RelayCommand(CancelClickCommandExecute);
	}

	public void SetMessageBoxWindow(RunAsAdminMessageBoxWindow window)
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

	private void OkClickCommandClickCommandExecute(object obj)
	{
		DialogResult = true;
		Close();
	}

	private void Close()
	{
		_messageBoxWindow.Close();
	}
}
