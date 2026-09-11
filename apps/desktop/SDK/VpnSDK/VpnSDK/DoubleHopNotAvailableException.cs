using System;
using VpnSDK.Enums;
using VpnSDK.Private.API;

namespace VpnSDK;

public class DoubleHopNotAvailableException : DoubleHopException
{
	public override ErrorType Type { get; } = ErrorType.DoublehopNotAvailable;

	public DoubleHopNotAvailableException(string message)
		: base(message)
	{
	}

	public DoubleHopNotAvailableException(string message, Exception inner)
		: base(message, inner)
	{
	}

	internal DoubleHopNotAvailableException(string message, ApiError error)
		: base(message, error)
	{
	}
}
