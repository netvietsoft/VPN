using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace VpnSDK.Internal.Configuration;

internal class RasConnectionConfiguration : IRasConnectionConfiguration, IConnectionConfiguration
{
	public NetworkConnectionType ConnectionType { get; set; }

	public VpnManagerType ManagerType => VpnManagerType.RAS;
}
