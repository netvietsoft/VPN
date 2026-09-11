namespace NextAiVPN.Entities;

public class OAuth2NextAiGlobalParameters
{
	public string AuthorizeUrl { get; private set; }

	public string TokenUrl { get; private set; }

	public string ClientId { get; private set; }

	public string RedirectUri { get; private set; }

	public string CallbackUrlScheme { get; private set; }

	public OAuth2NextAiGlobalParameters()
	{
		string value = "com.nextaitechnology.vpn";
		string text = "nextaiglobal.com";
		string authorizeUrl = "https://www." + text + "/connect/authorize";
		string tokenUrl = "https://id.service." + text + "/connect/token";
		string clientId = "nextaivpn.app";
		string redirectUri = $"{value}://www.{text}/ios/{value}/callback";
		AuthorizeUrl = authorizeUrl;
		TokenUrl = tokenUrl;
		ClientId = clientId;
		RedirectUri = redirectUri;
		CallbackUrlScheme = "nextaivpn";
	}
}
