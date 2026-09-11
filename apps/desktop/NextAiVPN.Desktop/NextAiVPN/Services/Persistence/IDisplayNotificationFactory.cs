using System.Windows.Controls;

namespace NextAiVPN.Services.Persistence;

internal interface IDisplayNotificationFactory
{
	void ManageNotifications(ref ListView notificationList);
}
