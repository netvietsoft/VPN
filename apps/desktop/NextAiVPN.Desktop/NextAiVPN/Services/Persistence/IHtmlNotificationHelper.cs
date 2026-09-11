using System.Windows.Controls;

namespace NextAiVPN.Services.Persistence;

internal interface IHtmlNotificationHelper
{
	void ConvertToTextBlock(string htmlString, ref TextBlock textBlock);
}
