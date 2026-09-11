using Newtonsoft.Json;
using VpnSDK.Private.API.Wireguard.DTO;

namespace VpnSDK.Private.API.DTO;

public class DoubleHopConfigurationRequest
{
	[JsonProperty("entry")]
	public DoubleHopLocation Entry { get; set; }

	[JsonProperty("exit")]
	public DoubleHopLocation Exit { get; set; }

	[JsonProperty("protocol")]
	public string Protocol { get; set; }

	[JsonProperty("wireguard")]
	public BaseRequestProperties Wireguard { get; set; }

	public DoubleHopConfigurationRequest()
	{
	}

	public DoubleHopConfigurationRequest(DoubleHopLocation entry, DoubleHopLocation exit, string protocol, bool allowLan, string publicKey, string uuid)
	{
		Entry = entry;
		Exit = exit;
		Protocol = protocol;
		Wireguard = new BaseRequestProperties(allowLan, publicKey, uuid);
	}

	public DoubleHopConfigurationRequest(DoubleHopLocation entry, DoubleHopLocation exit, string protocol)
	{
		Entry = entry;
		Exit = exit;
		Protocol = protocol;
	}
}
