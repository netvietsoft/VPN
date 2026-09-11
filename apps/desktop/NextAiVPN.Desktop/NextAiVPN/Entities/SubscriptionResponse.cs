using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class SubscriptionResponse
{
	[JsonProperty("active_subscription")]
	public ActiveSubscription ActiveSubscription { get; set; }

	[JsonProperty("subscription_type")]
	public string SubscriptionType { get; set; }

	[JsonProperty("success")]
	public string Success { get; set; }

	[JsonProperty("message")]
	public string Message { get; set; }

	[JsonProperty("is_trial_period")]
	public bool IsTrialPeriod { get; set; }
}
