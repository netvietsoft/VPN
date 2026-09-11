using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class EndpointsUnreachableException : APIException
{
	public override ErrorType Type { get; } = ErrorType.AllApiEndpointsUnreachable;

	public EndpointsUnreachableException(string message)
		: base(message)
	{
	}

	public EndpointsUnreachableException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
