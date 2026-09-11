using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;

namespace NextAiVPN.UI.Feedback.DiscardChanges;

public class DiscardChangesWindowViewModel : ViewModelBase
{
	private readonly DiscardChangesWindow _window;

	public ICommand CloseButtonClickCommand { get; set; }

	public ICommand YesButtonClickCommand { get; set; }

	public DiscardChangesWindowViewModel(DiscardChangesWindow window)
	{
		_window = window;
		CloseButtonClickCommand = new ActionCommand(CloseButtonClickCommandExecute);
		YesButtonClickCommand = new ActionCommand(YesButtonClickCommandExecute);
	}

	private void YesButtonClickCommandExecute()
	{
		_window.DialogResult = true;
		_window.Close();
	}

	private void CloseButtonClickCommandExecute()
	{
		_window.DialogResult = false;
		_window.Close();
	}
}
