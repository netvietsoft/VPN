using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class NotElevatedException : CoreException
{
	public override ErrorType Type { get; } = ErrorType.CoreNotElevated;

	public NotElevatedException(string message)
		: base(message)
	{
	}

	public NotElevatedException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
