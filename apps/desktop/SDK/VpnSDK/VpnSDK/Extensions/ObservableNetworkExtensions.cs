using System;
using System.Net.NetworkInformation;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using VpnSDK.DTO;

namespace VpnSDK.Extensions;

internal static class ObservableNetworkExtensions
{
	public static IObservable<TotalDataTransfer> ObserveDataUsageFromStart(this NetworkInterface networkInterface, TimeSpan checkRate, IScheduler scheduler = null)
	{
		if (scheduler == null)
		{
			scheduler = Scheduler.Default.DisableOptimizations();
		}
		TotalDataTransfer info = new TotalDataTransfer(0L, 0L);
		try
		{
			IPInterfaceStatistics iPStatistics = networkInterface.GetIPStatistics();
			info = new TotalDataTransfer(iPStatistics.BytesSent, iPStatistics.BytesReceived);
		}
		catch
		{
		}
		return Observable.Interval(checkRate, scheduler).StartWith(-1L).Select((Func<long, TotalDataTransfer>)delegate
		{
			try
			{
				IPInterfaceStatistics iPStatistics2 = networkInterface.GetIPStatistics();
				return new TotalDataTransfer(iPStatistics2.BytesSent - info.Sent, iPStatistics2.BytesReceived - info.Received);
			}
			catch
			{
				return (TotalDataTransfer)null;
			}
		});
	}
}
