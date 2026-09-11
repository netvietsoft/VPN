using Newtonsoft.Json;

namespace VpnSDK.Private.API.Wireguard.DTO;

public class ConfigurationResponse : JsonResponseResult
{
	[JsonProperty("success", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public new bool Success { get; set; }

	[JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public string Message { get; set; }

	[JsonProperty("config")]
	public WireguardConfiguration Config { get; set; }

	[JsonProperty("settings")]
	public WireguardSettings Settings { get; set; }
}
