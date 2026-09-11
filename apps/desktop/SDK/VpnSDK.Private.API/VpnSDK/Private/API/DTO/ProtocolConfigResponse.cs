using System.Collections.Generic;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class ProtocolConfigResponse : JsonResponseResult
{
	[JsonProperty("allowed_ips")]
	public List<string> AllowedIps { get; set; }
}
