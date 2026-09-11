using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestEase;
using VpnSDK.Private.API.Utilities;
using VpnSDK.Private.API.Wireguard.DTO;

namespace VpnSDK.Private.API.Wireguard;

internal class WireguardAPI
{
	private string _workingApiUrl;

	private IWGAPI API;

	private string ApiKey;

	public WireguardAPI(string apiKey, string baseUrl)
		: this(apiKey, new string[1] { baseUrl })
	{
	}

	public WireguardAPI(string apiKey, string[] baseUrls)
	{
		WireguardAPI wireguardAPI = this;
		RequestModifier requestModifier = async delegate(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(wireguardAPI._workingApiUrl))
			{
				foreach (string baseUrl in baseUrls.ToList())
				{
					if ((await Ping(baseUrl + "/ping").ConfigureAwait(continueOnCapturedContext: false)).Item1)
					{
						wireguardAPI._workingApiUrl = baseUrl;
						break;
					}
				}
				if (string.IsNullOrEmpty(wireguardAPI._workingApiUrl))
				{
					throw new InvalidOperationException("Could not get any response from API.");
				}
			}
			request.RequestUri = new Uri(wireguardAPI._workingApiUrl + request.RequestUri.AbsolutePath);
		};
		ApiKey = apiKey;
		API = RestClient.For<IWGAPI>(baseUrls[0], requestModifier);
		API.UserAgent = CurrentApplicationHelper.GetName() + "/" + CurrentApplicationHelper.GetVersion() + " (Windows)";
	}

	public Task<WireguardConfiguration> GenerateConfiguration(CancellationToken token, NetworkCredential aioCredential, string server, string config_uuid, string public_key = null, string private_key = null, bool allowLan = true)
	{
		return GenerateConfiguration(token, aioCredential.UserName, aioCredential.Password, server, config_uuid, public_key, private_key, allowLan);
	}

	public async Task<WireguardConfiguration> GenerateConfiguration(CancellationToken token, string aiouser, string aiopass, string server, string config_uuid, string public_key = null, string private_key = null, bool allowLan = true)
	{
		KeyPair keyPair = default(KeyPair);
		if (string.IsNullOrEmpty(private_key) && !string.IsNullOrEmpty(public_key))
		{
			keyPair.Private = private_key;
			keyPair.Public = Convert.ToBase64String(Curve25519.GetPublicKey(Curve25519.ClampPrivateKey(Convert.FromBase64String(private_key))));
		}
		else
		{
			if (!string.IsNullOrEmpty(public_key) || !string.IsNullOrEmpty(private_key))
			{
				throw new WireguardApiException("WireGuard keys missing.");
			}
			keyPair = KeypairGenerator.GeneratePair();
		}
		Response<ConfigurationResponse> response = await API.GenerateAsync(new ConfigurationRequest(aiouser, aiopass, server, config_uuid, keyPair.Public, ApiKey, allowLan), token).ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			ConfigurationResponse content = response.GetContent();
			if (!string.IsNullOrEmpty(content.Message))
			{
				throw new WireguardApiException(content.Message, response.ResponseMessage);
			}
			if (!response.ResponseMessage.IsSuccessStatusCode)
			{
				throw new WireguardApiException($"Could not get WireGuard configuration. HTTP status code: {response.ResponseMessage.StatusCode}", response.ResponseMessage);
			}
			content.Config.Interface.PrivateKey = keyPair.Private;
			return content.Config;
		}
		catch (Exception ex) when (ex is JsonReaderException || ex is JsonSerializationException)
		{
			throw new WireguardApiException("Could not deserialize the response body.", ex);
		}
	}

	public void OverrideApiUrl(string newApiUrl)
	{
		_workingApiUrl = newApiUrl;
	}

	public static async Task<(bool isSuccessful, HttpResponseMessage response)> Ping(string url)
	{
		HttpResponseMessage response = null;
		using HttpClient http = new HttpClient();
		_ = 1;
		try
		{
			http.Timeout = TimeSpan.FromSeconds(10.0);
			response = await http.GetAsync(url);
			string text = await response.Content.ReadAsStringAsync();
			if (response.StatusCode != HttpStatusCode.OK || text == null || !text.Contains("\"success\": true"))
			{
				return (isSuccessful: false, response: response);
			}
			return (isSuccessful: true, response: response);
		}
		catch
		{
			return (isSuccessful: false, response: response);
		}
	}
}
