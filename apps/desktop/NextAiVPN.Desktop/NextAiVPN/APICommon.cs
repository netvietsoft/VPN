using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using Newtonsoft.Json;
using RestSharp;

namespace NextAiVPN;

public class APICommon : IApiClient
{
	private readonly AccessTokenGenerator _accessTokenGenerator;

	private readonly IBugsnagService _bugsnagService;

	private readonly IPreferencesRepository _preferencesRepository;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly IAnalyticsService _analyticsService;

	private readonly ITokenRefreshHandler _tokenRefreshHandler;

	private readonly RestClient _restClient;

	private readonly string _userAgent;

	private const int NextAiGlobalRefreshTokenBuffer = 250;

	public APICommon(AccessTokenGenerator accessTokenGenerator, IBugsnagService bugsnagService, IPreferencesRepository preferencesRepository, IAppSettingsHelper appSettingsHelper, IAppLogger logger, IAnalyticsService analyticsService, ITokenRefreshHandler tokenRefreshHandler)
	{
		_accessTokenGenerator = accessTokenGenerator;
		_bugsnagService = bugsnagService;
		_preferencesRepository = preferencesRepository;
		_appSettingsHelper = appSettingsHelper;
		_logger = logger;
		_analyticsService = analyticsService;
		_tokenRefreshHandler = tokenRefreshHandler;
		_userAgent = SystemInfo.UserAgent;
		_restClient = RestClientFactory.Create(new RestClientOptions
		{
			UserAgent = _userAgent
		});
	}

	public async Task<NextAiTechnologyAuthResponse> GetAuthData(string code)
	{
		_ = 1;
		try
		{
			RestRequest request = new RestRequest(ApiEndpoints.ExchangeEndPoint, Method.Post).AddParameter("code", code, ParameterType.GetOrPost).AddHeader("Accept", "application/json");
			RestResponse<NextAiTechnologyAuthResponse> response = await _restClient.ExecuteAsync<NextAiTechnologyAuthResponse>(request);
			if (response.Data != null && response.StatusCode == HttpStatusCode.OK)
			{
				await SaveSettings(response.Data);
			}
			return response.Data;
		}
		catch (Exception ex)
		{
			LogError("[APICommon GetTokenJson] - " + ex.Message);
			return null;
		}
	}

	public async Task<string> GetLatestVersionNumber()
	{
		try
		{
			RestRequest request = new RestRequest(ApiEndpoints.CheckVersionNumber);
			string callTime = DateTime.Now.ToString("G");
			RestResponse<List<PlatformInfo>> restResponse = await _restClient.ExecuteAsync<List<PlatformInfo>>(request);
			if (restResponse.Data == null)
			{
				_logger?.Information("API call time " + ApiEndpoints.CheckVersionNumber + " - " + callTime, "GetLatestVersionNumber", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 89);
				LogError($"[APICommon GetVersionNumber] - empty response (StatusCode: {restResponse.StatusCode})");
				return null;
			}
			string appVersion = SystemInfo.AppVersion;
			foreach (PlatformInfo datum in restResponse.Data)
			{
				if (datum.Platform != "windows")
				{
					continue;
				}
				string version = datum.Version;
				if (!string.IsNullOrEmpty(version))
				{
					Version version2 = new Version(appVersion);
					Version value = new Version(version);
					if (version2.CompareTo(value) < 0)
					{
						return datum.Version;
					}
				}
			}
		}
		catch (Exception exception)
		{
			_logger.Error(exception, "GetLatestVersionNumber", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 115);
			return "Error";
		}
		return null;
	}

	public async Task<string> GetWebView2InstallerLink()
	{
		string webView2InstallerLink = "";
		try
		{
			RestRequest request = new RestRequest(ApiEndpoints.CheckVersionNumber);
			foreach (PlatformInfo datum in (await _restClient.ExecuteAsync<List<PlatformInfo>>(request)).Data)
			{
				if (datum.Platform == "windows" && !string.IsNullOrEmpty(datum.Version))
				{
					webView2InstallerLink = datum.WebView2;
					break;
				}
			}
		}
		catch (Exception ex)
		{
			LogError("[APICommon GetWebView2InstallerLink] - " + ex.Message);
			return "Error";
		}
		if (string.IsNullOrEmpty(webView2InstallerLink))
		{
			_logger.Error("Can't get edge webview2 runtime installer link", "GetWebView2InstallerLink", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 149);
		}
		return webView2InstallerLink;
	}

