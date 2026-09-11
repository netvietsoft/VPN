using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class VPNAuthenticationException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.VpnAuthenticationError;

	public VPNAuthenticationException(string message)
		: base(message)
	{
	}

	public VPNAuthenticationException(string message, Exception inner)
		: base(message, inner)
	{
		base.HResult = inner.HResult;
	}
}
