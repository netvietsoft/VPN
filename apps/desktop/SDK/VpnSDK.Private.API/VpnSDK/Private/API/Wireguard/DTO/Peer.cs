using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace VpnSDK.Private.API.Wireguard.DTO;

[Serializable]
public class Peer
{
	public bool AllowUntunnelledTraffic
	{
		get
		{
			return Enumerable.Contains(AllowedIPs, "0.0.0.0/1");
		}
		set
		{
			AllowedIPs = ((!value) ? new string[1] { "0.0.0.0/0" } : new string[2] { "0.0.0.0/1", "128.0.0.0/1" });
		}
	}

	[ConfigurationEntry]
	[JsonProperty("AllowedIPs")]
	public string[] AllowedIPs { get; set; }

	[ConfigurationEntry]
	[JsonProperty("Endpoint")]
	public string Endpoint { get; set; }

	[ConfigurationEntry]
	[JsonProperty("PublicKey")]
	public string PublicKey { get; set; }

	[ConfigurationEntry]
	public int? PersistentKeepAlive { get; set; }

	[ConfigurationEntry]
	public string PresharedKey { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[Peer]" + Environment.NewLine);
		foreach (PropertyInfo item in from x in typeof(Peer).GetProperties()
			where x.GetCustomAttributes(typeof(ConfigurationEntryAttribute)).Any()
			select x)
		{
			string text = "";
			if (item.PropertyType.IsArray)
			{
				if (!(item.GetValue(this) is Array array))
				{
					continue;
				}
				foreach (object item2 in array)
				{
					text = text + item2.ToString() + ",";
				}
				text = text.TrimEnd(',');
			}
			else
			{
				object value = item.GetValue(this);
				if (value != null)
				{
					text = value.ToString();
				}
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				stringBuilder.AppendLine(item.Name + " = " + text);
			}
		}
		return stringBuilder.ToString();
	}
}
