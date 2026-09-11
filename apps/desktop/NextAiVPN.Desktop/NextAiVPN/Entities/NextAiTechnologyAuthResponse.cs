using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class NextAiTechnologyAuthResponse
{
	[JsonProperty("id_token")]
	public string IdToken { get; set; }

	[JsonProperty("access_token")]
	public string AccessToken { get; set; }

	[JsonProperty("expires_in")]
	public string ExpiresIn { get; set; }

	[JsonProperty("token_type")]
	public string TokenType { get; set; }

	[JsonProperty("refresh_token")]
	public string RefreshToken { get; set; }

	[JsonProperty("scope")]
	public string Scope { get; set; }

	[JsonProperty("sub")]
	public string Sub { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("nickname")]
	public string Nickname { get; set; }

	[JsonProperty("given_name")]
	public string GivenName { get; set; }

	[JsonProperty("family_name")]
	public string FamilyName { get; set; }

	[JsonProperty("email")]
	public string Email { get; set; }

	[JsonProperty("expires_at")]
	public string ExpiresAt { get; set; }

	[JsonProperty("nextaivpn_id")]
	public string NextAiVpnId { get; set; }

	[JsonProperty("vpn_username")]
	public string VpnUsername { get; set; }

	[JsonProperty("vpn_password")]
	public string VpnPassword { get; set; }

	[JsonProperty("subscription_type")]
	public string SubscriptionType { get; set; }

	[JsonProperty("subscription_id")]
	public string SubscriptionId { get; set; }

	[JsonProperty("error")]
	public string Error { get; set; }

	[JsonProperty("message")]
	public string Message { get; set; }
}
