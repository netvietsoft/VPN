using System;

namespace VpnSDK;

public class HTTPException : BaseSDKException
{
	public HTTPException(string message)
		: base(message)
	{
	}

	public HTTPException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
