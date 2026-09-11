using Newtonsoft.Json;

namespace VpnSDK.Private.API;

public class ApiHeader
{
	[JsonProperty("headerName")]
	public string HeaderName { get; set; }

	[JsonProperty("headerValue")]
	public string HeaderValue { get; set; }
}
