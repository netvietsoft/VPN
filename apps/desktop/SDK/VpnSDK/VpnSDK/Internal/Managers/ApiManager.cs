using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceId;
using Microsoft.Extensions.Logging;
using RestEase;
using VpnSDK.Common.Settings;
using VpnSDK.DTO;
using VpnSDK.Enums;
using VpnSDK.Extensions;
using VpnSDK.Helpers;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Configuration;
using VpnSDK.Internal.Dtos;
using VpnSDK.Internal.Helpers;
using VpnSDK.Private.API;
using VpnSDK.Private.API.DTO;
using VpnSDK.Private.API.Utilities;
using VpnSDK.Private.API.Wireguard;
using VpnSDK.Private.API.Wireguard.DTO;

namespace VpnSDK.Internal.Managers;

internal class ApiManager : IDisposable
{
	private static ApiManager _instance;

	private static string[] _ignoredHeaders = new string[4] { "Location", "Host", "User-Agent", "Authorization" };

	private readonly ILogger _logger;

	private readonly ISDKConfiguration _configuration;

	private readonly VpnApiExtended _vpnApi;

	private string _currentApiUrl = string.Empty;

	private WireguardAPI _wireguardApi;

	private string _machineUserHash;

	public Action RequestSending { get; set; }

	public Action RequestSent { get; set; }

	internal UserProxy User { get; private set; } = new UserProxy(null);

	internal BrandingInfoProxy BrandingInfo { get; private set; } = new BrandingInfoProxy(null);

	internal SmartCollection<ILocation> Locations { get; } = new SmartCollection<ILocation>();

	private string SERVER_CACHE => Path.Combine(_configuration.ServerListCacheDirectory, "cache.dat");

	public event EventHandler<RefreshLocationListStatus> LocationsRefreshStatusChanged;

	public event EventHandler<ProxyError> OnProxyError;

	public event EventHandler<HttpError> OnHttpError;

	public event EventHandler<IUser> OnTokenRefresh;

	private ApiManager(ISDKConfiguration configuration)
	{
		_logger = LogProvider.GetLogger("VpnSDK::API");
		_configuration = configuration;
		if (configuration.ApiBaseUrls.Length == 0)
		{
			string apiKey = configuration.ApiKey;
			string authorizationToken = configuration.AuthorizationToken;
			ILoggerFactory loggerFactoryInstance = LogProvider.LoggerFactoryInstance;
			_vpnApi = new VpnApiExtended(apiKey, authorizationToken, "https://api.wlvpn.com/v3", default(TimeSpan), loggerFactoryInstance);
			_currentApiUrl = "api.wlvpn.com";
		}
		else
		{
			_vpnApi = new VpnApiExtended(configuration.ApiKey, configuration.AuthorizationToken, TimeSpan.FromSeconds(30.0), configuration.LoginApiUrl, LogProvider.LoggerFactoryInstance, configuration.ApiBaseUrls);
			_currentApiUrl = new Uri(configuration.ApiBaseUrls.First()).Host;
		}
		_vpnApi.OnResponseReceived += VpnApiOnOnResponseReceived;
		_vpnApi.OnRequestSend += VpnApiOnOnRequestSend;
		_vpnApi.OnHttpError += OnHttpErrorHandler;
		_vpnApi.OnProxyError += OnProxyErrorHandler;
		_vpnApi.OnTokenRefresh += OnTokenRefreshHandler;
		_vpnApi.UseTokenAuthentication = configuration.UseTokenAuthentication;
	}

	public void ClearUser()
	{
		User.UpdateProxiedUser(null);
	}

	public void Dispose()
	{
		_vpnApi.OnResponseReceived -= VpnApiOnOnResponseReceived;
		_vpnApi.OnRequestSend -= VpnApiOnOnRequestSend;
		_vpnApi.OnHttpError -= OnHttpErrorHandler;
		_vpnApi.OnProxyError -= OnProxyError;
		_vpnApi.Dispose();
	}

	internal static ApiManager GetInstance(ISDKConfiguration configuration)
	{
		return _instance ?? (_instance = new ApiManager(configuration));
	}

	internal void UpdateApiKey(string apiKey)
	{
		_vpnApi.UpdateApiKey(apiKey);
	}

	internal void UpdateAuthSuffix(string authSuffix)
	{
		_vpnApi.UpdateAuthSuffix(authSuffix);
	}

