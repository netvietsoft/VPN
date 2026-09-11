using System;
using System.Linq;
using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;
using RestSharp;

namespace NextAiVPN.Services;

internal class TokenRefreshHandler : ITokenRefreshHandler
{
	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IPreferencesRepository _preferencesRepository;

	private readonly IBugsnagService _bugsnagService;

	private readonly IAppLogger _logger;

	public TokenRefreshHandler(IAppSettingsHelper appSettingsHelper, IPreferencesRepository preferencesRepository, IBugsnagService bugsnagService, IAppLogger logger)
	{
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_preferencesRepository = preferencesRepository ?? throw new ArgumentNullException("preferencesRepository");
		_bugsnagService = bugsnagService ?? throw new ArgumentNullException("bugsnagService");
		_logger = logger;
	}

	public void RefreshTokensIfNeeded(RestResponse response)
	{
		try
		{
			if (response?.Headers == null || !response.Headers.Any((HeaderParameter t) => t.Name == "X-New-Access-Token"))
			{
				return;
			}
			foreach (HeaderParameter header in response.Headers)
			{
				switch (header.Name)
				{
				case "X-New-Access-Token":
					RefreshToken("access_token", header.Value.ToString());
					break;
				case "X-New-Refresh-Token":
					RefreshToken("refresh_token", header.Value.ToString());
					break;
				case "X-New-Expires-At":
					RefreshToken("expires_at", header.Value.ToString());
					break;
				}
			}
		}
		catch (Exception ex)
		{
			LogError($"RefreshToken error Path: {"TokenRefreshHandler"}.{"RefreshTokensIfNeeded"}() - {ex.Message}");
		}
	}

	public void RefreshToken(string key, string newValue)
	{
		try
		{
			_appSettingsHelper.SetValue(key, newValue);
			switch (key)
			{
			case "access_token":
				_preferencesRepository.SaveSinglePreference("accesstoken", newValue);
				break;
			case "refresh_token":
				_preferencesRepository.SaveSinglePreference("refreshtoken", newValue);
				break;
			case "expires_at":
				_preferencesRepository.SaveSinglePreference("expiresat", newValue);
				break;
			}
		}
		catch (Exception ex)
		{
			LogError("Unable to update token in configuration settings: " + ex.Message);
		}
	}

	private void LogError(string msg)
	{
		_logger?.Error(msg, "LogError", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\TokenRefreshHandler.cs", 100);
		_bugsnagService.Notify(msg);
	}
}
