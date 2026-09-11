using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VpnSDK.Private.API.DTO;

public class BrandingInfo
{
	[JsonProperty("app_name")]
	public string AppName { get; set; }

	[JsonProperty("app_key")]
	public string ApiKey { get; set; }

	[JsonProperty("slug")]
	public string Slug { get; set; }

	[JsonProperty("last_updated_at")]
	[JsonConverter(typeof(UnixDateTimeConverter))]
	public DateTime LastUpdatedAt { get; set; }

	[JsonProperty("urls")]
	public Dictionary<string, string> Urls { get; set; }

	[JsonProperty("colors")]
	public Dictionary<string, string> Colors { get; set; }
}
