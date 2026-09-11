using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using NextAiVPN.Entities;

namespace NextAiVPN.Services;

public interface INotificationService
{
	Task<ObservableCollection<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default(CancellationToken));

	void SaveSortSelected(string sortValue);

	void SaveNewNotificationsCount(int count);

	Task<bool> HasNewNotificationAsync(CancellationToken cancellationToken = default(CancellationToken));
}
