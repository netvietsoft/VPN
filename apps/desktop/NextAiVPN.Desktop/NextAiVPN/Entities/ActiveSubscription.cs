using System;
using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class ActiveSubscription
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("nc_user")]
	public string NcUser { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("product")]
	public Product Product { get; set; }

	[JsonProperty("status")]
	public string Status { get; set; }

	[JsonProperty("view_status")]
	public string ViewStatus { get; set; }

	[JsonProperty("plan")]
	public Plan Plan { get; set; }

	[JsonProperty("new_plan")]
	public object NewPlan { get; set; }

	[JsonProperty("autorenewal")]
	public bool Autorenewal { get; set; }

	[JsonProperty("current_period_start")]
	public DateTime? CurrentPeriodStart { get; set; }

	[JsonProperty("current_period_end")]
	public DateTime? CurrentPeriodEnd { get; set; }

	[JsonProperty("trial_end")]
	public DateTime? TrialEnd { get; set; }

	[JsonProperty("cancelled_at")]
	public string CancelledAt { get; set; }

	[JsonProperty("created_at")]
	public DateTime? CreatedAt { get; set; }

	[JsonProperty("updated_at")]
	public DateTime? UpdatedAt { get; set; }

	[JsonProperty("expires_at")]
	public DateTime? ExpiresAt { get; set; }

	[JsonProperty("paused")]
	public object Paused { get; set; }

	[JsonProperty("current_period_price")]
	public CurrentPeriodPrice CurrentPeriodPrice { get; set; }

	[JsonProperty("product_id")]
	public string ProductId { get; set; }

	[JsonProperty("trial_converted_to_paid")]
	public bool TrialConvertedToPaid { get; set; }

	[JsonProperty("consumption")]
	public int Consumption { get; set; }
}
