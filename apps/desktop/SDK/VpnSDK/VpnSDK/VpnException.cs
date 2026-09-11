using System;

namespace VpnSDK;

public class VpnException : BaseSDKException
{
	public VpnException(string message)
		: base(message)
	{
	}

	public VpnException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
