using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class InvalidServerException : HTTPException
{
	public override ErrorType Type { get; } = ErrorType.InvalidServerError;

	public InvalidServerException(string message)
		: base(message)
	{
	}

	public InvalidServerException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
