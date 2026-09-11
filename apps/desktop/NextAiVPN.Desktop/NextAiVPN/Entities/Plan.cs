using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class Plan
{
	[JsonProperty("id")]
	public int Id { get; set; }

	[JsonProperty("slug")]
	public string Slug { get; set; }

	[JsonProperty("period_iso_8601")]
	public string PeriodIso8601 { get; set; }

	[JsonProperty("params")]
	public object Params { get; set; }
}
