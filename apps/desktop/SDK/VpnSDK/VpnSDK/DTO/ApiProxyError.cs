using VpnSDK.Private.API.DTO;

namespace VpnSDK.DTO;

public class ApiProxyError : ApiHttpError
{
	public string Hostname { get; internal set; }

	internal static ApiProxyError Create(ProxyError error)
	{
		return new ApiProxyError
		{
			RequestUri = error.RequestUri,
			HttpResponse = error.HttpResponse,
			ResponseTime = error.ResponseTime,
			SystemException = error.SystemException
		};
	}
}
