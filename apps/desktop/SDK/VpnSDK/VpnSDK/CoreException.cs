using System;

namespace VpnSDK;

public class CoreException : BaseSDKException
{
	public CoreException(string message)
		: base(message)
	{
	}

	public CoreException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
