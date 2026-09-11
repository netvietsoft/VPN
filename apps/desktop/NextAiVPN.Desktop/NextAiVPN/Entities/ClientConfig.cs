using System.Collections.Generic;
using Newtonsoft.Json;

namespace NextAiVPN.Entities;

internal class ClientConfig
{
	[JsonProperty("message")]
	public string Message { get; set; }

	[JsonProperty("size")]
	public int Size { get; set; }

	[JsonProperty("success")]
	public int Success { get; set; }

	[JsonProperty("data")]
	public List<Data> Data { get; set; }
}
