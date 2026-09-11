namespace VpnSDK.Private.OpenVpn.Exceptions;

public class TlsHandshakeException : OpenVpnException
{
	public TlsHandshakeException(string message)
		: base(message)
	{
	}
}
