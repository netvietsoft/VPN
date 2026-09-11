using System;
using System.Net.Http;

namespace VpnSDK.Private.API;

internal class WireguardApiException : Exception
{
	public HttpResponseMessage ResponseMessage { get; }

	public WireguardApiException(string message, HttpResponseMessage response = null)
		: base(message)
	{
		ResponseMessage = response;
	}

	public WireguardApiException(string message, Exception innerException, HttpResponseMessage response = null)
		: base(message, innerException)
	{
		ResponseMessage = response;
	}
}
