using System.Collections.Generic;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class AccountMetadataRequest
{
	[JsonProperty("metadata")]
	public Dictionary<string, string> Metadata { get; private set; }

	public AccountMetadataRequest(Dictionary<string, string> metadata)
	{
		Metadata = metadata;
	}
}
