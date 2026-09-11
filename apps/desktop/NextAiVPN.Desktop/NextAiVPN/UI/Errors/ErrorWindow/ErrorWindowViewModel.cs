using System.Windows.Input;

namespace NextAiVPN.UI.Errors.ErrorWindow;

public class ErrorWindowViewModel : ViewModelBase, IErrorWindowViewModel
{
	private ErrorWindow _parentWindow;

	public string MainHeader { get; set; }

	public string SubHeader { get; set; }

	public string Message { get; set; }

	public ICommand GotItButtonClickCommand { get; set; }

	public void GetParentWindow(ErrorWindow window)
	{
		_parentWindow = window;
	}

	public ErrorWindowViewModel()
	{
		GotItButtonClickCommand = new RelayCommand(GotItButtonClickCommandExecute);
	}

	private void GotItButtonClickCommandExecute(object obj)
	{
		_parentWindow?.Close();
	}
}
