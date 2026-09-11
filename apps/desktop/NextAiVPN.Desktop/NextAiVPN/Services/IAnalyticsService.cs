using System.Threading.Tasks;

namespace NextAiVPN.Services;

public interface IAnalyticsService
{
	void RefreshData();

	Task SendNotification(string eventName);

	Task SendCustomConnectNotification(string eventName, string locationCity);

	Task SendConnectNotification(string eventName);
}
