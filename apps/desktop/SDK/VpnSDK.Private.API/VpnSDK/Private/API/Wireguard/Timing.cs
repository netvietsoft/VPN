using System;
using System.Threading;

namespace VpnSDK.Private.API.Wireguard;

internal static class Timing
{
	private static Random random;

	static Timing()
	{
		random = new Random();
	}

	public static void Sleep()
	{
		Thread.Sleep(TimeSpan.FromTicks(random.Next(250)));
	}
}
