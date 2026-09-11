namespace VpnSDK.Private.OpenVpn.Exceptions;

public class AuthenticationException : OpenVpnException
{
	public AuthenticationException(string message)
		: base(message)
	{
	}
}
