using System.Net;

namespace VpnSDK.Common.Routing;

internal class RouteInfo
{
	internal IPAddress DestinationIpAddress { get; }

	internal IPAddress SubnetMask { get; }

	internal RouteInfo(IPAddress destinationIpAddress, IPAddress subnetMask)
	{
		DestinationIpAddress = destinationIpAddress;
		SubnetMask = subnetMask;
	}
}
