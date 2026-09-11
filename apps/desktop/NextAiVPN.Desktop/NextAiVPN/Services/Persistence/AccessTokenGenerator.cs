using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Threading;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using Newtonsoft.Json;
using RestSharp;

namespace NextAiVPN.Services.Persistence;

public class AccessTokenGenerator
{
	private const int RefreshTokenTimeStock = 15;

	private readonly OAuth2NextAiGlobalParameters _parameters;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly RestClient _restClient;

	private DispatcherTimer _refreshTokenTimer;

	public AccessTokenGenerator(OAuth2NextAiGlobalParameters parameters, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_parameters = parameters;
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
		_restClient = RestClientFactory.Create(new RestClientOptions(_parameters.TokenUrl));
	}

	private void SetTimer(int time)
	{
		_refreshTokenTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(GetRefreshTime(time))
		};
		_refreshTokenTimer.Tick += _refreshTokenTimer_Tick;
		_refreshTokenTimer.Start();
	}

	private async void _refreshTokenTimer_Tick(object sender, EventArgs e)
	{
		await RefreshToken();
	}

	public async Task RefreshToken()
	{
		string value = _appSettingsHelper.GetValue("refresh_token");
		RestRequest request = new RestRequest(string.Empty, Method.Post).AddStringBody("grant_type=refresh_token&client_id=" + _parameters.ClientId + "&refresh_token=" + value, "application/x-www-form-urlencoded");
		SaveResponse(await _restClient.ExecuteAsync(request), "Tokens refreshed successfully");
	}

	private static int GetRefreshTime(int expiredIn)
	{
		if (expiredIn < 15)
		{
			return 60;
		}
		return expiredIn - 15;
	}

	public async Task Generate(string codeVerifier, string code)
	{
		RestRequest request = new RestRequest(string.Empty, Method.Post).AddStringBody($"grant_type=authorization_code&client_id={_parameters.ClientId}&code_verifier={codeVerifier}&code={code}&redirect_uri={_parameters.RedirectUri}", "application/x-www-form-urlencoded");
		SaveResponse(await _restClient.ExecuteAsync(request), "Tokens generated successfully");
	}

	private void SaveResponse(RestResponse response, string message)
	{
		if (response.IsSuccessful)
		{
			NextAiGlobalTokenResponse nextaiglobalTokenResponse = JsonConvert.DeserializeObject<NextAiGlobalTokenResponse>(response.Content);
			if (nextaiglobalTokenResponse != null)
			{
				if (nextaiglobalTokenResponse.AccessToken != null)
				{
					_logger?.Information(message, "SaveResponse", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AccessTokenGenerator.cs", 89);
					_appSettingsHelper.SetValue("access_token", nextaiglobalTokenResponse.AccessToken);
					_appSettingsHelper.SetValue("id_token", nextaiglobalTokenResponse.IdToken);
					_appSettingsHelper.SetValue("refresh_token", nextaiglobalTokenResponse.RefreshToken);
					_appSettingsHelper.SetValue("expires_in", nextaiglobalTokenResponse.ExpiresIn.ToString());
					_appSettingsHelper.SetValue("LastRefreshTokenDate", DateTime.Now.ToString("F", CultureInfo.InvariantCulture));
				}
				else
				{
					_logger?.Error("AccessToken == null", "SaveResponse", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AccessTokenGenerator.cs", 98);
				}
			}
		}
		else
		{
			_logger?.Error("Tokens generate error", "SaveResponse", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AccessTokenGenerator.cs", 105);
			_logger?.Error(response.ErrorMessage ?? $"HTTP {(int)response.StatusCode} ({response.ResponseStatus})", "SaveResponse", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\AccessTokenGenerator.cs", 106);
		}
	}
}
