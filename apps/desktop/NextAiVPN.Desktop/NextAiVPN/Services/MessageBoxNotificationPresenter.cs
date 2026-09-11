using System.Windows;

namespace NextAiVPN.Services;

internal class MessageBoxNotificationPresenter : INotificationPresenter
{
	public void ShowError(string message)
	{
		MessageBox.Show(message);
	}
}
