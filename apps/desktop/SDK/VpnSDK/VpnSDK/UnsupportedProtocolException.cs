using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class UnsupportedProtocolException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.VpnUnsupportedProtocolError;

	public UnsupportedProtocolException(string message)
		: base(message)
	{
	}

	public UnsupportedProtocolException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
