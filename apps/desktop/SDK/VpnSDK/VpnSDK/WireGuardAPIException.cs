using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class WireGuardAPIException : HTTPException
{
	public override ErrorType Type { get; } = ErrorType.WireGuardApiError;

	public WireGuardAPIException(string message)
		: base(message)
	{
	}

	public WireGuardAPIException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