	public string GetNextAiVpnVersionLink()
	{
		string result = "";
		try
		{
			RestRequest request = new RestRequest(ApiEndpoints.CheckVersionNumber);
			foreach (PlatformInfo item in JsonConvert.DeserializeObject<List<PlatformInfo>>(_restClient.Execute(request).Content))
			{
				if (item.Platform == "windows")
				{
					result = item.Url;
					break;
				}
			}
		}
		catch (Exception ex)
		{
			LogError("[APICommon GetNextAiVPNLinkError] - " + ex.Message);
			return "error";
		}
		return result;
	}

	public async Task SaveSettings(NextAiTechnologyAuthResponse responseData)
	{
		_ = 3;
		try
		{
			if (responseData != null)
			{
				_appSettingsHelper.SetValue("nickname", responseData.Nickname);
				_appSettingsHelper.SetValue("access_token", responseData.AccessToken);
				_appSettingsHelper.SetValue("id_token", responseData.IdToken);
				_appSettingsHelper.SetValue("expires_in", responseData.ExpiresIn);
				_appSettingsHelper.SetValue("token_type", responseData.TokenType);
				_appSettingsHelper.SetValue("refresh_token", responseData.RefreshToken);
				_appSettingsHelper.SetValue("expires_at", responseData.ExpiresAt);
				_appSettingsHelper.SetValue("nextaivpn_id", responseData.NextAiVpnId);
				_appSettingsHelper.SetValue("subscription_type", responseData.SubscriptionType);
				if (await _preferencesRepository.UserExists())
				{
					await _preferencesRepository.UpdateSignInPrimaryInfo();
					await _preferencesRepository.LoadUserPreferencesToConfig(_appSettingsHelper.GetValue("nickname"), isSignIn: true);
				}
				else
				{
					await _preferencesRepository.SaveSignInPrimaryInfo();
				}
			}
		}
		catch (Exception ex)
		{
			LogError("[APICommon SaveSettings] - " + ex.Message);
		}
	}

	public void RefreshTokensIfNeeded(RestResponse response)
	{
		_tokenRefreshHandler.RefreshTokensIfNeeded(response);
	}

	public async Task RefreshNextAiGlobalTokensIfNeeded()
	{
		try
		{
			string value = _appSettingsHelper.GetValue("LastRefreshTokenDate");
			if (!string.IsNullOrEmpty(value))
			{
				DateTime dateTime = DateTime.Parse(value, CultureInfo.InvariantCulture);
				string text = _appSettingsHelper.GetValue("expires_in");
				if (string.IsNullOrEmpty(text))
				{
					text = "0";
				}
				int num = int.Parse(text, CultureInfo.InvariantCulture);
				if (num <= 250 || dateTime.AddSeconds(num - 250) < DateTime.Now)
				{
					await _accessTokenGenerator.RefreshToken();
				}
			}
		}
		catch (Exception ex)
		{
			_logger.Error(ex.Message, "RefreshNextAiGlobalTokensIfNeeded", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 259);
		}
	}

	public async Task<string> GetSubscriptionStatus()
	{
		_ = 1;
		try
		{
			RestRequest request = new RestRequest(ApiEndpoints.CheckSubscriptionEndPoint, Method.Post).AddHeader("X-nc-User", _appSettingsHelper.GetValue("nickname")).AddHeader("X-NAMP-Token", _appSettingsHelper.GetValue("access_token")).AddHeader("X-NAMP-Refresh-Token", _appSettingsHelper.GetValue("refresh_token"))
				.AddHeader("X-NAMP-Expires-At", _appSettingsHelper.GetValue("expires_at"))
				.AddHeader("Accept", "application/json")
				.AddHeader("Accept-Language", RequestContext.BuildAcceptLanguage());
			RestResponse<CheckResponse> restResponse = await _restClient.ExecuteAsync<CheckResponse>(request);
			if (restResponse == null)
			{
				LogError("[APICommon GetSubscriptionStatus] - response is null");
				return "Error";
			}
			RefreshTokensIfNeeded(restResponse);
			try
			{
				HttpStatusCode statusCode = restResponse.StatusCode;
				if (restResponse.Data == null)
				{
					return "1";
				}
				if (restResponse.Data.Valid == "1")
				{
					return restResponse.Data.Valid;
				}
				switch (statusCode)
				{
				case HttpStatusCode.InternalServerError:
					LogResponseFromTheServer(statusCode, restResponse.Content);
					return "500";
				case HttpStatusCode.BadRequest:
					LogResponseFromTheServer(statusCode, restResponse.Content);
					return "400";
				case HttpStatusCode.OK:
					if (restResponse.Data.Valid == "0")
					{
						if (restResponse.Data.UserMessage.Contains("no account found for this access token. please relogin"))
						{
							LogResponseFromTheServer(statusCode, restResponse.Content);
							_bugsnagService.Notify("Relogin - No account found for this access token.");
							await _analyticsService.SendNotification("Relogin Requested");
							return "Relogin";
						}
						LogResponseFromTheServer(statusCode, restResponse.Content);
						return "NoSubscription";
					}
					break;
				}
				LogError($"[Subscription Relogin Issue]: {statusCode}.\nResponse.Data:{restResponse.Content}");
				return "1";
			}
			catch (Exception ex)
			{
				if (!ex.Message.Contains("Object reference not set to an instance of an object."))
				{
					LogError("[APICommon GetSubscriptionStatus] - Unable to parse valid value, access granted for user [" + _appSettingsHelper.GetValue("nickname") + "]");
				}
				return "1";
			}
		}
		catch (Exception ex2)
		{
			LogError("[APICommon GetSubscriptionStatus] - " + ex2.Message);
			return "Error";
		}
	}

