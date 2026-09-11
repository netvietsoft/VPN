using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class InvalidConfigurationException : CoreException
{
	public override ErrorType Type { get; } = ErrorType.CoreInvalidConfiguration;

	public InvalidConfigurationException(string message)
		: base(message)
	{
	}

	public InvalidConfigurationException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
