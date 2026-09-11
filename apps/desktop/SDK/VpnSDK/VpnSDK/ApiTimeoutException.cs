using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class ApiTimeoutException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.ApiTimeout;

	public ApiTimeoutException(string message)
		: base(message)
	{
	}

	public ApiTimeoutException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
