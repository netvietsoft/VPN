using System.Threading.Tasks;
using NextAiVPN.Entities;
using RestSharp;

namespace NextAiVPN.Services;

public interface IApiClient
{
	Task<NextAiTechnologyAuthResponse> GetAuthData(string code);

	Task<string> GetLatestVersionNumber();

	Task<string> GetWebView2InstallerLink();

	string GetNextAiVpnVersionLink();

	Task SaveSettings(NextAiTechnologyAuthResponse responseData);

	void RefreshTokensIfNeeded(RestResponse response);

	Task RefreshNextAiGlobalTokensIfNeeded();

	Task<string> GetSubscriptionStatus();

	void LogOut(SDKMonitor sdk);

	void RefreshToken(string key, string newValue);

	void SaveProtocol(string[] protocolInfo);

	void SaveConnectedTo(string location);

	void SaveLastConnected(string location);

	void SaveLastConnectedId(string id);

	string GetLastConnected();

	void SetIsBestAvailable(string value);

	void SetIsPurchaseStarted(string value);
}
