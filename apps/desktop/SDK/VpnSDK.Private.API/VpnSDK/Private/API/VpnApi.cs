using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HashTableHashing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Polly;
using Polly.Retry;
using RestEase;
using VpnSDK.Common.Settings;
using VpnSDK.Private.API.Common.Enums;
using VpnSDK.Private.API.Common.Helper;
using VpnSDK.Private.API.DTO;
using VpnSDK.Private.API.Utilities;
using VpnSDK.Private.API.Wireguard;
using VpnSDK.Private.API.Wireguard.DTO;

namespace VpnSDK.Private.API;

internal abstract class VpnApi : IDisposable
{
	protected static ILogger _logger;

	private IVpnApi _restService;

	private string _apiKey;

	protected readonly ApiType _apiType;

	private string _brandToken;

	private JsonSerializerSettings _jsonSerializerSettings;

	private uint _lastServerHash;

	private HttpClient _httpClient;

	private Dictionary<string, string> _headers;

	protected HttpAPIHandler _httpApiHandler;

	protected HttpDomainPicker _httpDomainPicker;

	private List<Uri> _baseUrls;

	private Uri _loginUrl;

	private static HttpStatusCode[] httpStatusCodesWorthRetrying = new HttpStatusCode[5]
	{
		HttpStatusCode.RequestTimeout,
		HttpStatusCode.InternalServerError,
		HttpStatusCode.BadGateway,
		HttpStatusCode.ServiceUnavailable,
		HttpStatusCode.GatewayTimeout
	};

	private readonly AsyncRetryPolicy _ipGeoRetryPolicy = Policy.Handle((ApiException ex) => Enumerable.Contains(httpStatusCodesWorthRetrying, ex.StatusCode)).Or<IOException>().Or<SocketException>()
		.Or<HttpRequestException>()
		.WaitAndRetryAsync(new TimeSpan[2]
		{
			TimeSpan.FromSeconds(3.0),
			TimeSpan.FromSeconds(6.0)
		}, delegate(Exception response, TimeSpan delay, int retryCount, Context context)
		{
			_logger?.LogWarning($"GeoAPI request failed. Attempt: {retryCount}, Response: {response.Message}");
		});

	private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle((ApiException ex) => Enumerable.Contains(httpStatusCodesWorthRetrying, ex.StatusCode)).Or<IOException>().Or<SocketException>()
		.Or<HttpRequestException>()
		.WaitAndRetryAsync(new TimeSpan[1] { TimeSpan.FromSeconds(3.0) }, delegate(Exception response, TimeSpan delay, int retryCount, Context context)
		{
			_logger?.LogWarning("API request failed. Response: " + response.Message);
		});

	public bool UseTokenAuthentication { get; set; }

	public event EventHandler<RawApiRequest> OnRequestSend;

	public event EventHandler<RawApiResponse> OnResponseReceived;

	public event EventHandler<HttpError> OnHttpError;

	public event EventHandler<ProxyError> OnProxyError;

	public VpnApi(string apiKey, string brandToken = null, string baseUrl = "https://api.wlvpn.com/v3", TimeSpan timeout = default(TimeSpan), ILoggerFactory loggerFactory = null)
		: this(apiKey, brandToken, timeout, null, loggerFactory, new Uri(baseUrl.TrimEnd('/')))
	{
	}

	public VpnApi(string apiKey, string brandToken, TimeSpan timeout, ILoggerFactory loggerFactory, params string[] baseUrls)
		: this(apiKey, brandToken, timeout, null, loggerFactory, baseUrls.Select((string x) => new Uri(x.TrimEnd('/'))).ToArray())
	{
	}

