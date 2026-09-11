using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class ConnectionTerminatedException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.VpnConnectionUnexpectedlyDisconnected;

	public ConnectionTerminatedException(string message)
		: base(message)
	{
	}

	public ConnectionTerminatedException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
