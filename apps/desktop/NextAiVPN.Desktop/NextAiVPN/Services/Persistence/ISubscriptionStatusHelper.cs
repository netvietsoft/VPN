using System.Threading.Tasks;
using NextAiVPN.Entities;

namespace NextAiVPN.Services.Persistence;

public interface ISubscriptionStatusHelper
{
	Task<SubscriptionStatus> GetSubscriptionStatusInfo();
}
