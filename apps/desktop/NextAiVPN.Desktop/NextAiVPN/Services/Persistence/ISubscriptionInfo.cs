using System.Threading.Tasks;
using NextAiVPN.Entities;

namespace NextAiVPN.Services.Persistence;

public interface ISubscriptionInfo
{
	string Id { get; set; }

	string Message { get; set; }

	string UserName { get; set; }

	string SubscriptionType { get; set; }

	bool IsSubscriptionExpireSoon { get; set; }

	bool IsTrialExpireSoon { get; set; }

	bool IsTrial { get; set; }

	bool IsTrialLimitReached { get; set; }

	bool IsTrialEnded { get; set; }

	ActiveSubscription Subscription { get; set; }

	Task RefreshData();

	void ToggleAutoRenewal();

	Task<SubscriptionResponse> GetSubscriptionResponseAsync();

	Task<NextAiTechnologyProlongationResponse> NextAiTechnologyProlongationActionAsync();
}
