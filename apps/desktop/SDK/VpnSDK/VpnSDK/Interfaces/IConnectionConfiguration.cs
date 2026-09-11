using VpnSDK.Enums;

namespace VpnSDK.Interfaces;

public interface IConnectionConfiguration
{
	NetworkConnectionType ConnectionType { get; }

	VpnManagerType ManagerType { get; }
}
