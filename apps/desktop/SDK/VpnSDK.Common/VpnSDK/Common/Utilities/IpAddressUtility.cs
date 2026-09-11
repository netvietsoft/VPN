using System;
using System.Net;

namespace VpnSDK.Common.Utilities;

internal class IpAddressUtility
{
	internal static uint ConvertIpAddressToUInt32(string ipAddress)
	{
		if (IPAddress.TryParse(ipAddress, out IPAddress address))
		{
			return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
		}
		return 0u;
	}

	internal static (IPAddress IPAddress, IPAddress SubnetMask) ConvertCidr(string cidr)
	{
		string[] array = cidr.Split('/');
		string ipString = array[0];
		int num = int.Parse(array[1]);
		IPAddress item = IPAddress.Parse(ipString);
		byte[] array2 = new byte[4];
		for (int i = 0; i < num; i++)
		{
			array2[i / 8] |= (byte)(1 << 7 - i % 8);
		}
		IPAddress item2 = new IPAddress(array2);
		return (IPAddress: item, SubnetMask: item2);
	}
}
