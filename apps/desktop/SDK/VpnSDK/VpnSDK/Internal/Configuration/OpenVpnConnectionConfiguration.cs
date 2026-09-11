using VpnSDK.Common.Settings;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace VpnSDK.Internal.Configuration;

internal class OpenVpnConnectionConfiguration : IOpenVpnConnectionConfiguration, IConnectionConfiguration, IDoubleHopConfiguration
{
	public NetworkConnectionType ConnectionType { get; } = NetworkConnectionType.OpenVPN;

	public OpenVpnCipherType Cipher { get; set; }

	public NetworkProtocolType ProtocolType { get; set; }

	public bool Scramble { get; set; }

	public ushort Port { get; set; }

	public DoubleHopSettings DoubleHopSettings { get; set; }

	public VpnManagerType ManagerType => VpnManagerType.OpenVPN;
}
