using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using VpnSDK.Common.Interop;
using VpnSDK.Common.Utilities;

namespace VpnSDK.Common.Routing;

internal static class WindowsRoutingTableManager
{
	internal static RouteOperationResult AddRoutes(string vpnInterfaceName, int metric, List<RouteInfo> routes)
	{
		return AddOrRemoveRoutes(vpnInterfaceName, metric, routes, addRoute: true);
	}

	internal static RouteOperationResult DeleteRoutes(string vpnInterfaceName, int metric, List<RouteInfo> routes)
	{
		return AddOrRemoveRoutes(vpnInterfaceName, metric, routes, addRoute: false);
	}

	internal static void DeleteRoute(string destinationIp, string subNetMask, int interfaceIndex)
	{
		DeleteIpForwardRows(GetIpForwardRows(destinationIp, subNetMask, interfaceIndex));
	}

	internal static void DeleteRoute(string destinationIp, string subNetMask, int metric, int interfaceIndex)
	{
		DeleteIpForwardRows(GetIpForwardRows(destinationIp, subNetMask, metric, interfaceIndex));
	}

	internal static void DeleteRoute(int metric, int interfaceIndex)
	{
		DeleteIpForwardRows(GetIpForwardRows(metric, interfaceIndex));
	}

	internal static bool DoesCustomRouteExist(int metric, int interfaceIndex)
	{
		return GetIpForwardRows(metric, interfaceIndex).Any();
	}

	internal static bool CreateRouteIfNotExists(string destinationIp, string subNetMask, string gateway, int interfaceIndex, int metric)
	{
		if (RouteExists(destinationIp, subNetMask, interfaceIndex, metric))
		{
			return false;
		}
		AddRoute(destinationIp, subNetMask, gateway, interfaceIndex, metric);
		return true;
	}

