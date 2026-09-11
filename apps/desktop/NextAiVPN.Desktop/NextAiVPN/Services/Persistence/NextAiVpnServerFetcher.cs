using System;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace NextAiVPN.Services.Persistence;

internal class NextAiVpnServerFetcher
{
	private readonly ICredentialStore _credentialStore;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly RestClient _restClient;

	public NextAiVpnServerFetcher(ICredentialStore credentialStore, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_credentialStore = credentialStore;
		_appSettingsHelper = appSettingsHelper;
		_logger = logger;
		_restClient = RestClientFactory.Create(new RestClientOptions("https://api.nextaivpn.com/v3"));
	}

	public async Task GetServersAsync()
	{
		string text = await LoginAsync();
		if (!string.IsNullOrEmpty(text))
		{
			RestRequest request = new RestRequest("servers");
			request.AddQueryParameter("simple_type", "true");
			request.AddHeader("X-API-VERSION", "3.1");
			request.AddHeader("Authorization", "Bearer " + text);
			request.AddHeader("Content-Type", "application/json; charset=utf-8");
			RestResponse restResponse = await _restClient.ExecuteAsync(request);
			_logger.Information($"Status: {(int)restResponse.StatusCode} {restResponse.StatusCode}", "GetServersAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiVpnServerFetcher.cs", 40);
			_logger.Information(restResponse.Content, "GetServersAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiVpnServerFetcher.cs", 41);
		}
	}

	public async Task<string> LoginAsync()
	{
		try
		{
			RestRequest request = new RestRequest("login", Method.Post);
			request.AddHeader("X-API-VERSION", "3.1");
			request.AddHeader("User-Agent", "Windows/nextaitechnology/1.5.3.");
			request.AddHeader("X-Client", "nextaitechnology");
			request.AddHeader("X-Client-Version", "1.5.3.");
			request.AddHeader("X-Platform", "Windows");
			request.AddHeader("X-Platform-Version", "10");
			request.AddHeader("Accept", "application/json");
			VpnCredentials credentials = _credentialStore.GetCredentials();
			var obj = new
			{
				api_key = _appSettingsHelper.GetValue("933f67de383fb9987d8c11216bc94da1"),
				password = credentials.VpnPassword,
				username = credentials.VpnUsername
			};
			request.AddJsonBody(obj);
			RestResponse restResponse = await _restClient.ExecuteAsync(request);
			if (restResponse.IsSuccessful)
			{
				_logger.Information("✅ Login successful!", "LoginAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiVpnServerFetcher.cs", 74);
				_logger.Information(restResponse.Content, "LoginAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiVpnServerFetcher.cs", 75);
				return JObject.Parse(restResponse.Content)["access_token"]?.ToString();
			}
			_logger.Information($"❌ Error: {restResponse.StatusCode}", "LoginAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiVpnServerFetcher.cs", 86);
			_logger.Information(restResponse.Content, "LoginAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiVpnServerFetcher.cs", 87);
		}
		catch (Exception exception)
		{
			_logger.Error(exception, "LoginAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\NextAiVpnServerFetcher.cs", 92);
		}
		return string.Empty;
	}
}
