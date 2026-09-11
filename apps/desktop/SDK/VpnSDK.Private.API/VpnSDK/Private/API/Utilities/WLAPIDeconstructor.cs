using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Private.API.Utilities;

internal static class WLAPIDeconstructor
{
	internal static Network Deconstruct(string jsonText, ApiType apiType)
	{
		List<Location> list = new List<Location>();
		using (JsonReader jsonReader = new JsonTextReader(new StringReader(jsonText)))
		{
			while (jsonReader.Read() && jsonReader.TokenType != JsonToken.StartArray)
			{
			}
			while (jsonReader.Read())
			{
				if (jsonReader.TokenType != JsonToken.StartObject)
				{
					continue;
				}
				JObject json = JObject.Load(jsonReader);
				Location location = list.FirstOrDefault((Location x) => x.Id == json.Value<string>("country"));
				if (location == null)
				{
					location = new Location(json.Value<string>("country"), CountryUtilities.CodeToCountryName(json.Value<string>("country")));
					list.Add(location);
				}
				Location location2 = location.Children?.Cast<Location>().FirstOrDefault((Location x) => x.Id == json.Value<string>("pop"));
				if (location2 == null)
				{
					location2 = new Location(json.Value<string>("pop"), json.Value<string>("city"), json.Value<double>("latitude"), json.Value<double>("longitude"));
					location.AddChild(location2);
				}
				VpnConfiguration vpnConfiguration = new VpnConfiguration();
				if (json.TryGetValue("preshared_key", out JToken value))
				{
					try
					{
						vpnConfiguration.PresharedKey = string.Intern(value.Value<string>());
					}
					catch
					{
					}
				}
				else if (string.IsNullOrEmpty(vpnConfiguration.PresharedKey) && apiType == ApiType.IPVanish)
				{
					vpnConfiguration.PresharedKey = "ipvanish";
				}
				else if (string.IsNullOrEmpty(vpnConfiguration.PresharedKey) && apiType == ApiType.WLVPN)
				{
					vpnConfiguration.PresharedKey = "vpn";
				}
				ScheduledMaintenance scheduledMaintenance = json.Value<JObject>("scheduled_maintenance")?.ToObject<ScheduledMaintenance>();
				string text = json.Value<string>("name");
				if (apiType != ApiType.StrongVPN && !text.Contains(".vpn."))
				{
					string[] array = text.Split('.');
					text = array[0] + ".vpn." + string.Join(".", array.Skip(1).ToArray());
				}
				Server node = Server.Create(text, IPAddress.Parse(json.Value<string>("ip_address")), json.Value<bool>("maintenance"), scheduledMaintenance, json.Value<short>("capacity"), vpnConfiguration);
				location2.AddChild(node);
			}
		}
		Network obj2 = new Network
		{
			Id = "VPN-Network"
		};
		Node[] nodes = list.ToArray();
		obj2.AddChildren(nodes);
		return obj2;
	}
}
