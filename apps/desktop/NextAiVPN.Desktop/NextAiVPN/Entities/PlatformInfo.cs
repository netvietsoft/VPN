using Newtonsoft.Json;

namespace NextAiVPN.Entities;

public class PlatformInfo
{
	[JsonProperty("platform")]
	public string Platform { get; set; }

	[JsonProperty("url")]
	public string Url { get; set; }

	[JsonProperty("update")]
	public string Update { get; set; }

	[JsonProperty("webview2")]
	public string WebView2 { get; set; }

	[JsonProperty("version")]
	public string Version { get; set; }
}
