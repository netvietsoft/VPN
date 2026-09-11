using System;
using System.Threading.Tasks;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

internal class NextAiVpnConnectionStrategy : IVpnConnectionStrategy
{
	private readonly SDKMonitor _monitor;

	public VpnType Mode => VpnType.NextAiVPN;

	public NextAiVpnConnectionStrategy(SDKMonitor monitor)
	{
		_monitor = monitor ?? throw new ArgumentNullException("monitor");
	}

	public Task ConnectAsync(ILocation location)
	{
		return _monitor.ConnectToNextAiVpnServers(location);
	}

	public Task DisconnectAsync()
	{
		return _monitor.DisconnectFromGateway();
	}
}
