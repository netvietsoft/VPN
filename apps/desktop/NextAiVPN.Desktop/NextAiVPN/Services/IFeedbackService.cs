using System.Threading.Tasks;

namespace NextAiVPN.Services;

public interface IFeedbackService
{
	Task<string> SendFeedbackAsync(string score, string feedbackBody, bool needToSendDiagnosticFile);

	Task<string> SendFeedbackNotification(string score, string feedbackBody, bool needToSendDiagnosticFile);

	void SaveLastFeedbackClosed(bool reset);
}
