using System;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using Newtonsoft.Json;
using RestSharp;

namespace NextAiVPN.Services.Persistence;

internal class SubscriptionStatusHelper : ISubscriptionStatusHelper
{
	private readonly IAccountTypeHelper _accountTypeHelper;

	private readonly IBugsnagService _bugsnagService;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly IApiClient _apiClient;

	private readonly ITokenRefreshHandler _tokenRefreshHandler;

	private readonly string _userAgent;

	private readonly RestClient _restClient;

	public SubscriptionStatusHelper(IAccountTypeHelper accountTypeHelper, IBugsnagService bugsnagService, IAppSettingsHelper appSettingsHelper, IAppLogger logger, IApiClient apiClient, ITokenRefreshHandler tokenRefreshHandler)
	{
		_accountTypeHelper = accountTypeHelper;
		_bugsnagService = bugsnagService;
		_appSettingsHelper = appSettingsHelper;
		_logger = logger;
		_apiClient = apiClient;
		_tokenRefreshHandler = tokenRefreshHandler;
		_userAgent = SystemInfo.UserAgent;
		_restClient = RestClientFactory.Create(new RestClientOptions
		{
			UserAgent = _userAgent
		});
	}

	public async Task<SubscriptionStatus> GetSubscriptionStatusInfo()
	{
		_ = 1;
		try
		{
			string url = ApiEndpoints.CheckSubscriptionEndPoint;
			string callTime = DateTime.Now.ToString("G");
			RestRequest request = new RestRequest(url, Method.Post);
			switch (_accountTypeHelper.GetAccountType())
			{
			case AccountType.NextAiTechnology:
				request.AddHeader("X-nc-User", _appSettingsHelper.GetValue("nickname"));
				request.AddHeader("X-NAMP-Token", _appSettingsHelper.GetValue("access_token"));
				request.AddHeader("X-NAMP-Refresh-Token", _appSettingsHelper.GetValue("refresh_token"));
				request.AddHeader("X-NAMP-Expires-At", _appSettingsHelper.GetValue("expires_at"));
				break;
			case AccountType.NextAiGlobal:
				await _apiClient.RefreshNextAiGlobalTokensIfNeeded();
				request.AddHeader("X-SPS-Token", _appSettingsHelper.GetValue("id_token"));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case AccountType.None:
				break;
			}
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Accept-Language", RequestContext.BuildAcceptLanguage());
			RestResponse<SubscriptionStatus> restResponse = await _restClient.ExecuteAsync<SubscriptionStatus>(request);
			if (string.IsNullOrEmpty(restResponse.Content))
			{
				_logger?.Information("API call time " + url + " - " + callTime, "GetSubscriptionStatusInfo", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionStatusHelper.cs", 79);
				return null;
			}
			SubscriptionStatus? result = JsonConvert.DeserializeObject<SubscriptionStatus>(restResponse.Content);
			if (_accountTypeHelper.GetAccountType() == AccountType.NextAiTechnology)
			{
				_tokenRefreshHandler.RefreshTokensIfNeeded(restResponse);
			}
			return result;
		}
		catch (Exception ex)
		{
			LogError(ex.Message);
			return null;
		}
	}

	private void LogError(string msg)
	{
		_bugsnagService.Notify(msg);
		_logger?.Error(msg, "LogError", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionStatusHelper.cs", 101);
	}
}
