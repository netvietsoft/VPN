using VpnSDK.Common.Settings;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace VpnSDK.Internal.Configuration;

internal class WireGuardConnectionConfiguration : IWireGuardConnectionConfiguration, IConnectionConfiguration, IDoubleHopConfiguration
{
	public NetworkConnectionType ConnectionType { get; } = NetworkConnectionType.WireGuard;

	public VpnManagerType ManagerType => VpnManagerType.WireGuard;

	public bool AllowLan { get; set; }

	public int? Mtu { get; set; }

	public short ServiceStartTimeoutInSeconds { get; set; }

	public DoubleHopSettings DoubleHopSettings { get; set; }
}
