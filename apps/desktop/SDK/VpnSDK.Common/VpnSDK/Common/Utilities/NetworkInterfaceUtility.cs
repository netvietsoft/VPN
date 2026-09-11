using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace VpnSDK.Common.Utilities;

internal static class NetworkInterfaceUtility
{
	internal static IPAddress? GetLocalIpAddress()
	{
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (NetworkInterface networkInterface in allNetworkInterfaces)
		{
			if (IsInterfaceOperational(networkInterface))
			{
				IPAddress firstNonLoopbackIpAddressWithValidGateway = GetFirstNonLoopbackIpAddressWithValidGateway(networkInterface);
				if (firstNonLoopbackIpAddressWithValidGateway != null)
				{
					return firstNonLoopbackIpAddressWithValidGateway;
				}
			}
		}
		return null;
	}

	internal static (int interfaceIndex, IPAddress? gatewayAddress) GetLocalInterfaceInfo()
	{
		IPInterfaceProperties iPProperties = (GetLocalInterface() ?? throw new Exception("Couldn't retrieve the local interface.")).GetIPProperties();
		int index = iPProperties.GetIPv4Properties().Index;
		IPAddress item = iPProperties.GatewayAddresses.FirstOrDefault((GatewayIPAddressInformation ga) => ga.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
		return (interfaceIndex: index, gatewayAddress: item);
	}

	internal static NetworkInterface? GetLocalInterface()
	{
		_ = IPAddress.Any;
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (NetworkInterface networkInterface in allNetworkInterfaces)
		{
			if (networkInterface.OperationalStatus == OperationalStatus.Up)
			{
				GatewayIPAddressInformationCollection gatewayAddresses = networkInterface.GetIPProperties().GatewayAddresses;
				if (gatewayAddresses.Count > 0 && gatewayAddresses.Any((GatewayIPAddressInformation g) => !object.Equals(g.Address, IPAddress.Any)))
				{
					return networkInterface;
				}
			}
		}
		return null;
	}

	private static bool IsInterfaceOperational(NetworkInterface networkInterface)
	{
		return networkInterface.OperationalStatus == OperationalStatus.Up;
	}

	private static IPAddress? GetFirstNonLoopbackIpAddressWithValidGateway(NetworkInterface networkInterface)
	{
		IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
		if (HasValidGateway(iPProperties.GatewayAddresses))
		{
			return GetFirstNonLoopbackIpAddress(iPProperties.UnicastAddresses);
		}
		return null;
	}

	private static bool HasValidGateway(ICollection<GatewayIPAddressInformation> gatewayAddresses)
	{
		if (gatewayAddresses.Count > 0)
		{
			return gatewayAddresses.Any((GatewayIPAddressInformation g) => !object.Equals(g.Address, IPAddress.Any));
		}
		return false;
	}

	private static IPAddress? GetFirstNonLoopbackIpAddress(UnicastIPAddressInformationCollection unicastAddresses)
	{
		foreach (UnicastIPAddressInformation unicastAddress in unicastAddresses)
		{
			if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(unicastAddress.Address))
			{
				return unicastAddress.Address;
			}
		}
		return null;
	}
}
