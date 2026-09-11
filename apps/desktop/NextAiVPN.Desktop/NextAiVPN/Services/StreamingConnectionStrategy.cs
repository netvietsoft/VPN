using System;
using System.Threading.Tasks;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

internal class StreamingConnectionStrategy : IVpnConnectionStrategy
{
	private readonly SDKMonitor _monitor;

	public VpnType Mode => VpnType.Streaming;

	public StreamingConnectionStrategy(SDKMonitor monitor)
	{
		_monitor = monitor ?? throw new ArgumentNullException("monitor");
	}

	public Task ConnectAsync(ILocation location)
	{
		return _monitor.ConnectToStreamingServers(_monitor.StreamingLocation);
	}

	public async Task DisconnectAsync()
	{
		await _monitor.DisconnectFromStreaming();
		await _monitor.GetStreamingLocations();
	}
}
