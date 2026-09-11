using System.Collections.Generic;
using Newtonsoft.Json;

namespace VpnSDK.Private.API;

public class Dns
{
	[JsonProperty("dnsName")]
	public string DnsName { get; set; }

	[JsonProperty("dnsValue")]
	public List<string> DnsValue { get; set; }
}
