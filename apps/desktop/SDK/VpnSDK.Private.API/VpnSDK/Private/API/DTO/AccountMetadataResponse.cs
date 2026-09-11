using System.Collections.Generic;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class AccountMetadataResponse : JsonResponseResult
{
	[JsonProperty("email")]
	public string Email { get; set; }

	[JsonProperty("account_type")]
	public int AccountType { get; set; }

	[JsonProperty("is_authorized")]
	public bool IsAuthorized { get; set; }

	[JsonProperty("metadata")]
	public Dictionary<string, string> Metadata { get; set; }
}
