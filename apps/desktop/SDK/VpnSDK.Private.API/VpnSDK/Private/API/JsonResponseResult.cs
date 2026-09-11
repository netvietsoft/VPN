using Newtonsoft.Json;

namespace VpnSDK.Private.API;

public class JsonResponseResult
{
	internal bool Success => ErrorCode == 0;

	[JsonProperty("reason")]
	internal string Reason { get; set; }

	[JsonProperty("code")]
	internal int ErrorCode { get; set; }

	internal JsonResponseResult()
	{
	}
}
