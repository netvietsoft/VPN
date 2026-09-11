using Newtonsoft.Json;

namespace VpnSDK.Private.API.Wireguard.DTO;

public class IsValidResponse
{
	[JsonProperty("isvalid")]
	public bool IsValid { get; set; }

	[JsonProperty("success")]
	public bool Success { get; set; } = true;

	[JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public string Message { get; set; }
}
