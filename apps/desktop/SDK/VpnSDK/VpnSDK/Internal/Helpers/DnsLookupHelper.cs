using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace VpnSDK.Internal.Helpers;

internal static class DnsLookupHelper
{
	public static async Task WaitForDnsResolution()
	{
		for (int i = 0; i < 10; i++)
		{
			using CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(0.5 * (double)(i + 1)));
			try
			{
				await Dns.GetHostEntryAsync("example.com").WithCancellation(source.Token).ConfigureAwait(continueOnCapturedContext: false);
				break;
			}
			catch
			{
			}
		}
	}
}
