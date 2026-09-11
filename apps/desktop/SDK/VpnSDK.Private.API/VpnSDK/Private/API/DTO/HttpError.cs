using System;
using System.Net.Http;

namespace VpnSDK.Private.API.DTO;

public class HttpError
{
	public HttpResponseMessage HttpResponse { get; internal set; }

	public Uri RequestUri { get; internal set; }

	public Exception SystemException { get; internal set; }

	public TimeSpan? ResponseTime { get; internal set; }
}
