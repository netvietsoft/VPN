using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class ServerListException : HTTPException
{
	public override ErrorType Type { get; } = ErrorType.ServerListError;

	public ServerListException(string message)
		: base(message)
	{
	}

	public ServerListException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
