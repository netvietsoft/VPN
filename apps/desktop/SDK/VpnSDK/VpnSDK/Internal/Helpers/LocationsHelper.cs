using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VpnSDK.DTO;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Dtos;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Internal.Helpers;

internal static class LocationsHelper
{
	internal static IList<Server> LocationToServers(ReadOnlyObservableCollection<ILocation> allLocations, IEnumerable<ILocation> selectedLocations, NetworkGeolocation lastKnownUserLocation)
	{
		IList<Server> list = new List<Server>();
		if (selectedLocations.FirstOrDefault() is ServerProxy serverProxy)
		{
			list.Add(serverProxy.Node);
		}
		else if (selectedLocations.FirstOrDefault() is IBestAvailable bestAvailable)
		{
			if (selectedLocations == null || !selectedLocations.Any() || selectedLocations.FirstOrDefault() is BestAvailable)
			{
				RegionProxy regionProxy = allLocations.OfType<RegionProxy>().FirstOrDefault((RegionProxy x) => x.HasNode);
				if (regionProxy != null)
				{
					list = BestAvailableServerHelper.Find(regionProxy.Node.Parent.GetParent<Network>(), lastKnownUserLocation);
				}
			}
			else if (bestAvailable.BestRegion is RegionProxy { HasNode: not false } regionProxy2)
			{
				list = BestAvailableServerHelper.Find(regionProxy2.Node, lastKnownUserLocation);
			}
		}
		else if (selectedLocations.Count() == 1)
		{
			ILocation location = selectedLocations.First();
			if (location is RegionProxy regionProxy3)
			{
				if (regionProxy3.HasNode)
				{
					list = BestAvailableServerHelper.Find(regionProxy3.Node, lastKnownUserLocation);
				}
			}
			else
			{
				list = BestAvailableServerHelper.Find((Location)location, lastKnownUserLocation);
			}
		}
		else
		{
			list = BestAvailableServerHelper.Find(selectedLocations, lastKnownUserLocation);
		}
		return list;
	}
}
