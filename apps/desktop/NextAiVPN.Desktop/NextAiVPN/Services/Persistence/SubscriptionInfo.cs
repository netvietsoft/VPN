using System;
using System.Net;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using Newtonsoft.Json;
using RestSharp;

namespace NextAiVPN.Services.Persistence;

internal class SubscriptionInfo : ISubscriptionInfo
{
	private const int MaxRetries = 2;

	private const int RetryBaseDelayMs = 1000;

	private const int ApiTimeoutSeconds = 15;

	private readonly IAccountTypeHelper _accountTypeHelper;

	private readonly IBugsnagService _bugsnagService;

	private readonly IApiClient _apiClient;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly RestClient _restClient;

	private string _userAgent;

	private string _accessToken;

	private string _refreshToken;

	private string _expiresAt;

	private string _subscriptionEndPoint;

	private string _subscriptionAutoRenewEndPoint;

	private string _parameter;

	private string _subscriptionProlongationEndPoint;

	public bool IsSubscriptionExpireSoon { get; set; }

	public bool IsTrialExpireSoon { get; set; }

	public string Id { get; set; }

	public string Message { get; set; }

	public string UserName { get; set; }

	public string SubscriptionType { get; set; }

	public ActiveSubscription Subscription { get; set; }

	public bool IsTrial { get; set; }

	public bool IsTrialLimitReached { get; set; }

	public bool IsTrialEnded { get; set; }

	public SubscriptionInfo(IAccountTypeHelper accountTypeHelper, IBugsnagService bugsnagService, IApiClient apiClient, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_accountTypeHelper = accountTypeHelper;
		_bugsnagService = bugsnagService;
		_apiClient = apiClient;
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
		RefreshProperties();
		InitializeDefaultSubscription();
		_restClient = RestClientFactory.Create(new RestClientOptions
		{
			UserAgent = _userAgent,
			Timeout = TimeSpan.FromSeconds(15L)
		});
	}

	public void InitializeDefaultSubscription()
	{
		Subscription = new ActiveSubscription
		{
			Id = "nextai_vip_unlimited",
			Name = "NextAI VIP Residential",
			Status = "active",
			ViewStatus = "active",
			Autorenewal = true,
			CurrentPeriodStart = DateTime.Today.AddDays(-30),
			CurrentPeriodEnd = DateTime.Today.AddYears(5),
			ExpiresAt = DateTime.Today.AddYears(5),
			TrialEnd = null,
			TrialConvertedToPaid = true,
			Plan = new Plan
			{
				Id = 1,
				Slug = "nextai_residential_vip",
				PeriodIso8601 = "P1Y"
			}
		};
		UserName = !string.IsNullOrEmpty(UserName) ? UserName : "NextAi User";
		SubscriptionType = "1";
		Message = "NextAI VIP Residential Plan";
		Id = "nextai_vip_unlimited";
		IsTrial = false;
		IsSubscriptionExpireSoon = false;
		IsTrialExpireSoon = false;
		IsTrialLimitReached = false;
		IsTrialEnded = false;
	}

	private void RefreshProperties()
	{
		string appVersion = SystemInfo.AppVersion;
		_parameter = "{\"platform\":\"windows\", \"version\":\"" + appVersion + "\"}";
		_userAgent = SystemInfo.UserAgent;
		string baseEndPoint = ApiEndpoints.BaseEndPoint;
		_subscriptionEndPoint = baseEndPoint + "/api/v1/subscription";
		_subscriptionAutoRenewEndPoint = _subscriptionEndPoint + "/autorenew";
		_subscriptionProlongationEndPoint = _subscriptionEndPoint + "/prolongation/action";
		RefreshRequestData();
		UserName = _appSettingsHelper.GetValue("nickname");
	}

	private bool IsExpire(DateTime date, int day)
	{
		return Math.Abs((DateTime.Today.Date - date.Date).Days) <= day;
	}

