using System;
using VpnSDK.Enums;
using VpnSDK.Private.API;

namespace VpnSDK;

public class DoubleHopException : APIException
{
	public override ErrorType Type { get; }

	public DoubleHopException(string message)
		: base(message)
	{
	}

	public DoubleHopException(string message, Exception inner)
		: base(message, inner)
	{
	}

	internal DoubleHopException(string message, ApiError error)
		: base(message)
	{
	}
}
