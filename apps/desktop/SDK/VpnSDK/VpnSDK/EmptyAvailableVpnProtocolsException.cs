using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class EmptyAvailableVpnProtocolsException : CoreException
{
	public override ErrorType Type { get; } = ErrorType.CoreNoAvailableProtocols;

	public EmptyAvailableVpnProtocolsException(string message)
		: base(message)
	{
	}

	public EmptyAvailableVpnProtocolsException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
