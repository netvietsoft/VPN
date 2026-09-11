using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class VpnHostServiceFileNotFoundException : VpnException
{
	public override ErrorType Type => ErrorType.VpnHostServiceFileNotFound;

	public VpnHostServiceFileNotFoundException(string message)
		: base(message)
	{
	}

	public VpnHostServiceFileNotFoundException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
