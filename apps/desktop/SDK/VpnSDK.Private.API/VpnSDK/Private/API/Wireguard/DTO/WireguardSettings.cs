using System;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.Wireguard.DTO;

[Serializable]
public class WireguardSettings
{
	[JsonProperty("port_start")]
	public ushort PortStart { get; set; }

	[JsonProperty("port_end")]
	public ushort PortEnd { get; set; }
}
