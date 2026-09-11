using System;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using RestSharp;

namespace NextAiVPN.Services;

internal class NextAiGlobalClientConfigService : IClientConfigService
{
	private readonly string _appVersion;

	private readonly string _userAgent;

	private readonly RestClient _restClient;

	private readonly string _baseUrl;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly IApiClient _apiClient;

	public NextAiGlobalClientConfigService(IAppSettingsHelper appSettingsHelper, IAppLogger logger, IApiClient apiClient)
	{
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
		_apiClient = apiClient;
		_appVersion = SystemInfo.AppVersion;
		_userAgent = SystemInfo.UserAgent;
		_restClient = RestClientFactory.Create(new RestClientOptions
		{
			UserAgent = _userAgent
		});
		_baseUrl = ApiEndpoints.BaseEndPoint;
	}

	public async Task<ClientConfig> GetClientConfig()
	{
		try
		{
			string param = "{\"platform\":\"windows\", \"version\":\"" + _appVersion + "\"}";
			return await ExecuteTaskAsync(_baseUrl + "/api/v1/client/config?platform=windows&version=" + _appVersion, param);
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "GetClientConfig", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\NextAiGlobalClientConfigService.cs", 53);
			return null;
		}
	}

	private async Task<ClientConfig> ExecuteTaskAsync(string endPoint, string param, Method method = Method.Get)
	{
		await _apiClient.RefreshNextAiGlobalTokensIfNeeded();
		RestRequest request = BuildClientRestRequest(endPoint, param, method);
		return (await _restClient.ExecuteAsync<ClientConfig>(request)).Data;
	}

	private RestRequest BuildClientRestRequest(string endPoint, string parameter, Method method)
	{
		RestRequest restRequest = new RestRequest(endPoint, method);
		restRequest.AddHeader("User-Agent", _userAgent);
		restRequest.AddHeader("X-SPS-Token", _appSettingsHelper.GetValue("id_token"));
		restRequest.AddStringBody(parameter, ContentType.Json);
		return restRequest;
	}
}
