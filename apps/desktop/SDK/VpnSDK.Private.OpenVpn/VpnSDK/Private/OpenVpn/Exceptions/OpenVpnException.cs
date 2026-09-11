using System;

namespace VpnSDK.Private.OpenVpn.Exceptions;

public class OpenVpnException : Exception
{
	public OpenVpnException(string message)
		: base(message)
	{
	}
}
