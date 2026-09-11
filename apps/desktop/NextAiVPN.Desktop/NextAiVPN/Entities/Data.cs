using Newtonsoft.Json;

namespace NextAiVPN.Entities;

internal class Data
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("value")]
	public Value Value { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }
}
