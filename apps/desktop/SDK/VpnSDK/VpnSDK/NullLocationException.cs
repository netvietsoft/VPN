using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class NullLocationException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.VpnLocationIsNull;

	public NullLocationException(string message)
		: base(message)
	{
	}

	public NullLocationException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
