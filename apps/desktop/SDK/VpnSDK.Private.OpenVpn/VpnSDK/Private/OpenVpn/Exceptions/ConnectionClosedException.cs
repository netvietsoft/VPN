namespace VpnSDK.Private.OpenVpn.Exceptions;

public class ConnectionClosedException : OpenVpnException
{
	public ConnectionClosedException(string message)
		: base(message)
	{
	}
}
