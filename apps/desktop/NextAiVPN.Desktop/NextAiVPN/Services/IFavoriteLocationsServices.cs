using System.Collections.Generic;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

public interface IFavoriteLocationsServices
{
	List<ILocation> GetFavoriteLocations();

	List<string> GetFavoriteListFromConfig();

	List<string> GetFavoriteListFromConfigOnlyCountries();
}
