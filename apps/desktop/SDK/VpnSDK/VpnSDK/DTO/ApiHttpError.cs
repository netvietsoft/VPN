using System;
using System.Net.Http;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.DTO;

public class ApiHttpError : ISDKError
{
	public HttpResponseMessage HttpResponse { get; internal set; }

	public Uri RequestUri { get; internal set; }

	public TimeSpan? ResponseTime { get; internal set; }

	public Exception SystemException { get; internal set; }

	internal static ApiHttpError Create(HttpError error)
	{
		return new ApiHttpError
		{
			RequestUri = error.RequestUri,
			HttpResponse = error.HttpResponse,
			ResponseTime = error.ResponseTime,
			SystemException = error.SystemException
		};
	}
}