	public void LogOut(SDKMonitor sdk)
	{
		try
		{
			GlobalEvents.RaiseLoggedOut();
			VpnCredentials vpnCredentials = sdk?.CredentialStore?.GetCredentials();
			sdk?.CredentialStore?.DeleteCredentials();
			_preferencesRepository.SetUserActiveStatus(0);
			_preferencesRepository.SaveUserPreferences(_appSettingsHelper.GetValue("nickname"));
			_appSettingsHelper.SetValue("nickname", string.Empty);
			_appSettingsHelper.SetValue("access_token", string.Empty);
			_appSettingsHelper.SetValue("id_token", string.Empty);
			_appSettingsHelper.SetValue("expires_in", string.Empty);
			_appSettingsHelper.SetValue("token_type", string.Empty);
			_appSettingsHelper.SetValue("refresh_token", string.Empty);
			_appSettingsHelper.SetValue("expires_at", string.Empty);
			_appSettingsHelper.SetValue("nextaivpn_id", string.Empty);
			_appSettingsHelper.SetValue("subscription_type", string.Empty);
			_appSettingsHelper.SetValue("protocol", "WireGuard");
			_appSettingsHelper.SetValue("protocolType", string.Empty);
			_appSettingsHelper.SetValue("scramble", "0");
			_appSettingsHelper.SetValue("ConnectedTo", string.Empty);
			_appSettingsHelper.SetValue("LastVersionCheck", string.Empty);
			_appSettingsHelper.SetValue("FavoritesList", string.Empty);
			_appSettingsHelper.SetValue("LastFeedbackSent", string.Empty);
			_appSettingsHelper.SetValue("LastFeedbackClosed", string.Empty);
			_appSettingsHelper.SetValue("LocalEnabled", "0");
			_appSettingsHelper.SetValue("IsBestAvailable", "0");
			_appSettingsHelper.SetValue("Startup", "1");
			_appSettingsHelper.SetValue("AutoConnect", "0");
			_appSettingsHelper.SetValue("KillSwitch", "0");
			_appSettingsHelper.SetValue("BlockLAN", "0");
			_appSettingsHelper.SetValue("IsPurchaseStarted", "0");
			_appSettingsHelper.SetValue("ThemeAppearance", string.Empty);
			_appSettingsHelper.SetValue("IsNeedSendAutoProtectNotification", "0");
			_appSettingsHelper.SetValue(AppSettingsKeys.SplitTunnelingHostnameIpsList, string.Empty);
			_appSettingsHelper.SetValue(AppSettingsKeys.SplitTunnelingAppList, string.Empty);
			_appSettingsHelper.SetValue("IsLoggedIn", "0");
			_appSettingsHelper.SetValue("NextAiGlobalUserId", string.Empty);
			_appSettingsHelper.SetValue("CodeVerifier", string.Empty);
			_appSettingsHelper.SetValue(AppSettingsKeys.IsLanAllowed, "0");
			_appSettingsHelper.SetValue("IsIPv6LeakProtection", "0");
			_appSettingsHelper.SetValue("IsDnsLeakProtection", "0");
			_appSettingsHelper.SetValue("IsDnsMonitoring", "0");
			_appSettingsHelper.SetValue("IsTrafficOptimizer", "0");
			_appSettingsHelper.SetValue("TrafficOptimizerAppList", string.Empty);
			_appSettingsHelper.SetValue("IsSplitTunnelingEnabled", "0");
			_appSettingsHelper.SetValue("IsAdBlocker", "0");
			_appSettingsHelper.SetValue("IsTrialFirstWindowShowed", "0");
			_appSettingsHelper.SetValue("StreamingSplitTunnelingDomainList", string.Empty);
			_appSettingsHelper.SetValue("IsStreamingSplitTunnelingEnabled", "0");
			if (sdk != null)
			{
				sdk.VpnExpandedWindow.Dispatcher.Invoke(delegate
				{
					sdk.VpnEntities.MainWindow.EnableSignButtons();
					sdk.TaskBarService.SetNotifyIcon("regular", null);
				});
				sdk.AccountTypeHelper.SetAccountType(AccountType.None);
				sdk.VpnExpandedWindow.TrustedNetworkService.RemoveAll();
				sdk.NextAiVpnSdkManager.AllowOnlyVPNConnectivity = false;
				sdk.NextAiVpnSdkManager.AllowLANTraffic = true;
				sdk.NextAiVpnSdkManager.Dispose();
			}
			_analyticsService.RefreshData();
			_logger.Information("User: " + vpnCredentials?.VpnUsername + " - logged out", "LogOut", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 411);
		}
		catch (Exception ex)
		{
			LogError("[APICommon Logout] - " + ex.Message);
		}
	}

