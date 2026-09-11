using System;
using VpnSDK.Common.Utilities;

namespace VpnSDK.Common.Interop;

internal struct MibIpForwardRow(string destinationIp, string subNetMask, string? gatewayIp, int interfaceIndex, int metric)
{
	internal uint DwForwardDest = IpAddressUtility.ConvertIpAddressToUInt32(destinationIp);

	internal uint DwForwardMask = IpAddressUtility.ConvertIpAddressToUInt32(subNetMask);

	internal uint DwForwardPolicy = 0u;

	internal uint DwForwardNextHop = ((!string.IsNullOrEmpty(gatewayIp)) ? IpAddressUtility.ConvertIpAddressToUInt32(gatewayIp) : 0u);

	internal uint DwForwardIfIndex = Convert.ToUInt32(interfaceIndex);

	internal uint DwForwardType = Constants.MibIpRouteTypeDirect;

	internal uint DwForwardProto = Constants.MibIpProtoNetMgmt;

	internal uint DwForwardAge = 0u;

	internal uint DwForwardNextHopAS = 0u;

	internal uint DwForwardMetric1 = Convert.ToUInt32(metric);

	internal uint DwForwardMetric2 = 0u;

	internal uint DwForwardMetric3 = 0u;

	internal uint DwForwardMetric4 = 0u;

	internal uint DwForwardMetric5 = 0u;
}
