using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class InvalidAccountException : HTTPException
{
	public AccountStatus? AccountStatus { get; }

	public override ErrorType Type { get; } = ErrorType.InvalidAccountError;

	public InvalidAccountException(string message, AccountStatus? accountStatus = null)
		: base(message)
	{
		AccountStatus = accountStatus;
	}

	public InvalidAccountException(string message, Exception inner, AccountStatus? accountStatus = null)
		: base(message, inner)
	{
		AccountStatus = accountStatus;
	}
}
