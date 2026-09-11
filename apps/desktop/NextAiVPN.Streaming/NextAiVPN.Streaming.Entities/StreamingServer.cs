using Newtonsoft.Json;

namespace NextAiVPN.Streaming.Entities;

public class StreamingServer
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("status")]
	public string Status { get; set; }

	[JsonProperty("utilization")]
	public string Utilization { get; set; }
}
