using System.Windows.Input;

namespace NextAiVPN.UI.Errors.ErrorWindow;

public interface IErrorWindowViewModel
{
	string MainHeader { get; set; }

	string SubHeader { get; set; }

	string Message { get; set; }

	ICommand GotItButtonClickCommand { get; set; }

	void GetParentWindow(ErrorWindow window);
}
