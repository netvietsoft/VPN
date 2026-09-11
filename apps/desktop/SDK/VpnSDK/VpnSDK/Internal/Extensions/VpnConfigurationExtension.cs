using System;
using System.Collections.Generic;
using VpnSDK.Enums;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Internal.Extensions;

internal static class VpnConfigurationExtension
{
	public static List<NetworkConnectionType> GetNetworkConnectionTypes(this VpnConfiguration configuration)
	{
		List<NetworkConnectionType> list = new List<NetworkConnectionType>();
		foreach (VpnType value in Enum.GetValues(typeof(VpnType)))
		{
			if ((configuration.Protocols & value) != VpnType.None)
			{
				switch (value)
				{
				case VpnType.IKEv2:
					list.Add(NetworkConnectionType.IKEv2);
					break;
				case VpnType.OpenVPN:
					list.Add(NetworkConnectionType.OpenVPN);
					break;
				case VpnType.WireGuard:
					list.Add(NetworkConnectionType.WireGuard);
					break;
				}
			}
		}
		return list;
	}
}
