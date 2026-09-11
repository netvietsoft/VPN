using System;

namespace VpnSDK;

public class WFPException : BaseSDKException
{
	public WFPException(string message)
		: base(message)
	{
	}

	public WFPException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
