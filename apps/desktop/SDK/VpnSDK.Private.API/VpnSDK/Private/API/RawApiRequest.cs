using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VpnSDK.Private.API;

[Serializable]
internal class RawApiRequest
{
	public string Content { get; private set; }

	public Dictionary<string, string> Headers { get; private set; }

	public string Method { get; private set; }

	public Uri Uri { get; private set; }

	private RawApiRequest()
	{
	}

	internal static async Task<RawApiRequest> Create(HttpRequestMessage message)
	{
		RawApiRequest res = new RawApiRequest
		{
			Headers = message.Headers.ToDictionary<KeyValuePair<string, IEnumerable<string>>, string, string>((KeyValuePair<string, IEnumerable<string>> a) => a.Key, (KeyValuePair<string, IEnumerable<string>> a) => string.Join(";", a.Value)),
			Method = message.Method.Method,
			Uri = message.RequestUri
		};
		if (message.Content != null)
		{
			RawApiRequest rawApiRequest = res;
			rawApiRequest.Content = await message.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		if (res.Content != null && res.Content.StartsWith("{"))
		{
			JObject jObject = JObject.Parse(res.Content);
			if (jObject.ContainsKey("password"))
			{
				jObject["password"] = "HIDDEN";
				res.Content = jObject.ToString();
			}
		}
		return res;
	}
}
