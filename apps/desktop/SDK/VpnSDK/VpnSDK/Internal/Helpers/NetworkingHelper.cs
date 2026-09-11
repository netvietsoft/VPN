using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using VpnSDK.Common.Helpers;

namespace VpnSDK.Internal.Helpers;

internal static class NetworkingHelper
{
	private const int ProprietaryVirtualInterfaceType = 53;

	private static readonly string[] VpnFilterStrings = new string[2] { "vpn", "tap" };

	public static async Task WaitForNetworkAvailable(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken == CancellationToken.None)
		{
			cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(60.0)).Token;
		}
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				if (await IsNetworkAvailableAsync())
				{
					break;
				}
			}
			catch
			{
				Debugger.Break();
			}
			await Task.Delay(100);
		}
	}

	public static string GetBaseDomain(string host)
	{
		if (!host.Contains("://"))
		{
			host = "http://" + host;
		}
		return string.Join(".", ((IEnumerable<string>)new Uri(host).Host.Split(new char[1] { '.' })).Reverse().Take(2).Reverse());
	}

	public static string GetActiveVpnConnectionName()
	{
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (NetworkInterface adapter in allNetworkInterfaces)
		{
			if (adapter.OperationalStatus == OperationalStatus.Up && (adapter.NetworkInterfaceType == NetworkInterfaceType.Ppp || adapter.NetworkInterfaceType == NetworkInterfaceType.Tunnel || adapter.NetworkInterfaceType == (NetworkInterfaceType)53 || (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet && (VpnFilterStrings.Any((string filterString) => adapter.Description.ToLower().Contains(filterString)) || VpnFilterStrings.Any((string filterString) => adapter.Name.ToLower().Contains(filterString))))))
			{
				return adapter.Name + " " + adapter.Description;
			}
		}
		return null;
	}

	private static async Task<bool> IsNetworkAvailableAsync()
	{
		if (NetworkInterface.GetIsNetworkAvailable())
		{
			NetworkInterface[] allNetworkInterfacesSafely = NetworkInterfaceHelper.GetAllNetworkInterfacesSafely();
			foreach (NetworkInterface networkInterface in allNetworkInterfacesSafely)
			{
				if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback && !networkInterface.Description.Split((char[]?)null).Any((string x) => x.StartsWith("Virtual", StringComparison.OrdinalIgnoreCase)))
				{
					IPInterfaceStatistics iPStatistics = networkInterface.GetIPStatistics();
					if (iPStatistics.BytesReceived > 0 || iPStatistics.BytesSent > 0 || iPStatistics.UnicastPacketsReceived > 0 || iPStatistics.NonUnicastPacketsReceived > 0)
					{
						return true;
					}
				}
			}
		}
		try
		{
			using Ping pinger = new Ping();
			if ((await pinger.SendPingAsync("www.msftncsi.com")).Status == IPStatus.Success)
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}
}
