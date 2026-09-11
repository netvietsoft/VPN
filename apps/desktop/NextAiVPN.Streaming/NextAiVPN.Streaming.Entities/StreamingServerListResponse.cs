using System.Collections.Generic;
using Newtonsoft.Json;

namespace NextAiVPN.Streaming.Entities;

public class StreamingServerListResponse
{
	[JsonProperty("countrycount")]
	public int CountryCount { get; set; }

	[JsonProperty("message")]
	public string Message { get; set; }

	[JsonProperty("totalservercount")]
	public int TotalServerCount { get; set; }

	[JsonProperty("serverlist")]
	public List<CountryServerList> ServerList { get; set; }

	[JsonProperty("success")]
	public int Success { get; set; }
}
