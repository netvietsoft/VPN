using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace VpnSDK.Internal.Helpers;

internal static class PingHelper
{
	public static Task<ushort?> Ping(this IPAddress address)
	{
		return PingAndUpdateAsync(null, address);
	}

	public static Task<ushort?> Ping(this string hostname)
	{
		return PingAndUpdateAsync(hostname);
	}

	public static Task<ushort?> Ping(this Uri uri)
	{
		if (uri != null)
		{
			return uri.DnsSafeHost.Ping();
		}
		return null;
	}

	public static async Task<ushort?> PingAndUpdateAsync(string ipOrHostname = null, IPAddress address = null, int timeout = 2500)
	{
		_ = 1;
		try
		{
			using Ping p = new Ping();
			if (string.IsNullOrEmpty(ipOrHostname) && address == null)
			{
				return null;
			}
			PingReply pingReply = ((address == null) ? (await p.SendPingAsync(ipOrHostname, timeout).ConfigureAwait(continueOnCapturedContext: false)) : (await p.SendPingAsync(address, timeout).ConfigureAwait(continueOnCapturedContext: false)));
			if (pingReply.Status != IPStatus.Success)
			{
				return null;
			}
			if (pingReply.RoundtripTime > 65535)
			{
				return null;
			}
			return Convert.ToUInt16(pingReply.RoundtripTime);
		}
		catch
		{
			return null;
		}
	}

	public static void PingSendAsync(string ip)
	{
		using Ping ping = new Ping();
		try
		{
			ping.SendAsync(ip, 5000);
		}
		catch (Exception)
		{
		}
	}
}
