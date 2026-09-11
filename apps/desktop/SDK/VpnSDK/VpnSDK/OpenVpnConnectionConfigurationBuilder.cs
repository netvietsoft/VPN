using VpnSDK.Common.Settings;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Configuration;

namespace VpnSDK;

public class OpenVpnConnectionConfigurationBuilder : ConnectionConfigurationBuilderBase
{
	private OpenVpnCipherType _cipher = OpenVpnCipherType.AES_256_CBC;

	private bool _scramble;

	private NetworkProtocolType _protocol = NetworkProtocolType.UDP;

	private ushort _port = 1194;

	private bool _isCipherSpecified;

	private bool _isPortSpecified;

	public IConnectionConfiguration Build()
	{
		OpenVpnConnectionConfiguration openVpnConnectionConfiguration = new OpenVpnConnectionConfiguration();
		if (_cipher == OpenVpnCipherType.AES_256_CBC && _scramble && !_isCipherSpecified)
		{
			_cipher = OpenVpnCipherType.AES_128_CBC;
		}
		if (_scramble && !_isPortSpecified)
		{
			_port = 3074;
		}
		openVpnConnectionConfiguration.Cipher = _cipher;
		openVpnConnectionConfiguration.Scramble = _scramble;
		openVpnConnectionConfiguration.ProtocolType = _protocol;
		openVpnConnectionConfiguration.Port = _port;
		Validate();
		if (IsDoubleHopEnabled)
		{
			openVpnConnectionConfiguration.DoubleHopSettings = new DoubleHopSettings(IsDoubleHopEnabled, EntryLocation.CountryCode, EntryLocation.City, ExitLocation.CountryCode, ExitLocation.City, $"{openVpnConnectionConfiguration.ConnectionType}-{openVpnConnectionConfiguration.ProtocolType}".ToLower());
		}
		return openVpnConnectionConfiguration;
	}

	public OpenVpnConnectionConfigurationBuilder SetCipher(OpenVpnCipherType cipherType)
	{
		_cipher = cipherType;
		_isCipherSpecified = true;
		return this;
	}

	public OpenVpnConnectionConfigurationBuilder SetScramble(bool scramble)
	{
		_scramble = scramble;
		return this;
	}

	public OpenVpnConnectionConfigurationBuilder SetNetworkProtocol(NetworkProtocolType protocol)
	{
		_protocol = protocol;
		return this;
	}

	public OpenVpnConnectionConfigurationBuilder SetPort(ushort port)
	{
		_port = port;
		_isPortSpecified = true;
		return this;
	}

	public OpenVpnConnectionConfigurationBuilder SetDoubleHopSettings(bool isEnabled, ILocation entryLocation, ILocation exitLocation)
	{
		IsDoubleHopEnabled = isEnabled;
		EntryLocation = entryLocation;
		ExitLocation = exitLocation;
		return this;
	}
}
