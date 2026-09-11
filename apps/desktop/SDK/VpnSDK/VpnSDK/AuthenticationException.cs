using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class AuthenticationException : APIException
{
	public override ErrorType Type { get; } = ErrorType.ApiAuthenticationError;

	public AuthenticationException(string message)
		: base(message)
	{
	}

	public AuthenticationException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
