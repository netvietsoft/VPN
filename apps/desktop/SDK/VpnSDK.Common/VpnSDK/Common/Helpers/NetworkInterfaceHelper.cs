using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Win32;
using VpnSDK.Common.Dto;

namespace VpnSDK.Common.Helpers;

public static class NetworkInterfaceHelper
{
	private const string NetworkAdapterRegistryPath = "SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}";

	private const string ComponentId = "ComponentId";

	private const string NetCfgInstanceId = "NetCfgInstanceId";

	public static NetworkInterface GetNetworkInterface(string hardwareid)
	{
		NetworkInterface[] allNetworkInterfacesSafely = GetAllNetworkInterfacesSafely();
		if (allNetworkInterfacesSafely.Length == 0)
		{
			return null;
		}
		IEnumerable<NetworkAdapterInfo> enumerable = from adapter in GetAllAdaptersFromRegistry()
			where adapter.ComponentId.Equals(hardwareid, StringComparison.InvariantCultureIgnoreCase)
			select adapter;
		if (enumerable == null || enumerable.Count() == 0)
		{
			return null;
		}
		NetworkInterface[] array = allNetworkInterfacesSafely;
		foreach (NetworkInterface networkInterface in array)
		{
			if (enumerable.Any((NetworkAdapterInfo adapter) => networkInterface.Id.Equals(adapter.InstanceId, StringComparison.OrdinalIgnoreCase)))
			{
				return networkInterface;
			}
		}
		return null;
	}

	public static NetworkInterface[] GetAllNetworkInterfacesSafely(int maxRetries = 5, int retryDelayMs = 400)
	{
		for (int i = 1; i <= maxRetries; i++)
		{
			try
			{
				NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				if (allNetworkInterfaces != null && allNetworkInterfaces.Length != 0)
				{
					return allNetworkInterfaces;
				}
			}
			catch (Exception)
			{
			}
			if (i < maxRetries)
			{
				Thread.Sleep(retryDelayMs);
			}
		}
		return Array.Empty<NetworkInterface>();
	}

	public static NetworkAdapterInfo[] GetAllAdaptersFromRegistry()
	{
		List<NetworkAdapterInfo> list = new List<NetworkAdapterInfo>();
		try
		{
			using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}");
			if (registryKey == null)
			{
				return list.ToArray();
			}
			string[] subKeyNames = registryKey.GetSubKeyNames();
			foreach (string text in subKeyNames)
			{
				if (text.Equals("Configuration", StringComparison.OrdinalIgnoreCase) || text.Equals("Properties", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				try
				{
					using RegistryKey registryKey2 = registryKey.OpenSubKey(text, writable: true);
					if (registryKey2 != null)
					{
						object? value = registryKey2.GetValue("ComponentId", string.Empty);
						object value2 = registryKey2.GetValue("NetCfgInstanceId", string.Empty);
						if (value is string componentId && value2 is string instanceId)
						{
							list.Add(new NetworkAdapterInfo
							{
								ComponentId = componentId,
								InstanceId = instanceId
							});
						}
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
		return list.ToArray();
	}
}