	private static void DeleteIpForwardRows(List<MibIpForwardRow> ipForwardRows)
	{
		nint num = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MibIpForwardRow)));
		try
		{
			foreach (MibIpForwardRow ipForwardRow in ipForwardRows)
			{
				Marshal.StructureToPtr(ipForwardRow, num, fDeleteOld: false);
				int num2 = IpHelperInterop.DeleteIpForwardEntry(num);
				if (num2 != 1168 && num2 != 0)
				{
					Win32Exception ex = new Win32Exception(num2);
					throw new InvalidOperationException($"Unable to delete windows route. The error code is {num2} and exception is {ex.ToString()}");
				}
			}
		}
		finally
		{
			Marshal.FreeHGlobal(num);
		}
	}

	private static List<MibIpForwardRow> GetIpForwardRows(int metric, int interfaceIndex)
	{
		return GetRoutingTable().Table.Where((MibIpForwardRow t) => t.DwForwardMetric1.Equals(Convert.ToUInt32(metric)) && t.DwForwardIfIndex.Equals(Convert.ToUInt32(interfaceIndex))).ToList();
	}

	private static List<MibIpForwardRow> GetIpForwardRows(string destinationIp, string subNetMask, int metric, int interfaceIndex)
	{
		return GetRoutingTable().Table.Where((MibIpForwardRow t) => t.DwForwardDest.Equals(IpAddressUtility.ConvertIpAddressToUInt32(destinationIp)) && t.DwForwardMask.Equals(IpAddressUtility.ConvertIpAddressToUInt32(subNetMask)) && t.DwForwardMetric1.Equals(Convert.ToUInt32(metric)) && t.DwForwardIfIndex.Equals(Convert.ToUInt32(interfaceIndex))).ToList();
	}

	private static List<MibIpForwardRow> GetIpForwardRows(string destinationIp, string subNetMask, int interfaceIndex)
	{
		return GetRoutingTable().Table.Where((MibIpForwardRow t) => t.DwForwardDest.Equals(IpAddressUtility.ConvertIpAddressToUInt32(destinationIp)) && t.DwForwardMask.Equals(IpAddressUtility.ConvertIpAddressToUInt32(subNetMask)) && t.DwForwardIfIndex.Equals(Convert.ToUInt32(interfaceIndex))).ToList();
	}

	private static RouteOperationResult AddOrRemoveRoutes(string vpnInterfaceName, int metric, List<RouteInfo> routes, bool addRoute)
	{
		if (string.IsNullOrEmpty(vpnInterfaceName))
		{
			return RouteOperationResult.EmptyInterfaceName;
		}
		NetworkInterface vpnInterfaceByName = GetVpnInterfaceByName(vpnInterfaceName);
		if (vpnInterfaceByName == null)
		{
			return RouteOperationResult.InterfaceNotFound;
		}
		if (vpnInterfaceByName.NetworkInterfaceType == NetworkInterfaceType.Loopback)
		{
			return RouteOperationResult.LoopbackInterface;
		}
		string vpnGatewayIp = GetVpnGatewayIp(vpnInterfaceByName);
		int vpnInterfaceIndex = GetVpnInterfaceIndex(vpnInterfaceByName);
		foreach (RouteInfo route in routes)
		{
			if (addRoute)
			{
				AddRoute(route.DestinationIpAddress.ToString(), route.SubnetMask.ToString(), vpnGatewayIp, vpnInterfaceIndex, metric);
			}
			else
			{
				DeleteRoute(route.DestinationIpAddress.ToString(), route.SubnetMask.ToString(), metric, vpnInterfaceIndex);
			}
		}
		return RouteOperationResult.Success;
	}

	private static bool RouteExists(string destinationIp, string subNetMask, int interfaceIndex, int metric)
	{
		return GetRoutingTable().Table.Where((MibIpForwardRow t) => t.DwForwardDest.Equals(IpAddressUtility.ConvertIpAddressToUInt32(destinationIp)) && t.DwForwardMask.Equals(IpAddressUtility.ConvertIpAddressToUInt32(subNetMask)) && t.DwForwardMetric1.Equals(Convert.ToUInt32(metric)) && t.DwForwardIfIndex.Equals(Convert.ToUInt32(interfaceIndex))).ToList().Any();
	}

	private static void AddRoute(string destinationIp, string subNetMask, string? gateway, int interfaceIndex, int metric)
	{
		MibIpForwardRow structure = new MibIpForwardRow(destinationIp, subNetMask, gateway, interfaceIndex, metric);
		nint num = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
		try
		{
			Marshal.StructureToPtr(structure, num, fDeleteOld: false);
			int num2 = IpHelperInterop.CreateIpForwardEntry(num);
			if (num2 == 5010 || num2 == 0)
			{
				return;
			}
			Win32Exception ex = new Win32Exception(num2);
			throw new InvalidOperationException($"Unable to add windows route. The error code is {num2} and exception is {ex.ToString()}");
		}
		finally
		{
			Marshal.FreeHGlobal(num);
		}
	}

	private static MibIpForwardTable GetRoutingTable()
	{
		nint num = IntPtr.Zero;
		try
		{
			int pdwSize = 0;
			IpHelperInterop.GetIpForwardTable(num, ref pdwSize, bOrder: true);
			num = Marshal.AllocHGlobal(pdwSize);
			IpHelperInterop.GetIpForwardTable(num, ref pdwSize, bOrder: true);
			return ParseIpForwardTablePtr(num);
		}
		finally
		{
			if (num != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(num);
			}
		}
	}

	private static MibIpForwardTable ParseIpForwardTablePtr(nint tablePtr)
	{
		int num = Marshal.SizeOf(typeof(MibIpForwardTable));
		MibIpForwardTable result = Marshal.PtrToStructure<MibIpForwardTable>(tablePtr);
		MibIpForwardRow[] array = new MibIpForwardRow[result.Size];
		nint num2 = new IntPtr(((IntPtr)tablePtr).ToInt64() + num);
		for (int i = 0; i < result.Size; i++)
		{
			array[i] = Marshal.PtrToStructure<MibIpForwardRow>(num2);
			num2 = IntPtr.Add(num2, Marshal.SizeOf(typeof(MibIpForwardRow)));
		}
		result.Table = array;
		return result;
	}

	private static NetworkInterface? GetVpnInterfaceByName(string vpnInterfaceName)
	{
		return NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault((NetworkInterface x) => x.Name.Equals(vpnInterfaceName, StringComparison.OrdinalIgnoreCase));
	}

	private static string? GetVpnGatewayIp(NetworkInterface vpnInterface)
	{
		return vpnInterface.GetIPProperties().GatewayAddresses.FirstOrDefault()?.Address.ToString();
	}

	private static int GetVpnInterfaceIndex(NetworkInterface vpnInterface)
	{
		return vpnInterface.GetIPProperties().GetIPv4Properties().Index;
	}
}
