using System.Net;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace VpnSDK.Internal;

internal class ConnectionInfo : IConnectionInfo
{
	public ILocation Location { get; internal set; }

	public IConnectionConfiguration Configuration { get; internal set; }

	public NetworkConnectionType Protocol { get; internal set; }

	public IPAddress ServerIp { get; internal set; }

	public bool IsKillKwitchOn { get; internal set; }

	public bool IsDnsProtected { get; internal set; }

	public bool IsIPv6Protected { get; internal set; }

	public bool IsLanTrafficAllowed { get; internal set; }

	public bool IsAutomaticProtocolUsed { get; internal set; }
}
