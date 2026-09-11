using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class APIException : HTTPException
{
	public override ErrorType Type { get; } = ErrorType.ApiError;

	public APIException(string message)
		: base(message)
	{
	}

	public APIException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
