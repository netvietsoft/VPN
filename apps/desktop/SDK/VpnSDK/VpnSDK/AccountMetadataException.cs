using System;
using VpnSDK.Enums;

namespace VpnSDK;

public class AccountMetadataException : CoreException
{
	public override ErrorType Type { get; } = ErrorType.AccountMetadataError;

	public AccountMetadataException(string message)
		: base(message)
	{
	}

	public AccountMetadataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
