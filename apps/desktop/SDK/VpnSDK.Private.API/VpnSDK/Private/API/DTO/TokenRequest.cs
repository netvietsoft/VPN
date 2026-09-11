using Newtonsoft.Json;

namespace VpnSDK.Private.API.DTO;

public class TokenRequest
{
	[JsonProperty("refresh_token")]
	public string RefreshToken { get; set; }

	public TokenRequest(string refreshToken)
	{
		RefreshToken = refreshToken;
	}
}
