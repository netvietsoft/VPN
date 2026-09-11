using System;

namespace VpnSDK.Private.API;

internal class VpnApiException : Exception
{
	public ApiError Error { get; private set; }

	public VpnApiException(ApiError error, string message)
		: base(message)
	{
		Error = error;
	}
}
