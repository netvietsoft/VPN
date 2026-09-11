using Newtonsoft.Json;

namespace NextAiVPN.Entities;

internal class Value
{
	[JsonProperty("title")]
	public string Title { get; set; }

	[JsonProperty("enable")]
	public bool Enable { get; set; }

	[JsonProperty("subtitle")]
	public string Subtitle { get; set; }
}
