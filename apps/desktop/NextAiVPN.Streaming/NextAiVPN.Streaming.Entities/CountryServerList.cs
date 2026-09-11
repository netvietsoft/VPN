using System.Collections.Generic;
using Newtonsoft.Json;

namespace NextAiVPN.Streaming.Entities;

public class CountryServerList
{
	[JsonProperty("country")]
	public string Country { get; set; }

	[JsonProperty("countryCode")]
	public string CountryCode { get; set; }

	[JsonProperty("protocol")]
	public string Protocol { get; set; }

	[JsonProperty("servers")]
	public List<StreamingServer> Servers { get; set; }
}
