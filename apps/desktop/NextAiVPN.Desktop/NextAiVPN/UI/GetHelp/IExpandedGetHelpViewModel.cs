using System.Windows.Input;

namespace NextAiVPN.UI.GetHelp;

public interface IExpandedGetHelpViewModel
{
	string MainControlHeader { get; set; }

	string SendFeedbackHeader { get; set; }

	string SendFeedbackText { get; set; }

	string SendFeedbackText2 { get; set; }

	string SendFeedbackLinkText { get; set; }

	string FAQHeader { get; set; }

	string FAQText { get; set; }

	string FAQText2 { get; set; }

	string FAQLinkText { get; set; }

	string CustomerSupportHeader { get; set; }

	string CustomerSupportText { get; set; }

	string CustomerSupportLinkText { get; set; }

	ICommand SendFeedbackMouseDownCommand { get; set; }

	ICommand FAQMouseDownCommand { get; set; }

	ICommand CustomerSupportMouseDownCommand { get; set; }
}
