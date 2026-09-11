using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace VpnSDK.Private.API.Common.Extensions;

internal static class LogExtensions
{
	public static void LogDebug(this ILogger logger, Func<string> messageFunc)
	{
		if (logger.IsEnabled(LogLevel.Debug))
		{
			string message = $"({Thread.CurrentThread.ManagedThreadId}) {messageFunc()}";
			logger.LogDebug(message);
		}
	}
}
