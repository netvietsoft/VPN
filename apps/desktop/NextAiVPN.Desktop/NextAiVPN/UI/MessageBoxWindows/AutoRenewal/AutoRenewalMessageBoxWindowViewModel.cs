using System.Windows.Input;

namespace NextAiVPN.UI.MessageBoxWindows.AutoRenewal;

public class AutoRenewalMessageBoxWindowViewModel
{
	private AutoRenewalMessageBoxWindow _messageBoxWindow;

	public bool DialogResult { get; set; }

	public string Header { get; set; }

	public string Description { get; set; }

	public ICommand YesClickCommand { get; set; }

	public ICommand CancelClickCommand { get; set; }

	public ICommand CloseClickCommand { get; set; }

	public AutoRenewalMessageBoxWindowViewModel()
	{
		Header = "Auto-renewal subscription";
		Description = "By clicking confirm, your subscription will be renewed automatically";
		YesClickCommand = new RelayCommand(YesClickCommandClickCommandExecute);
		CloseClickCommand = new RelayCommand(CloseClickCommandExecute);
		CancelClickCommand = new RelayCommand(CancelClickCommandExecute);
	}

	public void SetMessageBoxWindow(AutoRenewalMessageBoxWindow window)
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

	private void YesClickCommandClickCommandExecute(object obj)
	{
		DialogResult = true;
		Close();
	}

	private void Close()
	{
		_messageBoxWindow.Hide();
	}
}
