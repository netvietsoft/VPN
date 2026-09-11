using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class NotAuthorizedException : HTTPException
{
	public override ErrorType Type { get; } = ErrorType.ApiNotAuthorizedError;

	public NotAuthorizedException(string message)
		: base(message)
	{
	}

	public NotAuthorizedException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