	internal async Task<BrandingInfo> GetBrandingInformation(string slug, string brandingKey)
	{
		try
		{
			BrandingInfo brandingInfo = await _vpnApi.GetBrandingInfo(slug, brandingKey).ConfigureAwait(continueOnCapturedContext: false);
			if (brandingInfo != null)
			{
				BrandingInfo.Update(brandingInfo);
			}
			return brandingInfo;
		}
		catch (Exception)
		{
			_logger?.LogError("Unable to retrieve branding information.");
			throw;
		}
	}

	internal async Task<IUser> Authenticate(Func<Task<User>> authenticate)
	{
		try
		{
			User user = await authenticate().ConfigureAwait(continueOnCapturedContext: false);
			if (user == null)
			{
				throw new NullReferenceException("Could not get user object.");
			}
			_logger?.LogTrace("Login call succeeded.");
			User.UpdateProxiedUser(user);
			if (user.BrandingInfo != null)
			{
				BrandingInfo.Update(user.BrandingInfo);
			}
			return User;
		}
		catch (VpnApiException ex)
		{
			HTTPException ex2 = ex.ToApiException();
			_logger?.LogError(ex, "Unable to authenticate user.");
			throw ex2;
		}
		catch (ApiException ex3)
		{
			LogApiExceptionHeaders(ex3);
			_logger?.LogError(ex3, "Unable to authenticate user.");
			throw new HTTPException(ex3.ReasonPhrase, ex3);
		}
		catch (Exception ex4)
		{
			if (ex4 is HttpRequestException)
			{
				_logger?.LogError(" -> " + (from x in ex4.GetAllExceptions()
					where x != null
					select x.GetType().FullName + ": " + x.Message));
			}
			_logger?.LogError(ex4, "Unable to authenticate user.");
			throw;
		}
	}

	internal Task<IUser> LoginWithAuthTokens(string accessToken, string refreshToken)
	{
		return Authenticate(() => _vpnApi.LoginWithAuthTokens(accessToken, refreshToken));
	}

	internal Task<IUser> Login(string username, string password)
	{
		return Authenticate(() => _vpnApi.Login(username, password));
	}

	internal void DropLocation(ILocation location)
	{
		Locations.Remove(location);
	}

	internal void DropServers(int ignoreNumber = 0)
	{
		if (ignoreNumber == 0)
		{
			Locations.Clear();
			return;
		}
		while (Locations.Count > ignoreNumber)
		{
			Locations.RemoveAt(0);
		}
	}

