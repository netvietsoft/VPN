using System.Net;
using VpnSDK.Enums;

namespace VpnSDK.Interfaces;

public interface IConnectionInfo
{
	ILocation Location { get; }

	IConnectionConfiguration Configuration { get; }

	NetworkConnectionType Protocol { get; }

	IPAddress ServerIp { get; }

	bool IsKillKwitchOn { get; }

	bool IsDnsProtected { get; }

	bool IsIPv6Protected { get; }

	bool IsLanTrafficAllowed { get; }

	bool IsAutomaticProtocolUsed { get; }
}
