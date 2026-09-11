using VpnSDK.Interfaces;

namespace NextAiVPN.Services.Persistence;

public interface ILocationHelper
{
	ILocation GetBestPingCountryLocation(string countryCode);

	ILocation GetNextBestPingCountryLocation(ILocation location);
}
