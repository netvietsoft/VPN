using System.Runtime.InteropServices;

namespace VpnSDK.Internal.Helpers;

internal static class NetworkInterop
{
	private static class Win32NetworkInterop
	{
		[DllImport("dnsapi.dll")]
		public static extern uint DnsFlushResolverCache();
	}

	public static void FlushDnsCache()
	{
		try
		{
			Win32NetworkInterop.DnsFlushResolverCache();
		}
		catch
		{
		}
	}
}
