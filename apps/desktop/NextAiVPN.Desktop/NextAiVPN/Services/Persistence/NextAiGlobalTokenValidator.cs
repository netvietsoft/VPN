using System;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using Newtonsoft.Json;
using RestSharp;

namespace NextAiVPN.Services.Persistence;

internal class NextAiGlobalTokenValidator : INextAiGlobalTokenValidator
{
	private const int MaxRetries = 2;

	private const int RetryBaseDelayMs = 1000;

	private const int ApiTimeoutSeconds = 15;

	private readonly AccessTokenGenerator _accessTokenGenerator;

	private readonly ICredentialStore _credentialStore;

	private readonly IAppLogger _logger;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IApiClient _apiClient;

	private readonly string _userAgent;

	private readonly RestClient _restClient;

	public NextAiGlobalTokenValidator(AccessTokenGenerator accessTokenGenerator, ICredentialStore credentialStore, IAppLogger logger, IAppSettingsHelper appSettingsHelper, IApiClient apiClient)
	{
		_accessTokenGenerator = accessTokenGenerator;
		_credentialStore = credentialStore;
		_logger = logger;
		_appSettingsHelper = appSettingsHelper;
		_apiClient = apiClient;
		_userAgent = SystemInfo.UserAgent;
		_restClient = RestClientFactory.Create(new RestClientOptions
		{
			UserAgent = _userAgent,
			Timeout = TimeSpan.FromSeconds(15L)
		});
	}

	public async Task<NextAiGlobalTokenValidatorResponse> ValidateTokenAsync()
	{
		for (int attempt = 0; attempt <= 2; attempt++)
		{
			try
			{
				NextAiGlobalTokenValidatorResponse nextaiglobalTokenValidatorResponse = await ValidateTokenInternalAsync();
				if (nextaiglobalTokenValidatorResponse.IsSuccess == 1)
				{
					SaveSuccessfulLogin(nextaiglobalTokenValidatorResponse);
				}
				return nextaiglobalTokenValidatorResponse;
			}
			catch (Exception exception)
			{
				_logger.Error(exception, "ValidateTokenAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiGlobalTokenValidator.cs", 62);
				if (attempt < 2)
				{
					_logger.Information($"NextAiGlobal login retry attempt {attempt + 1} of {2}", "ValidateTokenAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiGlobalTokenValidator.cs", 66);
					await DelayBetweenRetriesAsync(attempt).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
		return null;
	}

	private async Task<NextAiGlobalTokenValidatorResponse> ValidateTokenInternalAsync()
	{
		NextAiGlobalTokenValidatorResponse nextaiglobalTokenValidatorResponse = await ExecuteAndDeserializeLoginAsync();
		if (nextaiglobalTokenValidatorResponse == null || nextaiglobalTokenValidatorResponse.Message == null)
		{
			return nextaiglobalTokenValidatorResponse;
		}
		_logger.Information("Validate NextAiGlobal token message: " + nextaiglobalTokenValidatorResponse.Message, "ValidateTokenInternalAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiGlobalTokenValidator.cs", 84);
		await _accessTokenGenerator.RefreshToken();
		return await ExecuteAndDeserializeLoginAsync();
	}

	private async Task<NextAiGlobalTokenValidatorResponse> ExecuteAndDeserializeLoginAsync()
	{
		string callTime = DateTime.Now.ToString("G");
		RestResponse restResponse = await ExecuteLoginAsync();
		if (restResponse == null || string.IsNullOrWhiteSpace(restResponse.Content))
		{
			_logger?.Information("API call time " + ApiEndpoints.NextAiGlobalLoginEndPoint + " - " + callTime, "ExecuteAndDeserializeLoginAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiGlobalTokenValidator.cs", 99);
			_logger?.Error($"NextAiGlobal login response is empty.\nStatusCode: {restResponse?.StatusCode};\nResponseStatus: {restResponse?.ResponseStatus};\nError: {restResponse?.ErrorMessage}\n" + restResponse?.ErrorException, "ExecuteAndDeserializeLoginAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiGlobalTokenValidator.cs", 100);
		}
		return JsonConvert.DeserializeObject<NextAiGlobalTokenValidatorResponse>(restResponse.Content);
	}

	protected virtual Task DelayBetweenRetriesAsync(int attempt)
	{
		return Task.Delay(1000 * (attempt + 1));
	}

	private void SaveSuccessfulLogin(NextAiGlobalTokenValidatorResponse result)
	{
		_appSettingsHelper.SetValue("NextAiGlobalUserId", result.UserId);
		_credentialStore.SaveCredentials(new VpnCredentials(result.VpnUsername, result.VpnPassword));
		_appSettingsHelper.SetValue("nickname", result.Username);
		Subscription activeSubscription = result.GetActiveSubscription();
		if (activeSubscription != null)
		{
			TimeSpan timeSpan = activeSubscription.ExpiresAt - DateTime.Now;
			_appSettingsHelper.SetValue("expires_at", timeSpan.ToString());
		}
	}

	protected virtual Task<RestResponse> ExecuteLoginAsync()
	{
		return ExecuteTaskAsync(ApiEndpoints.NextAiGlobalLoginEndPoint, Method.Post);
	}

	private async Task<RestResponse> ExecuteTaskAsync(string endPoint, Method method = Method.Get)
	{
		await _apiClient.RefreshNextAiGlobalTokensIfNeeded();
		string value = _appSettingsHelper.GetValue("id_token");
		string parameter = "{\"id_token\":\"" + value + "\"}";
		RestRequest request = BuildClientRestRequest(endPoint, parameter, method);
		return await _restClient.ExecuteAsync(request);
	}

	private RestRequest BuildClientRestRequest(string endPoint, string parameter, Method method)
	{
		RestRequest restRequest = new RestRequest(endPoint, method);
		restRequest.AddHeader("User-Agent", _userAgent);
		restRequest.AddStringBody(parameter, ContentType.Json);
		return restRequest;
	}
}
