using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class VpnHostServiceException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.WireGuardApiError;

	public VpnHostServiceException(string message)
		: base(message)
	{
	}

	public VpnHostServiceException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
