using System.Net;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class NetworkCredentialResponse : JsonResponseResult
{
	[JsonProperty("Username")]
	public string Username { get; set; }

	[JsonProperty("Password")]
	public string Password { get; set; }

	public NetworkCredential ToNetworkCredential()
	{
		return new NetworkCredential(Username, Password);
	}
}
