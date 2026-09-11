using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;

namespace NextAiVPN.UI.MessageBoxWindows.SignOutMessageWindow;

public class SignOutMessageBoxWindowViewModel : ViewModelBase
{
	private SignOutMessageBoxWindow _parent;

	private Visibility _parentWindowVisibility = Visibility.Hidden;

	public ICommand CancelButtonClickCommand { get; set; }

	public ICommand YesButtonClickCommand { get; set; }

	public string Title { get; set; } = "Ready to Sign Out?";

	public string Description { get; set; } = "You'll need to log in again to use the VPN";

	public bool DialogResult { get; set; }

	public Visibility ParentWindowVisibility
	{
		get
		{
			return _parentWindowVisibility;
		}
		set
		{
			if (_parentWindowVisibility != value)
			{
				_parentWindowVisibility = value;
				OnPropertyChanged("ParentWindowVisibility");
			}
		}
	}

	public SignOutMessageBoxWindowViewModel()
	{
		CancelButtonClickCommand = new ActionCommand(CancelButtonClickCommandExecute);
		YesButtonClickCommand = new ActionCommand(YesButtonClickCommandExecute);
	}

	public void SetParent(SignOutMessageBoxWindow parent)
	{
		_parent = parent;
	}

	private void YesButtonClickCommandExecute()
	{
		Close(dialogResult: true);
	}

	private void CancelButtonClickCommandExecute()
	{
		Close(dialogResult: false);
	}

	private void Close(bool dialogResult)
	{
		DialogResult = dialogResult;
		_parent?.Close();
	}

	public void OnWindowClosing(object sender, CancelEventArgs e)
	{
		ParentWindowVisibility = Visibility.Hidden;
		e.Cancel = true;
	}
}
