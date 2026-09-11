using System;
using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class Subscription
{
	[JsonProperty("user_product_id")]
	public string UserProductId { get; set; }

	[JsonProperty("period")]
	public string Period { get; set; }

	[JsonProperty("auto_renew")]
	public bool AutoRenew { get; set; }

	[JsonProperty("created_at")]
	public DateTime CreatedAt { get; set; }

	[JsonProperty("expires_at")]
	public DateTime ExpiresAt { get; set; }
}
