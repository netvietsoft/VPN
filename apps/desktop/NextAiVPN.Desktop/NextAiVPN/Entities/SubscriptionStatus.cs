using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class SubscriptionStatus
{
	[JsonProperty("valid")]
	public string Valid { get; set; }

	[JsonProperty("error_type")]
	public string ErrorType { get; set; }

	[JsonProperty("user_message")]
	public string UserMessage { get; set; }

	[JsonProperty("status")]
	public string Status { get; set; }

	[JsonProperty("subscription_type")]
	public string SubscriptionType { get; set; }
}
