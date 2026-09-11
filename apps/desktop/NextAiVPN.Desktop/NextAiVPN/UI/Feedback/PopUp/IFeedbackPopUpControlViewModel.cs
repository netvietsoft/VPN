using System.Windows;
using System.Windows.Input;

namespace NextAiVPN.UI.Feedback.PopUp;

public interface IFeedbackPopUpControlViewModel
{
	VPNWindowExpanded ExpandedWindow { get; set; }

	Visibility UserControlVisibility { get; set; }

	ICommand CloseButtonClickCommand { get; set; }

	ICommand TextBlockLinkClickCommand { get; set; }

	ICommand RemindMeLaterClickCommand { get; set; }

	ICommand DontAskAgainClickCommand { get; set; }

	int UserControlHeight { get; set; }
}
