using System;
using System.Collections.Generic;
using System.Linq;
using NextAiVPN.Enums;
using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class NextAiGlobalTokenValidatorResponse
{
	[JsonProperty("success")]
	public int IsSuccess { get; set; }

	[JsonProperty("user_id")]
	public string UserId { get; set; }

	[JsonProperty("given_name")]
	public string GivenName { get; set; }

	[JsonProperty("vpn_username")]
	public string VpnUsername { get; set; }

	[JsonProperty("vpn_password")]
	public string VpnPassword { get; set; }

	[JsonProperty("created_at")]
	public string CreatedAt { get; set; }

	[JsonProperty("username")]
	public string Username { get; set; }

	[JsonProperty("active_subscriptions")]
	public List<Subscription> ActiveSubscriptions { get; set; }

	[JsonProperty("message")]
	public string Message { get; set; }

	[JsonProperty("subscription_type")]
	public string SubscriptionType { get; set; }

	public Subscription GetActiveSubscription()
	{
		if (ActiveSubscriptions == null || ActiveSubscriptions.Count == 0)
		{
			return null;
		}
		return ActiveSubscriptions.FirstOrDefault((Subscription x) => !IsDateGone(x.ExpiresAt));
	}

	public SubscriptionState GetSubscriptionState()
	{
		if (GetActiveSubscription() != null)
		{
			return SubscriptionState.Active;
		}
		if (string.IsNullOrEmpty(Username))
		{
			return SubscriptionState.No;
		}
		return SubscriptionState.Expired;
	}

	private static bool IsDateGone(DateTime dateToCheck)
	{
		return dateToCheck < DateTime.Now;
	}
}
