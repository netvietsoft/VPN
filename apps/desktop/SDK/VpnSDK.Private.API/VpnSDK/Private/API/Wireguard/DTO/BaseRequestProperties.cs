using Newtonsoft.Json;

namespace VpnSDK.Private.API.Wireguard.DTO;

public class BaseRequestProperties
{
	[JsonProperty("allowlan")]
	public int AllowLan { get; set; }

	[JsonProperty("publickey")]
	public string PublicKey { get; set; }

	[JsonProperty("uuid")]
	public string UUID { get; set; }

	public BaseRequestProperties()
	{
	}

	public BaseRequestProperties(bool allowLan, string publicKey, string uuid)
	{
		AllowLan = (allowLan ? 1 : 0);
		PublicKey = publicKey;
		UUID = uuid;
	}
}
