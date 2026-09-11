using System;
using System.Security.Cryptography;
using System.Text;
using NextAiVPN.Common;
using NextAiVPN.Entities;

namespace NextAiVPN.Services.Persistence;

internal class OAuth2NextAiGlobalAuthenticator
{
	private readonly IAppSettingsHelper _appSettingsHelper;

	public OAuth2NextAiGlobalAuthenticator(IAppSettingsHelper appSettingsHelper)
	{
		_appSettingsHelper = appSettingsHelper;
	}

	public string Authenticate()
	{
		string text = CreateCodeVerifier();
		_appSettingsHelper.SetValue("CodeVerifier", text);
		string codeChallenge = CodeChallenge(text);
		OAuth2NextAiGlobalParameters parameters = new OAuth2NextAiGlobalParameters();
		return GenerateUrl(parameters, codeChallenge);
	}

	public string GenerateUrl(OAuth2NextAiGlobalParameters parameters, string codeChallenge)
	{
		return parameters.AuthorizeUrl + "?response_type=code&scope=openid%20offline_access%20external.nextaiglobal.com&code_challenge=" + codeChallenge + "&code_challenge_method=S256&client_id=" + parameters.ClientId + "&redirect_uri=" + parameters.RedirectUri;
	}

	public string CodeChallenge(string verifier)
	{
		using SHA256 sHA = SHA256.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(verifier);
		return Convert.ToBase64String(sHA.ComputeHash(bytes)).Replace("+", "-").Replace("/", "_")
			.TrimEnd('=')
			.Trim();
	}

	public string CreateCodeVerifier()
	{
		byte[] array = new byte[32];
		using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
		{
			randomNumberGenerator.GetBytes(array);
		}
		return Convert.ToBase64String(array).Replace("+", "-").Replace("/", "_")
			.TrimEnd('=')
			.Trim();
	}
}
