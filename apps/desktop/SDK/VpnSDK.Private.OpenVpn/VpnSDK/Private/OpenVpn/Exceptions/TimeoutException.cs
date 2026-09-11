namespace VpnSDK.Private.OpenVpn.Exceptions;

public class TimeoutException : OpenVpnException
{
	public TimeoutException(string message)
		: base(message)
	{
	}
}
