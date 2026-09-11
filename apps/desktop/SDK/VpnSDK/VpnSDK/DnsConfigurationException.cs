using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class DnsConfigurationException : VpnException
{
	public override ErrorType Type { get; } = ErrorType.DnsConfigurationError;

	public DnsConfigurationException(string message)
		: base(message)
	{
	}

	public DnsConfigurationException(string message, Exception inner)
		: base(message, inner)
	{
		base.HResult = inner.HResult;
	}
}