	public VpnApi(string apiKey, string brandToken, TimeSpan timeout, Uri loginUrl = null, ILoggerFactory loggerFactory = null, params Uri[] baseUrls)
	{
		if (timeout == default(TimeSpan))
		{
			timeout = TimeSpan.FromSeconds(25.0);
		}
		_brandToken = brandToken;
		LogProvider.SetLogFactory(loggerFactory);
		_logger = LogProvider.GetLogger("VpnSDK::Private::API");
		if (string.IsNullOrEmpty(apiKey))
		{
			_apiType = ApiType.WLVPN;
		}
		else if (string.IsNullOrEmpty(brandToken))
		{
			if (apiKey.StartsWith("ZmOLNuJz0"))
			{
				_apiType = ApiType.StrongVPN;
			}
			else if (apiKey.StartsWith("619a91cf2"))
			{
				_apiType = ApiType.IPVanish;
			}
			else if (baseUrls.Any((Uri x) => x.Host.IndexOf("ipvanish", StringComparison.OrdinalIgnoreCase) >= 0))
			{
				_apiType = ApiType.IPVanish;
			}
			else
			{
				_apiType = ApiType.StrongVPN;
			}
		}
		else
		{
			_apiType = ApiType.WLVPN;
		}
		_apiKey = apiKey;
		_baseUrls = baseUrls.ToList();
		_loginUrl = loginUrl;
		_jsonSerializerSettings = new JsonSerializerSettings
		{
			Converters = new JsonConverter[3]
			{
				new IPAddressConverter(),
				new UnixDateTimeConverter(),
				new NullStringConverter()
			}
		};
		_headers = PopulateHeaders(_apiKey);
		_httpApiHandler = new HttpAPIHandler(null, HttpErrorDelegate, RequestDelegate, ResponseDelegate, timeout);
		_httpDomainPicker = new HttpDomainPicker(baseUrls.Select((Uri x) => x.ToString()).ToList(), ProxyErrorDelegate, _headers, _httpApiHandler);
		_httpClient = new HttpClient(_httpDomainPicker, disposeHandler: true)
		{
			BaseAddress = _baseUrls[0]
		};
		_httpClient.DefaultRequestHeaders.ConnectionClose = true;
		foreach (KeyValuePair<string, string> header in _headers)
		{
			_httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
		}
		_restService = new RestClient(_httpClient)
		{
			JsonSerializerSettings = _jsonSerializerSettings
		}.For<IVpnApi>();
	}

	private Dictionary<string, string> PopulateHeaders(string apiKey)
	{
		return new Dictionary<string, string>
		{
			{
				"User-Agent",
				CurrentApplicationHelper.GetName() + "/" + CurrentApplicationHelper.GetVersion() + " (Windows)"
			},
			{ "X-API-Key", apiKey }
		};
	}

	private void ResponseDelegate(RawApiResponse message)
	{
		OnResponseReceived?.Invoke(this, message);
	}

	private void HttpErrorDelegate(HttpError error)
	{
		OnHttpError?.Invoke(this, error);
	}

	private void ProxyErrorDelegate(ProxyError error)
	{
		OnProxyError?.Invoke(this, error);
	}

	private void RequestDelegate(RawApiRequest request)
	{
		OnRequestSend?.Invoke(this, request);
	}

	private void OverrideUrlForRequest(Uri apiUrl)
	{
		_httpDomainPicker.OverrideUrlForRequest = apiUrl;
	}

	public async Task<BrandingInfo> GetBrandingInfo(string slugOrDomain, string brandingKey, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.GetBrandingData(slugOrDomain, brandingKey, cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) ?? throw new VpnApiException(ApiError.Unknown, "Could not retrieve branding information.");
	}

	public void UpdateApiKey(string newApiKey)
	{
		_apiKey = newApiKey;
	}

	public void UpdateAuthSuffix(string suffix)
	{
		_brandToken = suffix;
	}

