using System.Collections.Generic;
using Newtonsoft.Json;

namespace NextAiVPN.Entities;

internal class NotificationApiResponse
{
	[JsonProperty("message")]
	public string Message { get; set; }

	[JsonProperty("subscription_id")]
	public string SubscriptionId { get; set; }

	[JsonProperty("size")]
	public int Size { get; set; }

	[JsonProperty("success")]
	public int Success { get; set; }

	[JsonProperty("data")]
	public List<NotificationData> Data { get; set; }
}
