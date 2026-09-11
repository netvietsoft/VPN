using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class ConnectionAlreadyEstablishedException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.VpnAlreadyConnected;

	public ConnectionAlreadyEstablishedException(string message)
		: base(message)
	{
	}

	public ConnectionAlreadyEstablishedException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
