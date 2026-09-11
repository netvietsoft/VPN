using Newtonsoft.Json;

namespace NextAiVPN.Entities;

internal class CheckResponse
{
	[JsonProperty("valid")]
	public string Valid { get; set; }

	[JsonProperty("user_message")]
	public string UserMessage { get; set; }

	[JsonProperty("error_type")]
	public int ErrorType { get; set; }

	[JsonProperty("subscription_type")]
	public int SubscriptionType { get; set; }
}