	public virtual async Task<User> LoginWithAuthTokens(string accessToken, string refreshToken, CancellationToken cancellationToken = default(CancellationToken))
	{
		_lastServerHash = 0u;
		UpdateAccessToken(accessToken);
		User result;
		try
		{
			result = await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.Login(null, cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			ValidateApiResponse(result, "Couldn't get a user object from API.", "LoginWithAuthTokens");
			result.AccessToken = accessToken;
			result.RefreshToken = refreshToken;
		}
		catch (VpnApiException ex)
		{
			if (ex.Error != ApiError.AccessTokenExpired)
			{
				throw ex;
			}
			result = await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.RefreshTokenWithLogin(new TokenRequest(refreshToken), cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			ValidateApiResponse(result, "Couldn't get a user object from API.", "LoginWithAuthTokens");
			UpdateAccessToken(result.AccessToken);
		}
		result.Brand = _brandToken;
		result.Username = result.Email;
		return result;
	}

	public virtual async Task<User> Login(string username, string password, CancellationToken cancellationToken = default(CancellationToken))
	{
		_httpClient.DefaultRequestHeaders.Authorization = null;
		_lastServerHash = 0u;
		LoginRequest loginRequest = new LoginRequest(username, password, _apiKey);
		User user = await _retryPolicy.ExecuteAsync(async delegate(CancellationToken cancelToken)
		{
			if (_loginUrl != null && !string.IsNullOrEmpty(_loginUrl.ToString()))
			{
				_logger?.LogInformation($"Login using specific Url : {_loginUrl}");
				OverrideUrlForRequest(_loginUrl);
				_logger?.LogInformation($"Login using specific Url : {_loginUrl}");
				return await _restService.LoginUsingVpnEndpoint(loginRequest, cancelToken);
			}
			_logger?.LogInformation("Login using login endpoint");
			return await _restService.Login(loginRequest, cancelToken);
		}, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ValidateApiResponse(user, "Couldn't get a user object from API.", "Login");
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken);
		user.Username = user.Email;
		user.Password = password;
		user.Brand = _brandToken;
		user.TokenCreationTime = DateTime.UtcNow;
		return user;
	}

	public virtual async Task<User> RefreshToken(User user, CancellationToken cancellationToken = default(CancellationToken))
	{
		User user2 = ((_apiType == ApiType.StrongVPN) ? (await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.RefreshToken(new TokenRequest(user.RefreshToken), cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) : (await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.RefreshTokenWithLogin(new TokenRequest(user.RefreshToken), cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false)));
		User user3 = user2;
		ValidateApiResponse(user3, "Couldn't refresh the token", "RefreshToken");
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user3.AccessToken);
		user3.Username = user.Username;
		user3.Password = user.Password;
		user3.Brand = _brandToken;
		user3.TokenCreationTime = DateTime.UtcNow;
		return user3;
	}

	public virtual async Task<Network> GetServers(CancellationToken cancellationToken = default(CancellationToken))
	{
		return (await GetServersIfUpdated(cancellationToken, ignoreCache: true).ConfigureAwait(continueOnCapturedContext: false)).Item2;
	}

	public virtual async Task<Tuple<bool, Network>> GetServersIfUpdated(CancellationToken cancellationToken = default(CancellationToken), bool ignoreCache = false)
	{
		string text = await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.GetServers(cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (text.Length < 250)
		{
			JsonResponseResult response = null;
			try
			{
				response = JsonConvert.DeserializeObject<JsonResponseResult>(text);
			}
			catch
			{
			}
			ValidateApiResponse(response, "Unknown.", "GetServersIfUpdated");
		}
		try
		{
			uint num = SuperFastHashInlineBitConverter.Hash(text);
			if (!ignoreCache && num == _lastServerHash)
			{
				return new Tuple<bool, Network>(item1: false, null);
			}
			_lastServerHash = num;
			return new Tuple<bool, Network>(item1: true, WLAPIDeconstructor.Deconstruct(text, _apiType));
		}
		catch (Exception innerException)
		{
			throw new SerializationException("Unable to deserialize API response.", innerException);
		}
	}

	public virtual async Task<GeoIP> GetGeolocation(CancellationToken cancellationToken = default(CancellationToken), bool oneTime = false)
	{
		GeoIP geoIP = ((!oneTime) ? (await _ipGeoRetryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.GetGeoLocation(cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) : (await _restService.GetGeoLocation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)));
		GeoIP geoIP2 = geoIP;
		ValidateApiResponse(geoIP2, "", "GetGeolocation");
		return geoIP2;
	}

	public virtual async Task<string> Ping(CancellationToken cancellationToken = default(CancellationToken))
	{
		string text;
		string urlString;
		switch (_apiType)
		{
		case ApiType.StrongVPN:
			text = await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.GetPing(cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			break;
		case ApiType.WLVPN:
			urlString = _baseUrls.First().ToString();
			text = await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.GetUrl(urlString, cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			break;
		default:
			urlString = _baseUrls.First().ToString();
			if (urlString.Contains("/v"))
			{
				urlString = urlString.Substring(0, urlString.LastIndexOf("/") + 1);
			}
			text = await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.GetUrl(urlString, cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			break;
		}
		try
		{
			JsonResponseResult response = JsonConvert.DeserializeObject<JsonResponseResult>(text);
			ValidateApiResponse(response, "Unknown.", "Ping");
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Error while trying to ping the server");
		}
		return text;
	}

	public void UpdateAccessToken(string accessToken)
	{
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
	}

	public void Dispose()
	{
		_httpClient?.Dispose();
	}

	public virtual async Task<WireguardConfiguration> GetWireGuardConfiguration(CancellationToken cancellationToken, string hostname, string configUuid, string publicKey = null, string privateKey = null, bool allowLan = true)
	{
		var (keyPair, server) = GenerateKeyPairAndFQDN(hostname, privateKey, publicKey);
		ConfigurationResponse configurationResponse;
		try
		{
			configurationResponse = await _restService.GetWireguardConfig(new ConfigurationRequest(server, configUuid, keyPair.Public, allowLan), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex) when (ex is JsonReaderException || ex is JsonSerializationException)
		{
			throw new WireguardApiException("Could not deserialize the response body.", ex);
		}
		if (configurationResponse == null)
		{
			throw new WireguardApiException("The API request returned null response.");
		}
		ValidateApiResponse(configurationResponse, "", "GetWireGuardConfiguration");
		if (!string.IsNullOrEmpty(configurationResponse.Message))
		{
			throw new InvalidOperationException(configurationResponse.Message);
		}
		configurationResponse.Config.Interface.PrivateKey = keyPair.Private;
		return configurationResponse.Config;
	}

	public virtual async Task<WireguardConfiguration> GetWireGuardConfiguration(CancellationToken cancellationToken, string hostname, string configUuid, DoubleHopSettings doubleHopSettings, string publicKey = null, string privateKey = null, bool allowLan = true)
	{
		var (keyPair, _) = GenerateKeyPairAndFQDN(hostname, privateKey, publicKey);
		DoubleHopConfigurationResponse doubleHopConfigurationResponse;
		ConfigurationResponse configurationResponse;
		try
		{
			doubleHopConfigurationResponse = await GetWireGuardDoubleHopConfiguration(cancellationToken, doubleHopSettings, allowLan, keyPair, configUuid).ConfigureAwait(continueOnCapturedContext: false);
			configurationResponse = doubleHopConfigurationResponse?.WireguardConfigResponse;
		}
		catch (Exception ex) when (ex is JsonReaderException || ex is JsonSerializationException)
		{
			throw new WireguardApiException("Could not deserialize the response body.", ex);
		}
		if (doubleHopConfigurationResponse == null)
		{
			throw new WireguardApiException("The API request returned null response.");
		}
		ValidateApiResponse(doubleHopConfigurationResponse, "", "GetWireGuardConfiguration");
		if (!string.IsNullOrEmpty(configurationResponse.Message))
		{
			throw new InvalidOperationException(configurationResponse.Message);
		}
		configurationResponse.Config.Interface.PrivateKey = keyPair.Private;
		return configurationResponse.Config;
	}

	public virtual async Task<DoubleHopConfigurationResponse> GetOpenVPNDoubleHopConfiguration(CancellationToken cancellationToken, DoubleHopSettings doubleHopSettings)
	{
		DoubleHopLocation entry = DoubleHopLocation.Create(doubleHopSettings.EntryCountryCode, doubleHopSettings.EntryCity);
		DoubleHopLocation exit = DoubleHopLocation.Create(doubleHopSettings.ExitCountryCode, doubleHopSettings.ExitCity);
		DoubleHopConfigurationResponse doubleHopConfigurationResponse = await _restService.GetDoubleHopConfig(new DoubleHopConfigurationRequest(entry, exit, doubleHopSettings.Protocol), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ValidateApiResponse(doubleHopConfigurationResponse, "Failed to retrieve double hop configuration for OpenVPN.", "GetOpenVPNDoubleHopConfiguration");
		return doubleHopConfigurationResponse;
	}

	public virtual async Task<List<string>> GetIkev2ProtocolConfig(CancellationToken cancellationToken)
	{
		ProtocolConfigResponse protocolConfigResponse = await _restService.GetProtocolConfig(Protocol.IKEv2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ValidateApiResponse(protocolConfigResponse, "Couldn't get protocol config response.", "GetIkev2ProtocolConfig");
		return protocolConfigResponse.AllowedIps;
	}

	public virtual async Task<NetworkCredentialResponse> GetIKEv2VpnTokenCredentials(string server, CancellationToken cancellationToken)
	{
		NetworkCredentialResponse networkCredentialResponse = await _restService.GetIKEv2VpnTokenCredentials(new SetUpRequest
		{
			Server = server
		}, cancellationToken);
		ValidateApiResponse(networkCredentialResponse, "Couldn't get token credentials for IKEv2 connection.", "GetIKEv2VpnTokenCredentials");
		return networkCredentialResponse;
	}

	public virtual async Task<AccountMetadataResponse> SaveAccountMetadata(Dictionary<string, string> keyValuePairs, CancellationToken cancellationToken = default(CancellationToken))
	{
		AccountMetadataResponse accountMetadataResponse = await _restService.SaveAccountMetadata(new AccountMetadataRequest(keyValuePairs), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ValidateApiResponse(accountMetadataResponse, "Couldn't save account metadata.", "SaveAccountMetadata");
		return accountMetadataResponse;
	}

	public virtual async Task<AccountMetadataResponse> GetAccountMetadata(CancellationToken cancellationToken = default(CancellationToken))
	{
		AccountMetadataResponse accountMetadataResponse = await _restService.GetAccountMetadata(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ValidateApiResponse(accountMetadataResponse, "Couldn't get account metadata.", "GetAccountMetadata");
		return accountMetadataResponse;
	}

	public virtual async Task<bool> Logout(CancellationToken cancellationToken = default(CancellationToken))
	{
		JsonResponseResult jsonResponseResult = await _retryPolicy.ExecuteAsync((CancellationToken cancelToken) => _restService.Logout(cancelToken), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ValidateApiResponse(jsonResponseResult, "Logout API returned null.", "Logout");
		return jsonResponseResult.Success;
	}

	public virtual async Task<AdditionalConfigResponse> GetAdditionalConfig(CancellationToken cancellationToken)
	{
		AdditionalConfigResponse content = (await _restService.GetAdditionalConfig(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetContent();
		ValidateApiResponse(content, "Couldn't get additional config response.", "GetAdditionalConfig");
		return content;
	}

	private (KeyPair keyPair, string FQDN) GenerateKeyPairAndFQDN(string hostname, string private_key, string public_key)
	{
		string item = hostname.Replace(".vpn", string.Empty);
		KeyPair item2 = default(KeyPair);
		if (string.IsNullOrEmpty(private_key) && !string.IsNullOrEmpty(public_key))
		{
			item2.Private = private_key;
			item2.Public = Convert.ToBase64String(Curve25519.GetPublicKey(Curve25519.ClampPrivateKey(Convert.FromBase64String(private_key))));
		}
		else
		{
			if (!string.IsNullOrEmpty(public_key) || !string.IsNullOrEmpty(private_key))
			{
				throw new InvalidOperationException("Can't provide just a public key silly.");
			}
			item2 = KeypairGenerator.GeneratePair();
		}
		return (keyPair: item2, FQDN: item);
	}

	private JsonResponseResult ValidateApiResponse(JsonResponseResult response, string exceptionMessage = "", [CallerMemberName] string methodName = "")
	{
		if (response == null)
		{
			_logger?.LogError("Failed to make the API call using {methodName}.", methodName);
			throw new VpnApiException(ApiError.Unknown, exceptionMessage);
		}
		if (!response.Success)
		{
			_logger?.LogError("Failed to make the API call using {methodName}. Error code: {ErrorCode}. Reason: {Reason}", methodName, response.ErrorCode, response.Reason);
			throw new VpnApiException(ErrorMapper.Map(response.ErrorCode, _apiType), response.Reason);
		}
		return response;
	}

	private async Task<DoubleHopConfigurationResponse> GetWireGuardDoubleHopConfiguration(CancellationToken cancellationToken, DoubleHopSettings doubleHopSettings, bool allowLan, KeyPair keyPair, string config_uuid)
	{
		DoubleHopLocation entry = DoubleHopLocation.Create(doubleHopSettings.EntryCountryCode, doubleHopSettings.EntryCity);
		DoubleHopLocation exit = DoubleHopLocation.Create(doubleHopSettings.ExitCountryCode, doubleHopSettings.ExitCity);
		return await _restService.GetDoubleHopConfig(new DoubleHopConfigurationRequest(entry, exit, doubleHopSettings.Protocol, allowLan, keyPair.Public, config_uuid), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}
}
