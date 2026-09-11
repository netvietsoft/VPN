using System.Net;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class GeoIP : JsonResponseResult
{
	[JsonProperty("ip")]
	public IPAddress IP { get; set; }

	[JsonProperty("location")]
	public GeoLocation Location { get; set; }
}
