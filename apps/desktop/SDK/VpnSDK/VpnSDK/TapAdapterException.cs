using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class TapAdapterException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.TAPAdapterError;

	public TapAdapterException(string message)
		: base(message)
	{
	}

	public TapAdapterException(string message, Exception inner)
		: base(message, inner)
	{
		base.HResult = inner.HResult;
	}
}
