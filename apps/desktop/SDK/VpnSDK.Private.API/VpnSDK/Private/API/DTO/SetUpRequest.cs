using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class SetUpRequest
{
	[JsonProperty("server")]
	public string Server { get; set; }
}
