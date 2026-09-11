using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class FetchLocationsException : HTTPException
{
	public override ErrorType Type { get; } = ErrorType.ApiFetchLocationsError;

	public FetchLocationsException(string message)
		: base(message)
	{
	}

	public FetchLocationsException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