	internal async Task<ServersLoadedFrom> GetServers()
	{
		if (!User.IsValid)
		{
			throw new NotAuthorizedException("No valid user provided. You must be logged in to fetch server data.");
		}
		RefreshLocationListStatus endStatus = RefreshLocationListStatus.Error;
		try
		{
			OnLocationRefreshStatusChanged(RefreshLocationListStatus.Refreshing);
			Network network = null;
			try
			{
				if (Locations.Count > 1)
				{
					Tuple<bool, Network> tuple = await _vpnApi.GetServersIfUpdated().ConfigureAwait(continueOnCapturedContext: false);
					if (tuple.Item1)
					{
						network = tuple.Item2;
					}
					else
					{
						endStatus = RefreshLocationListStatus.RefreshedWithNoChanges;
					}
				}
				else
				{
					if (Locations.Count <= 1 && File.Exists(SERVER_CACHE) && _configuration.KeepServerCache.HasValue)
					{
						TimeSpan timeSpan = DateTime.Now - File.GetLastWriteTime(SERVER_CACHE);
						if (timeSpan < _configuration.KeepServerCache.Value)
						{
							try
							{
								_logger?.LogWarning("Loading servers from cache. CacheAge={CacheAge}", timeSpan);
								network = JsonAPIDTOSerializer.Deserialize<Network>(SERVER_CACHE);
								UpdateLocationsFromNetwork(network);
								endStatus = RefreshLocationListStatus.RefreshedWithChanges;
								return ServersLoadedFrom.Cache;
							}
							catch (Exception exception)
							{
								try
								{
									File.Delete(SERVER_CACHE);
								}
								catch (Exception exception2)
								{
									_logger?.LogWarning(exception2, "Couldn't delete servers cache file.");
								}
								_logger?.LogError(exception, "Unable to load servers from cache.");
							}
						}
					}
					network = await _vpnApi.GetServers().ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			catch (Exception ex)
			{
				if (ex is HttpRequestException)
				{
					_logger?.LogError(" -> " + (from x in ex.GetAllExceptions()
						where x != null
						select x.GetType().FullName + ": " + x.Message));
				}
				if (ex is ApiException e)
				{
					LogApiExceptionHeaders(e);
				}
				_logger?.LogError(ex, "Unable to retrieve servers.");
				if (ex is VpnApiException e2)
				{
					throw e2.ToApiException();
				}
				if (ex is ApiException ex2)
				{
					LogApiExceptionHeaders(ex2);
					throw new HTTPException(ex2.ReasonPhrase, ex2);
				}
				throw;
			}
			if (network != null)
			{
				if (_configuration.KeepServerCache.HasValue && !string.IsNullOrEmpty(SERVER_CACHE))
				{
					try
					{
						Directory.CreateDirectory(Path.GetDirectoryName(SERVER_CACHE));
						File.Delete(SERVER_CACHE);
						JsonAPIDTOSerializer.Serialize(network, SERVER_CACHE);
					}
					catch (Exception exception3)
					{
						File.Delete(SERVER_CACHE);
						_logger?.LogWarning(exception3, "Unable to cache servers.");
					}
				}
				UpdateLocationsFromNetwork(network);
				endStatus = RefreshLocationListStatus.RefreshedWithChanges;
			}
			else
			{
				endStatus = RefreshLocationListStatus.RefreshedWithNoChanges;
			}
			return ServersLoadedFrom.Api;
		}
		finally
		{
			OnLocationRefreshStatusChanged(endStatus);
		}
	}

	internal async Task<NetworkGeolocation> GetCurrentPosition(CancellationToken token, bool oneTime = false)
	{
		try
		{
			return await _vpnApi.GetGeolocation(token, oneTime).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (VpnApiException e)
		{
			HTTPException ex = e.ToApiException();
			_logger?.LogError("Unable to get current position");
			throw ex;
		}
	}

	internal async Task RefreshToken(CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			await _vpnApi.RefreshToken(User.User, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (ApiException ex)
		{
			LogApiExceptionHeaders(ex);
			throw new HTTPException(ex.ReasonPhrase, ex);
		}
		catch (VpnApiException e)
		{
			HTTPException ex2 = e.ToApiException();
			_logger?.LogError("Unable to refresh user token.");
			throw ex2;
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Unable to refresh user token.");
			throw;
		}
	}

	internal async Task<VpnSDK.Private.API.Wireguard.DTO.WireguardConfiguration> GenerateWireGuardConfiguration(CancellationToken token, Server server, bool allowLan = true)
	{
		GenerateMachineUserHash(server);
		try
		{
			if (_configuration.WireguardConfiguration is WireguardConfigurationStandalone wireguardConfigurationStandalone)
			{
				if (_wireguardApi == null && !string.IsNullOrEmpty(wireguardConfigurationStandalone.ApiKey))
				{
					if (wireguardConfigurationStandalone.ApiBaseUrls.Count() == 0)
					{
						throw new InvalidOperationException("WireGuard configuration is missing API base URL.");
					}
					_wireguardApi = new WireguardAPI(wireguardConfigurationStandalone.ApiKey, wireguardConfigurationStandalone.ApiBaseUrls);
				}
				return await _wireguardApi.GenerateConfiguration(token, User.VpnCredential, server.Hostname, _machineUserHash, null, null, allowLan).ConfigureAwait(continueOnCapturedContext: false);
			}
			return await _vpnApi.GetWireGuardConfiguration(token, server.Hostname, _machineUserHash, null, null, allowLan).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (VpnApiException e)
		{
			HTTPException ex = e.ToApiException();
			_logger?.LogError("Unable to get Wireguard configuration");
			throw ex;
		}
	}

	internal async Task<VpnSDK.Private.API.Wireguard.DTO.WireguardConfiguration> GenerateWireGuardConfiguration(CancellationToken token, Server server, DoubleHopSettings doubleHopSettings, bool allowLan = true)
	{
		GenerateMachineUserHash(server);
		try
		{
			return await _vpnApi.GetWireGuardConfiguration(token, server.Hostname, _machineUserHash, doubleHopSettings, null, null, allowLan).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (VpnApiException e)
		{
			HTTPException ex = e.ToApiException();
			_logger?.LogError("Unable to generate the WireGuard DoubleHop configuration");
			throw ex;
		}
	}

	private void GenerateMachineUserHash(Server server)
	{
		if (_machineUserHash == null)
		{
			using (SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider())
			{
				DeviceIdBuilder deviceIdBuilder = new DeviceIdBuilder().AddUserName();
				new WindowsDeviceIdBuilder(deviceIdBuilder).AddMotherboardSerialNumber();
				byte[] array = sHA1CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes($"{deviceIdBuilder}WIN-{User.EmailAddress}{server.Hostname}"));
				Array.Resize(ref array, 16);
				_machineUserHash = new Guid(array).ToString().ToUpper();
			}
		}
	}

	internal async Task<DoubleHopConfigurationResponse> GetOpenVPNDoubleHopConfiguration(CancellationToken token, DoubleHopSettings doubleHopSettings)
	{
		try
		{
			return await _vpnApi.GetOpenVPNDoubleHopConfiguration(token, doubleHopSettings).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (VpnApiException e)
		{
			HTTPException ex = e.ToApiException();
			_logger?.LogError("Unable to generate the OpenVPN DoubleHop configuration");
			throw ex;
		}
	}

	internal async Task<List<string>> GetIkev2ProtocolConfig(CancellationToken cancellationToken)
	{
		try
		{
			return await _vpnApi.GetIkev2ProtocolConfig(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (VpnApiException e)
		{
			HTTPException ex = e.ToApiException();
			_logger?.LogError("Unable to get Ikev2 protocol configuration");
			throw ex;
		}
	}

	internal async Task<NetworkCredential> GetIKEv2VpnTokenCredentials(string server, CancellationToken cancellationToken)
	{
		try
		{
			return (await _vpnApi.GetIKEv2VpnTokenCredentials(server, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ToNetworkCredential();
		}
		catch (VpnApiException e)
		{
			HTTPException ex = e.ToApiException();
			_logger?.LogError("Unable to get token credentials for Ikev2 connection.");
			throw ex;
		}
	}

	internal async Task<List<VpnSDK.Private.API.Dns>> GetDnsConfigForThreatProtection(CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			return (await _vpnApi.GetAdditionalConfig(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))?.Dns;
		}
		catch (VpnApiException e)
		{
			HTTPException ex = e.ToApiException();
			_logger?.LogError("Unable to get dns config for threat protection.");
			throw ex;
		}
	}

	internal void UpdateLocationsFromNetwork(Network network)
	{
		bool flag = false;
		if (Locations.OfType<BestAvailable>().Any())
		{
			flag = true;
		}
		List<Location> list = network.GetChildren<Location>().SelectMany((Location x) => x.GetChildren<Location>()).ToList();
		List<Location> activeCities = new List<Location>();
		foreach (Location item in list)
		{
			List<Server> list2 = item.Children?.OfType<Server>().Where((Server sr) => sr.InMaintenance).ToList();
			if (list2.Count > 0)
			{
				foreach (Server item2 in list2)
				{
					item.Children.Remove(item2);
				}
			}
			List<Node> children = item.Children;
			if (children != null && children.OfType<Server>().ToList().Count > 0)
			{
				activeCities.Add(item);
			}
		}
		List<ILocation> list3 = new List<ILocation>();
		if (Locations.Count > 1)
		{
			list3 = Locations.Where((ILocation location) => activeCities.All((Location location2) => !location2.Id.Equals(location.Id)) && !(location is BestAvailable)).ToList();
			foreach (ILocation item3 in list3)
			{
				(item3 as RegionProxy)?.UpdateProxiedObject(null);
			}
			Locations.RemoveRange(list3);
			list3.Clear();
			if (Debugger.IsAttached && list3.Count > 0)
			{
				_logger?.LogTrace("Regions removed: " + string.Join(",", list3.Select((ILocation x) => x.Id)));
			}
		}
		List<ILocation> list4 = new List<ILocation>();
		foreach (Location activeCity in activeCities)
		{
			List<Node> children2 = activeCity.Children;
			if (children2 != null && children2.OfType<Server>().Count((Server x) => !x.InMaintenance) > 0)
			{
				RegionProxy regionProxy = Locations.OfType<RegionProxy>().FirstOrDefault((RegionProxy x) => x.Id == activeCity.Id);
				if (regionProxy == null)
				{
					list4.Add(new RegionProxy(activeCity));
				}
				else
				{
					regionProxy.UpdateProxiedObject(activeCity);
				}
			}
		}
		if (!flag)
		{
			list4.Insert(0, new BestAvailable());
		}
		Locations.AddRange(list4);
	}

	internal async Task<bool> SaveAccountMetadata(Dictionary<string, string> keyValuePairs)
	{
		try
		{
			return (await _vpnApi.SaveAccountMetadata(keyValuePairs).ConfigureAwait(continueOnCapturedContext: false)).Success;
		}
		catch (VpnApiException ex)
		{
			_logger?.LogError("Error while saving account metadata.");
			if (ex.Error == ApiError.InvalidMetadata)
			{
				throw new AccountMetadataException("Invalid metadata", ex);
			}
			throw ex.ToApiException();
		}
	}

	internal async Task<Dictionary<string, string>> GetAccountMetadata()
	{
		try
		{
			return (await _vpnApi.GetAccountMetadata().ConfigureAwait(continueOnCapturedContext: false))?.Metadata;
		}
		catch (VpnApiException ex)
		{
			_logger?.LogError("Unable to get account metadata.");
			if (ex.Error == ApiError.InvalidMetadata)
			{
				throw new AccountMetadataException("Invalid metadata", ex);
			}
			throw ex.ToApiException();
		}
	}

	internal async Task EnsureValidAccount(CancellationToken cancellationToken)
	{
		try
		{
			AccountMetadataResponse accountMetadataResponse = await _vpnApi.GetAccountMetadata(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (!accountMetadataResponse.AccountType.Equals(1))
			{
				throw new InvalidAccountException($"Invalid Account: The account status is {(AccountStatus)accountMetadataResponse.AccountType}", (AccountStatus)accountMetadataResponse.AccountType);
			}
		}
		catch (VpnApiException e)
		{
			_logger?.LogError("Unable to get account details.");
			throw e.ToApiException();
		}
	}

	internal async Task Logout()
	{
		try
		{
			await _vpnApi.Logout().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (VpnApiException e)
		{
			HTTPException ex = e.ToApiException();
			_logger?.LogError("Unable to logout the user");
			throw ex;
		}
	}

	internal void SetApiTimeout(int timeoutInSeconds)
	{
		_vpnApi.SetApiTimeout(TimeSpan.FromSeconds((double)timeoutInSeconds));
	}

	internal void SetTokenExpire(DateTime expireDateTime)
	{
		_vpnApi.SetTokenExpire(expireDateTime);
	}

	internal bool IsUsingTokenAuthentication()
	{
		return _configuration.UseTokenAuthentication;
	}

	private void OnLocationRefreshStatusChanged(RefreshLocationListStatus status)
	{
		_logger?.LogTrace("RefreshLocationListStatus={RefreshLocationListStatus}", status);
		LocationsRefreshStatusChanged?.Invoke(this, status);
	}

	private void VpnApiOnOnResponseReceived(object sender, RawApiResponse e)
	{
		if (Debugger.IsAttached && e.Headers != null)
		{
			foreach (string item in e.Headers.Select((KeyValuePair<string, string> x) => $"{x.Key}: {x.Value}"))
			{
				_ = item;
			}
		}
		if (e.Request.Uri.Host != _currentApiUrl && !e.Request.Uri.Host.StartsWith("ipgeo."))
		{
			_currentApiUrl = e.Request.Uri.Host;
			_logger?.LogInformation("API server changed to {Mirror}.", HashHelper.SHA256HashString(e.Request.Uri.Host, trimHash: true));
		}
		if (!e.Success)
		{
			_logger?.LogWarning("API {Method} request on server {Mirror} failed after {ResponseTime}ms. Status Code={StatusCode}", e.Request.Uri.Segments.Last().TrimEnd(new char[1] { '/' }), HashHelper.SHA256HashString(e.Request.Uri.Host, trimHash: true), e.ResponseTime.TotalMilliseconds, e.StatusCode);
		}
		RequestSent?.Invoke();
	}

	private void VpnApiOnOnRequestSend(object sender, RawApiRequest e)
	{
		_ = Debugger.IsAttached;
		RequestSending?.Invoke();
	}

	private void OnProxyErrorHandler(object sender, ProxyError e)
	{
		OnProxyError?.Invoke(this, e);
	}

	private void OnHttpErrorHandler(object sender, HttpError e)
	{
		OnHttpError?.Invoke(this, e);
	}

	private void OnTokenRefreshHandler(object sender, User user)
	{
		User.UpdateProxiedUser(user);
		OnTokenRefresh?.Invoke(this, User);
	}

	private void LogApiExceptionHeaders(ApiException e)
	{
		if (!e.Headers.Any())
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder("Response Headers:");
		foreach (KeyValuePair<string, IEnumerable<string>> header in e.Headers)
		{
			if (!Enumerable.Contains(_ignoredHeaders, header.Key))
			{
				stringBuilder.AppendLine($"{header.Key}: {header.Value}");
			}
		}
		_logger?.LogInformation(stringBuilder.ToString());
	}
}
