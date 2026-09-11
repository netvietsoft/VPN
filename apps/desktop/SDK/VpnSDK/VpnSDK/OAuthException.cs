using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class OAuthException : HTTPException
{
	public override ErrorType Type { get; } = ErrorType.ApiOAuthError;

	public OAuthException(string message)
		: base(message)
	{
	}

	public OAuthException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
