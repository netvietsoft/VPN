using System;
using System.Net.NetworkInformation;
using VpnSDK.Internal.Interop;

namespace VpnSDK.Internal.Extensions;

internal static class NetworkInterfaceExtensions
{
	public static int GetMetric(this NetworkInterface networkInterface)
	{
		int interfaceIndex = networkInterface.GetInterfaceIndex();
		if (interfaceIndex == -1)
		{
			return -1;
		}
		NetworkInterfaceHelper.InitializeIpInterfaceEntry(out var row);
		row.InterfaceIndex = (uint)interfaceIndex;
		row.Family = (networkInterface.Supports(NetworkInterfaceComponent.IPv4) ? NetworkInterfaceHelper.ADDRESS_FAMILY.AF_INET : NetworkInterfaceHelper.ADDRESS_FAMILY.AF_INET6);
		if (NetworkInterfaceHelper.GetIpInterfaceEntry(ref row) != 0)
		{
			return -1;
		}
		return (int)row.Metric;
	}

	public static void SetMetric(this NetworkInterface networkInterface, uint metric)
	{
		int interfaceIndex = networkInterface.GetInterfaceIndex();
		if (interfaceIndex < 0)
		{
			throw new InvalidOperationException("Unable to set metric.");
		}
		NetworkInterfaceHelper.InitializeIpInterfaceEntry(out var row);
		row.InterfaceIndex = (uint)interfaceIndex;
		row.Family = (networkInterface.Supports(NetworkInterfaceComponent.IPv4) ? NetworkInterfaceHelper.ADDRESS_FAMILY.AF_INET : NetworkInterfaceHelper.ADDRESS_FAMILY.AF_INET6);
		if (NetworkInterfaceHelper.GetIpInterfaceEntry(ref row) == 0)
		{
			if (metric == 0)
			{
				row.UseAutomaticMetric = true;
			}
			else
			{
				row.UseAutomaticMetric = false;
				row.Metric = metric;
			}
			NetworkInterfaceHelper.SetIpInterfaceEntry(in row);
		}
	}

	public static int GetInterfaceIndex(this NetworkInterface networkInterface)
	{
		try
		{
			IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
			if (networkInterface.Supports(NetworkInterfaceComponent.IPv4))
			{
				return iPProperties.GetIPv4Properties().Index;
			}
			if (networkInterface.Supports(NetworkInterfaceComponent.IPv6))
			{
				return iPProperties.GetIPv6Properties().Index;
			}
			return -1;
		}
		catch
		{
			return -1;
		}
	}
}
