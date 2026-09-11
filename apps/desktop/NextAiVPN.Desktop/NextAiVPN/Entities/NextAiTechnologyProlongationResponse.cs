using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class NextAiTechnologyProlongationResponse
{
	[JsonProperty("success")]
	public bool IsSuccess { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("message")]
	public string Message { get; set; }
}