	public async Task RefreshData()
	{
		try
		{
			RefreshProperties();
			RestResponse restResponse = await FetchSubscriptionWithRetriesAsync();
			if (restResponse == null || string.IsNullOrWhiteSpace(restResponse.Content))
			{
				_logger?.Error($"Subscription response is empty after {3} attempts. StatusCode: {restResponse?.StatusCode}; ResponseStatus: {restResponse?.ResponseStatus}; Error: {restResponse?.ErrorMessage}", "RefreshData", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionInfo.cs", 135);
				if (Subscription == null)
				{
					InitializeDefaultSubscription();
				}
				return;
			}
			_logger?.Information(restResponse.Content ?? "", "RefreshData", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionInfo.cs", 144);
			SubscriptionResponse subscriptionResponse = JsonConvert.DeserializeObject<SubscriptionResponse>(restResponse.Content);
			if (subscriptionResponse == null || subscriptionResponse.Success.Equals("0") || subscriptionResponse.ActiveSubscription == null)
			{
				if (Subscription == null)
				{
					InitializeDefaultSubscription();
				}
				return;
			}
			Subscription = subscriptionResponse.ActiveSubscription;
			IsTrialLimitReached = IsLimitReached(Subscription);
			IsTrial = IsTrialActive(Subscription);
			IsTrialEnded = IsTrialConverted(Subscription);
			if (IsTrial)
			{
				DateTime? trialEnd = Subscription.TrialEnd;
				if (trialEnd.HasValue)
				{
					DateTime valueOrDefault = trialEnd.GetValueOrDefault();
					IsTrialExpireSoon = IsExpire(valueOrDefault, 7);
				}
			}
			SubscriptionType = subscriptionResponse.SubscriptionType ?? "1";
			Id = subscriptionResponse.ActiveSubscription.Id ?? "nextai_vip_unlimited";
			Message = ParseSubscriptionMessageSrt(subscriptionResponse);
			if (subscriptionResponse.ActiveSubscription.ExpiresAt.HasValue)
			{
				IsSubscriptionExpireSoon = IsExpire(subscriptionResponse.ActiveSubscription.ExpiresAt.Value, 7);
			}
			if (!string.IsNullOrEmpty(Id) && !Id.Equals("0"))
			{
				_appSettingsHelper.SetValue("SubscriptionId", Id);
			}
		}
		catch (Exception ex)
		{
			_logger?.Error(ex, "RefreshData", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionInfo.cs", 190);
			_bugsnagService.Notify($"Subscription error - Path: {"SubscriptionInfo"}.{"RefreshData"}\n{ex.Message}");
			if (Subscription == null)
			{
				InitializeDefaultSubscription();
			}
		}
	}

	private async Task<RestResponse> FetchSubscriptionWithRetriesAsync()
	{
		RestResponse response = null;
		for (int attempt = 0; attempt <= 2; attempt++)
		{
			string callTime = DateTime.Now.ToString("G");
			response = await ExecuteTaskAsync(_subscriptionEndPoint, _parameter);
			if (response != null && !string.IsNullOrWhiteSpace(response.Content))
			{
				return response;
			}
			_logger?.Information("API call time " + _subscriptionEndPoint + " - " + callTime, "FetchSubscriptionWithRetriesAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionInfo.cs", 214);
			if (attempt < 2)
			{
				_logger?.Information($"Subscription refresh retry attempt {attempt + 1} of {2}", "FetchSubscriptionWithRetriesAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionInfo.cs", 218);
				await DelayBetweenRetriesAsync(attempt).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		return response;
	}

	protected virtual Task DelayBetweenRetriesAsync(int attempt)
	{
		return Task.Delay(1000 * (attempt + 1));
	}

	public async void ToggleAutoRenewal()
	{
		await Task.Run(delegate
		{
			try
			{
				string param = "{\"platform\":\"windows\", \"version\":\"" + SystemInfo.AppVersion + "\", \"autorenewal\":\"true\"}";
				ExecuteTaskAsync(_subscriptionAutoRenewEndPoint, param, Method.Post);
			}
			catch (Exception exception)
			{
				_logger?.Error(exception, "ToggleAutoRenewal", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionInfo.cs", 248);
			}
		});
	}

	public async Task<SubscriptionResponse> GetSubscriptionResponseAsync()
	{
		return JsonConvert.DeserializeObject<SubscriptionResponse>((await ExecuteTaskAsync(_subscriptionEndPoint, _parameter)).Content);
	}

	public async Task<NextAiTechnologyProlongationResponse> NextAiTechnologyProlongationActionAsync()
	{
		try
		{
			RefreshProperties();
			RestResponse restResponse = await ExecuteTaskAsync(_subscriptionProlongationEndPoint, _parameter, Method.Post);
			_logger?.Information($"[Prolongation] Status: {restResponse.StatusCode}, Content: {restResponse.Content}", "NextAiTechnologyProlongationActionAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionInfo.cs", 268);
			if (restResponse.StatusCode == HttpStatusCode.OK)
			{
				return JsonConvert.DeserializeObject<NextAiTechnologyProlongationResponse>(restResponse.Content);
			}
			return new NextAiTechnologyProlongationResponse
			{
				IsSuccess = false
			};
		}
		catch (Exception ex)
		{
			_logger?.Error(ex, "NextAiTechnologyProlongationActionAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionInfo.cs", 283);
			_bugsnagService.Notify("NextAiTechnology prolongation error.\n" + ex.Message);
			return new NextAiTechnologyProlongationResponse
			{
				IsSuccess = false
			};
		}
	}

	private async Task<RestResponse> ExecuteTaskAsync(string endPoint, string param, Method method = Method.Get)
	{
		RestRequest request = await BuildClientRestRequest(endPoint, param, method);
		RestResponse restResponse = await _restClient.ExecuteAsync(request).ConfigureAwait(continueOnCapturedContext: false);
		if (_accountTypeHelper.GetAccountType() == AccountType.NextAiTechnology)
		{
			_apiClient.RefreshTokensIfNeeded(restResponse);
		}
		return restResponse;
	}

	private static string ParseSubscriptionMessageSrt(SubscriptionResponse subscriptionResponse)
	{
		string periodIso = subscriptionResponse.ActiveSubscription.Plan.PeriodIso8601;
		if (periodIso == null)
		{
			return subscriptionResponse.Message ?? "No subscription info";
		}
		string text = periodIso.ToLower();
		int num = int.Parse(text[1].ToString());
		string value = string.Empty;
		if (num > 1)
		{
			if (text.Contains("y"))
			{
				value = "Years";
			}
			if (text.Contains("m"))
			{
				value = "Months";
			}
		}
		else
		{
			if (text.Contains("y"))
			{
				value = "Year";
			}
			if (text.Contains("m"))
			{
				value = "Month";
			}
		}
		return $"{num}-{value}";
	}

	private async Task<RestRequest> BuildClientRestRequest(string endPoint, string parameter, Method method)
	{
		RestRequest request = new RestRequest(endPoint, method);
		switch (_accountTypeHelper.GetAccountType())
		{
		case AccountType.NextAiTechnology:
			RefreshRequestData();
			request.AddHeader("X-nc-User", UserName);
			request.AddHeader("X-NAMP-Token", _accessToken);
			request.AddHeader("X-NAMP-Refresh-Token", _refreshToken);
			request.AddHeader("X-NAMP-Expires-At", _expiresAt);
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
		request.AddStringBody(parameter, ContentType.Json);
		return request;
	}

	private static bool IsTrialConverted(ActiveSubscription subscription)
	{
		if (subscription != null && subscription.TrialConvertedToPaid)
		{
			return subscription.TrialEnd.HasValue;
		}
		return false;
	}

	private static bool IsTrialActive(ActiveSubscription subscription)
	{
		if (subscription != null && !subscription.TrialConvertedToPaid)
		{
			return subscription.TrialEnd.HasValue;
		}
		return false;
	}

	private bool IsLimitReached(ActiveSubscription subscription)
	{
		if (!subscription.TrialEnd.HasValue)
		{
			return false;
		}
		bool num = 30720 < subscription.Consumption;
		if (num)
		{
			IAppLogger logger = _logger;
			if (logger == null)
			{
				return num;
			}
			logger.Information($"[Trial first] Traffic limit reached: {subscription.Consumption} Mb", "IsLimitReached", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SubscriptionInfo.cs", 402);
		}
		return num;
	}

	private void RefreshRequestData()
	{
		_accessToken = _appSettingsHelper.GetValue("access_token");
		_refreshToken = _appSettingsHelper.GetValue("refresh_token");
		_expiresAt = _appSettingsHelper.GetValue("expires_at");
	}
}
