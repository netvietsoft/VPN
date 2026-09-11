using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class ConnectionException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.VpnConnectionError;

	public ConnectionException(string message)
		: base(message)
	{
	}

	public ConnectionException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
