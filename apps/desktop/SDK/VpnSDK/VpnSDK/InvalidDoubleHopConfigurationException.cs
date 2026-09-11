using System;
using VpnSDK.Enums;
using VpnSDK.Private.API;

namespace VpnSDK;

public class InvalidDoubleHopConfigurationException : DoubleHopException
{
	public override ErrorType Type { get; } = ErrorType.InvalidDoubleHopConfiguration;

	public InvalidDoubleHopConfigurationException(string message)
		: base(message)
	{
	}

	public InvalidDoubleHopConfigurationException(string message, Exception inner)
		: base(message, inner)
	{
	}

	internal InvalidDoubleHopConfigurationException(string message, ApiError error)
		: base(message, error)
	{
	}
}
