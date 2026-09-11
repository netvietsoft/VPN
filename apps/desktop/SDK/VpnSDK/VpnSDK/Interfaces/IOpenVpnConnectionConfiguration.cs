using VpnSDK.Enums;

namespace VpnSDK.Interfaces;

public interface IOpenVpnConnectionConfiguration : IConnectionConfiguration, IDoubleHopConfiguration
{
	OpenVpnCipherType Cipher { get; set; }

	NetworkProtocolType ProtocolType { get; set; }

	bool Scramble { get; set; }

	ushort Port { get; set; }
}
