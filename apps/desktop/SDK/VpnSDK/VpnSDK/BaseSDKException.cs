using System;
using VpnSDK.Enums;

namespace VpnSDK;

public abstract class BaseSDKException : Exception
{
	public virtual ErrorType Type { get; }

	public BaseSDKException(string message)
		: base(message)
	{
	}

	public BaseSDKException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
