using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class ReconnectOnPowerEventException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.ReconnectOnPowerEventException;

	public ReconnectOnPowerEventException(string message)
		: base(message)
	{
	}

	public ReconnectOnPowerEventException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
