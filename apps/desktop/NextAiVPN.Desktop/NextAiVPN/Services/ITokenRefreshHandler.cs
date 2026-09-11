using RestSharp;

namespace NextAiVPN.Services;

public interface ITokenRefreshHandler
{
	void RefreshTokensIfNeeded(RestResponse response);

	void RefreshToken(string key, string newValue);
}
