using System;
using VpnSDK.Enums;

namespace VpnSDK;

internal class ServerUnhealthyException : HTTPException
{
	public override ErrorType Type { get; } = ErrorType.ServerUnhealthy;

	public ServerUnhealthyException(string message)
		: base(message)
	{
	}

	public ServerUnhealthyException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
