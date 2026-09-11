using System;
using Newtonsoft.Json;
using VpnSDK.Private.API.Utilities;

namespace VpnSDK.Private.API.DTO;

public class LoginRequest
{
	[JsonProperty("client")]
	public string Client { get; private set; } = "Unknown";

	[JsonProperty("os")]
	public string OS { get; private set; }

	[JsonProperty("username")]
	public string Username { get; private set; }

	[JsonProperty("password")]
	public string Password { get; private set; }

	[JsonProperty(PropertyName = "api_key")]
	public string ApiKey { get; private set; }

	public LoginRequest(string username, string password, string apiKey)
	{
		Username = username;
		Password = password;
		ApiKey = apiKey;
		OS = Environment.OSVersion.VersionString;
		Client = CurrentApplicationHelper.GetName() + " " + CurrentApplicationHelper.GetVersion();
	}
}
