using System;
using System.Collections.Generic;
using System.Linq;
using VpnSDK.DTO;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Dtos;
using VpnSDK.Internal.Extensions;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Internal.Helpers;

internal static class BestAvailableServerHelper
{
	private const int ServerLoadThreshold = 85;

	private const string GB = "GB";

	private const string UK = "UK";

	private const double CountryServersPoolRatio = 0.15;

	private const int MinimumCountryServers = 4;

	private const int MinServersToSortByLoad = 10;

	private const int FastestServers = 3;

	private const int RandomServers = 2;

	private const int MaxCities = 3;

	private const int MinPoolSizeForCountryServers = 8;

	public static IList<Server> Find(Network network, NetworkGeolocation userLocation)
	{
		return network.GetChildren<Location>().SelectMany((Location x) => x.GetChildren<Location>()).GetClosestServers(userLocation)
			.GetBestServers();
	}

	public static IList<Server> Find(IEnumerable<ILocation> locations, NetworkGeolocation userLocation)
	{
		return locations.Select((ILocation x) => ((RegionProxy)x).Node).GetClosestServers(userLocation).GetBestServers();
	}

	public static IList<Server> Find(IList<Location> locations, NetworkGeolocation userLocation)
	{
		return locations.GetClosestServers(userLocation).GetBestServers();
	}

	public static IList<Server> Find(Location location, NetworkGeolocation userLocation)
	{
		IList<Server> servers = ((!(location.Children.FirstOrDefault() is Location)) ? new List<Location> { location }.GetClosestServers(userLocation) : location.GetChildren<Location>().GetClosestServers(userLocation));
		return servers.GetBestServers();
	}

	public static IList<Server> GetClosestServers(this IEnumerable<Location> locations, NetworkGeolocation userLocation)
	{
		if (locations.FirstOrDefault()?.Children.FirstOrDefault() is Location)
		{
			return locations.GetClosestServers(userLocation);
		}
		if (locations.FirstOrDefault()?.Children.FirstOrDefault() is Server)
		{
			List<Server> list = new List<Server>();
			List<Server> userCountryServers = locations.GetUserCountryServers(userLocation);
			List<Server> neighbouringCitiesServers = locations.GetNeighbouringCitiesServers(userLocation, userCountryServers);
			list.AddRange(userCountryServers);
			list.AddRange(neighbouringCitiesServers);
			return list;
		}
		throw new InvalidCastException("The locations enumerable is not either a Server or a Location.");
	}

	private static List<Server> GetNeighbouringCitiesServers(this IEnumerable<Location> locations, NetworkGeolocation userLocation, List<Server> userCountryServers)
	{
		List<Server> servers = new List<Server>();
		try
		{
			(from location in locations
				select new
				{
					Location = location,
					Servers = (from s in location.Flatten().OfType<Server>()
						where !s.InMaintenance && s.Load < 85
						select s).ToList().Except(userCountryServers)
				} into x
				where x.Servers.Any()
				orderby x.Location.GeoCoordinate.ToGeoCoordinate().GetDistanceTo(userLocation.ToGeoCoordinate())
				select x).Take(3).ToList().ForEach(x =>
			{
				servers.AddRange(x.Servers);
			});
		}
		catch
		{
		}
		return servers;
	}

	private static List<Server> GetUserCountryServers(this IEnumerable<Location> locations, NetworkGeolocation userLocation)
	{
		List<Server> list = new List<Server>();
		if (userLocation != null)
		{
			string userCountryCode = (userLocation.CountryCode.Equals("GB", StringComparison.InvariantCultureIgnoreCase) ? "UK" : userLocation.CountryCode);
			List<Location> list2 = (from x in locations
				where x.ParentId.Equals(userCountryCode, StringComparison.InvariantCultureIgnoreCase)
				orderby x.GeoCoordinate.ToGeoCoordinate().GetDistanceTo(userLocation.ToGeoCoordinate())
				select x).ToList();
			for (int num = 0; num < list2.Count; num += 3)
			{
				List<Server> list3 = list2.Skip(num).Take(3).SelectMany((Location x) => from Server s in from s in x.Flatten()
						where s is Server
						select s
					where !s.InMaintenance && s.Load < 85
					select s)
					.ToList();
				if (list3.Any())
				{
					list.AddRange(list3);
				}
				if (list.Count >= 4)
				{
					break;
				}
			}
		}
		return list;
	}

	private static IList<Server> GetBestServers(this IList<Server> servers)
	{
		IEnumerable<Server> source = servers.Where((Server s) => s.Parent.ParentId == servers.First().Parent.ParentId);
		int count = Math.Max(8, (int)(0.15 * (double)source.Count()));
		IEnumerable<Server> source2 = (from _ in source.OrderBy((Server x) => x.Load).Take(count)
			orderby Guid.NewGuid()
			select _).Take(3);
		IList<Server> bestServers = ((source.Count() <= 10) ? source2.ToList() : source2.OrderBy((Server x) => x.Load).ToList());
		int count2 = 5 - bestServers.Count;
		servers.OrderBy((Server _) => Guid.NewGuid()).Except(bestServers).Take(count2)
			.ToList()
			.ForEach(delegate(Server s)
			{
				bestServers.Add(s);
			});
		return bestServers;
	}
}
