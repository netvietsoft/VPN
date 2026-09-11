using Newtonsoft.Json;

namespace NextAiVPN.Entities;

internal class FeedbackResponse
{
	[JsonProperty("message")]
	public string Message { get; set; }

	[JsonProperty("success")]
	public string Success { get; set; }
}
