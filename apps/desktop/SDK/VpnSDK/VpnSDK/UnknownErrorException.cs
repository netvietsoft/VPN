using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class UnknownErrorException : BaseSDKException
{
	public override ErrorType Type { get; } = ErrorType.Unknown;

	public UnknownErrorException(string message)
		: base(message)
	{
	}

	public UnknownErrorException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