	public void RefreshToken(string key, string newValue)
	{
		_tokenRefreshHandler.RefreshToken(key, newValue);
	}

	public void SaveProtocol(string[] protocolInfo)
	{
		try
		{
			_appSettingsHelper.SetValue("protocol", protocolInfo[0]);
			_appSettingsHelper.SetValue("protocolType", protocolInfo[1]);
			if (protocolInfo[2] != null)
			{
				_appSettingsHelper.SetValue("scramble", protocolInfo[2]);
			}
		}
		catch (Exception ex)
		{
			_logger.Error("Unable to write protocol settings - " + ex.Message, "SaveProtocol", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 437);
		}
	}

	public void SaveConnectedTo(string location)
	{
		try
		{
			_appSettingsHelper.SetValue("ConnectedTo", location);
			_preferencesRepository.SaveSinglePreference("connectedto", location);
		}
		catch (Exception ex)
		{
			_logger.Error("Unable to write ConnectedTo settings - " + ex.Message, "SaveConnectedTo", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 450);
		}
	}

	public void SaveLastConnected(string location)
	{
		try
		{
			_appSettingsHelper.SetValue("LastConnected", location);
			_preferencesRepository.SaveSinglePreference("lastconnected", location);
		}
		catch (Exception ex)
		{
			_logger.Error("Unable to write LastConnected settings - " + ex.Message, "SaveLastConnected", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 463);
		}
	}

	public void SaveLastConnectedId(string id)
	{
		try
		{
			_appSettingsHelper.SetValue("LastConnectedId", id);
		}
		catch (Exception ex)
		{
			_logger.Error("Unable to write LastConnectedId settings - " + ex.Message, "SaveLastConnectedId", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 476);
		}
	}

	public string GetLastConnected()
	{
		string text = _appSettingsHelper.GetValue("LastConnected");
		if (text.Equals("0"))
		{
			text = "bestavailable";
		}
		return "/Resources/Flags/" + text + ".png";
	}

	public void SetIsBestAvailable(string value)
	{
		try
		{
			_appSettingsHelper.SetValue("IsBestAvailable", value);
			_preferencesRepository.SaveSinglePreference("isbestavailable", value);
		}
		catch (Exception ex)
		{
			_logger.Error("Unable to write IsBestAvailable settings - " + ex.Message, "SetIsBestAvailable", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 499);
		}
	}

	public void SetIsPurchaseStarted(string value)
	{
		try
		{
			_appSettingsHelper.SetValue("IsPurchaseStarted", value);
		}
		catch (Exception ex)
		{
			_logger.Error("Unable to write IsPurchaseStarted settings - " + ex.Message, "SetIsPurchaseStarted", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 511);
		}
	}

	private void LogResponseFromTheServer(HttpStatusCode responseStatusCode, string responseData)
	{
		_logger.Warning($"Subscription status. Status code: {responseStatusCode}\nResponse data: {responseData}", "LogResponseFromTheServer", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 517);
	}

	private void LogError(string msg)
	{
		_bugsnagService.Notify(msg);
		_logger.Error(msg, "LogError", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\APICommon.cs", 523);
	}
}
