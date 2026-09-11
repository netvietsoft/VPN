using VpnSDK.Enums;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Configuration;

namespace VpnSDK;

public class RasConnectionConfigurationBuilder
{
	private NetworkConnectionType? _connectionType;

	public IConnectionConfiguration Build()
	{
		RasConnectionConfiguration rasConnectionConfiguration = new RasConnectionConfiguration();
		Validate();
		if (_connectionType.HasValue)
		{
			rasConnectionConfiguration.ConnectionType = _connectionType.Value;
		}
		return rasConnectionConfiguration;
	}

	public RasConnectionConfigurationBuilder SetConnectionType(NetworkConnectionType connectionType)
	{
		_connectionType = connectionType;
		return this;
	}

	private void Validate()
	{
		if (!_connectionType.HasValue)
		{
			throw new InvalidConfigurationException("Connection type is not configured.");
		}
	}
}
