using System;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using RestSharp;

namespace NextAiVPN.Services;

internal class NextAiTechnologyClientConfigService : IClientConfigService
{
	private readonly string _appVersion;

	private readonly string _baseUrl;

	private readonly string _userAgent;

	private readonly RestClient _restClient;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	public NextAiTechnologyClientConfigService(IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
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
		catch (Exception ex)
		{
			_logger?.Error(ex.Message, "GetClientConfig", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\NextAiTechnologyClientConfigService.cs", 46);
			return null;
		}
	}

	private async Task<ClientConfig> ExecuteTaskAsync(string endPoint, string param, Method method = Method.Get)
	{
		RestRequest request = BuildClientRestRequest(endPoint, param, method);
		return (await _restClient.ExecuteAsync<ClientConfig>(request)).Data;
	}

	private RestRequest BuildClientRestRequest(string endPoint, string parameter, Method method)
	{
		RestRequest restRequest = new RestRequest(endPoint, method);
		restRequest.AddHeader("X-NC-User", _appSettingsHelper.GetValue("nickname"));
		restRequest.AddHeader("X-NAMP-Token", _appSettingsHelper.GetValue("access_token"));
		restRequest.AddHeader("X-NAMP-Refresh-Token", _appSettingsHelper.GetValue("refresh_token"));
		restRequest.AddHeader("X-NAMP-Expires-At", _appSettingsHelper.GetValue("expires_at"));
		restRequest.AddStringBody(parameter, ContentType.Json);
		return restRequest;
	}
}
