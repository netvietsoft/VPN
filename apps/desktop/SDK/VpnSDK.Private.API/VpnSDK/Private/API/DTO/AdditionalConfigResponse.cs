using System.Collections.Generic;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class AdditionalConfigResponse : JsonResponseResult
{
	[JsonProperty("vanity")]
	public string Vanity { get; set; }

	[JsonProperty("additionalApiHeaders")]
	public List<ApiHeader> AdditionalApiHeaders { get; set; }

	[JsonProperty("dns")]
	public List<Dns> Dns { get; set; }
}
