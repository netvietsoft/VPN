using Newtonsoft.Json;
using VpnSDK.Private.API.Wireguard.DTO;

namespace VpnSDK.Private.API.DTO;

public class DoubleHopConfigurationResponse : JsonResponseResult
{
	[JsonProperty("entry_server")]
	public string EntryServer { get; set; }

	[JsonProperty("exit_server")]
	public string ExitServer { get; set; }

	[JsonProperty("port")]
	public int Port { get; set; }

	[JsonProperty("wireguard")]
	public ConfigurationResponse WireguardConfigResponse { get; set; }
}
