using VpnSDK.Common.Settings;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Configuration;

namespace VpnSDK;

public class WireGuardConnectionConfigurationBuilder : ConnectionConfigurationBuilderBase
{
	private const int MinMtu = 1300;

	private const int MaxMtu = 65535;

	private const short MinServiceStartTimeout = 10;

	private const short MaxServiceStartTimeout = 60;

	private bool _blockUntunneledTraffic;

	private int? _mtu;

	private short _serviceStartTimeout = 30;

	public IConnectionConfiguration Build()
	{
		WireGuardConnectionConfiguration wireGuardConnectionConfiguration = new WireGuardConnectionConfiguration
		{
			AllowLan = !_blockUntunneledTraffic
		};
		Validate();
		wireGuardConnectionConfiguration.Mtu = _mtu;
		wireGuardConnectionConfiguration.ServiceStartTimeoutInSeconds = _serviceStartTimeout;
		if (IsDoubleHopEnabled)
		{
			wireGuardConnectionConfiguration.DoubleHopSettings = new DoubleHopSettings(IsDoubleHopEnabled, EntryLocation.CountryCode, EntryLocation.City, ExitLocation.CountryCode, ExitLocation.City, wireGuardConnectionConfiguration.ConnectionType.ToString().ToLower());
		}
		return wireGuardConnectionConfiguration;
	}

	public WireGuardConnectionConfigurationBuilder SetBlockUntunneledTraffic(bool blockUntunneledTraffic)
	{
		_blockUntunneledTraffic = blockUntunneledTraffic;
		return this;
	}

	public WireGuardConnectionConfigurationBuilder SetServiceStartTimeout(short serviceStartTimeout)
	{
		_serviceStartTimeout = serviceStartTimeout;
		return this;
	}

	public WireGuardConnectionConfigurationBuilder SetMtu(int? mtu)
	{
		_mtu = mtu;
		return this;
	}

	public WireGuardConnectionConfigurationBuilder SetDoubleHopSettings(bool isEnabled, ILocation entryLocation, ILocation exitLocation)
	{
		IsDoubleHopEnabled = isEnabled;
		EntryLocation = entryLocation;
		ExitLocation = exitLocation;
		return this;
	}

	protected override void Validate()
	{
		base.Validate();
		if (_mtu.HasValue && (_mtu < 1300 || _mtu > 65535))
		{
			throw new InvalidConfigurationException($"Invalid MTU value. MTU should be between {1300} and {65535}.");
		}
		if (_serviceStartTimeout < 10 || _serviceStartTimeout > 60)
		{
			throw new InvalidConfigurationException($"Invalid WireGuard service start timeout value. Timeout in seconds should be between {(short)10} and {(short)60}.");
		}
	}
}
