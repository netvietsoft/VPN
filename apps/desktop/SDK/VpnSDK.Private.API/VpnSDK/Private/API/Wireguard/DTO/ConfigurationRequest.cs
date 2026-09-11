using Newtonsoft.Json;

namespace VpnSDK.Private.API.Wireguard.DTO;

public class ConfigurationRequest : BaseRequestProperties
{
	[JsonProperty("aiouser", NullValueHandling = NullValueHandling.Ignore)]
	public string AIOUser { get; set; }

	[JsonProperty("aiopass", NullValueHandling = NullValueHandling.Ignore)]
	public string AIOPass { get; set; }

	[JsonProperty("server")]
	public string Server { get; set; }

	[JsonProperty("apikey", NullValueHandling = NullValueHandling.Ignore)]
	public string APIKey { get; set; }

	public ConfigurationRequest()
	{
	}

	public ConfigurationRequest(string aiouser, string aiopass, string server, string config_uuid, string public_key, string api_key, bool allowlan)
	{
		AIOUser = aiouser;
		AIOPass = aiopass;
		Server = server;
		base.UUID = config_uuid;
		base.PublicKey = public_key;
		APIKey = api_key;
		base.AllowLan = (allowlan ? 1 : 0);
	}

	public ConfigurationRequest(string server, string config_uuid, string public_key, bool allowlan)
	{
		Server = server;
		base.UUID = config_uuid;
		base.PublicKey = public_key;
		base.AllowLan = (allowlan ? 1 : 0);
	}
}
