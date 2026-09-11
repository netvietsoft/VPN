using System.Collections.Generic;
using System.Linq;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services.Persistence;

internal class LocationHelper : ILocationHelper
{
	private readonly ISDK _sdkManager;

	public LocationHelper(ISDK sdkObject)
	{
		_sdkManager = sdkObject;
	}

	public ILocation GetBestPingCountryLocation(string countryCode)
	{
		ILocation result = _sdkManager.Locations.FirstOrDefault((ILocation x) => x.CountryCode.Equals(countryCode));
		ushort num = ushort.MaxValue;
		foreach (ILocation location in _sdkManager.Locations)
		{
			if (location.CountryCode.Equals(countryCode) && num > location.PingMs)
			{
				num = location.PingMs.Value;
				result = location;
			}
		}
		return result;
	}

	public ILocation GetNextBestPingCountryLocation(ILocation location)
	{
		IEnumerable<ILocation> source = _sdkManager.Locations.Where((ILocation x) => x.CountryCode.Equals(location.CountryCode));
		if (source.Count() <= 1)
		{
			return null;
		}
		return source.OrderBy((ILocation r) => r.PingMs).FirstOrDefault((ILocation x) => !x.CityCode.Equals(location.CityCode));
	}
}
