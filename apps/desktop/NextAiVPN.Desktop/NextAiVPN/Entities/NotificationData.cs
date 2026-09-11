using System;
using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class NotificationData
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("title")]
	public string Title { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("date")]
	public DateTime Date { get; set; }

	[JsonProperty("type")]
	public int Type { get; set; }
}
