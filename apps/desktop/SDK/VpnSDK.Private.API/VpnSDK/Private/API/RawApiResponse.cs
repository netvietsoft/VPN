using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VpnSDK.Private.API.Common.Extensions;
using VpnSDK.Private.API.Common.Helper;

namespace VpnSDK.Private.API;

[Serializable]
internal class RawApiResponse
{
	private static readonly ILogger _logger = LogProvider.GetLogger("VpnSDK::Private::API");

	private const int TimeoutInSeconds = 20;

	private const string UnknownUri = "Unknown URI";

	public string Content { get; private set; }

	public Dictionary<string, string> Headers { get; private set; }

	public bool Success { get; private set; }

	public HttpStatusCode StatusCode { get; private set; }

	public TimeSpan ResponseTime { get; private set; }

	public RawApiRequest Request { get; private set; }

	private RawApiResponse()
	{
	}

	internal static RawApiResponse CreateFailure(Exception e, RawApiRequest request = null, TimeSpan? failureTime = null)
	{
		RawApiResponse rawApiResponse = new RawApiResponse
		{
			Success = false,
			Content = e.Message,
			Request = request
		};
		if (failureTime.HasValue)
		{
			rawApiResponse.ResponseTime = failureTime.Value;
		}
		return rawApiResponse;
	}

	internal static async Task<RawApiResponse> Create(HttpResponseMessage message, RawApiRequest request = null, TimeSpan? responseTime = null)
	{
		RawApiResponse res = new RawApiResponse
		{
			Headers = message.Headers.ToDictionary<KeyValuePair<string, IEnumerable<string>>, string, string>((KeyValuePair<string, IEnumerable<string>> a) => a.Key, (KeyValuePair<string, IEnumerable<string>> a) => string.Join(";", a.Value)),
			Success = message.IsSuccessStatusCode,
			StatusCode = message.StatusCode
		};
		if (responseTime.HasValue)
		{
			res.ResponseTime = responseTime.Value;
		}
		if (message.Content != null)
		{
			try
			{
				_logger?.LogDebug(() => "Start reading response in RawAPI.");
				TimeSpan delay = TimeSpan.FromSeconds(20.0);
				Task<string> readTask = message.Content.ReadAsStringAsync();
				Task timeoutTask = Task.Delay(delay);
				if (await Task.WhenAny(new Task[2] { readTask, timeoutTask }).ConfigureAwait(continueOnCapturedContext: false) == timeoutTask)
				{
					string text = request?.Uri?.AbsoluteUri ?? "Unknown URI";
					_logger?.LogError("Timeout occurred when reading the response. Requested URI: " + text);
					throw new TimeoutException("Operation timeout while reading the content in RawAPI");
				}
				RawApiResponse rawApiResponse = res;
				rawApiResponse.Content = await readTask.ConfigureAwait(continueOnCapturedContext: false);
				_logger?.LogDebug(() => "Finished reading response in RawAPI.");
			}
			catch (Exception arg)
			{
				_logger?.LogError($"Exception occurred when creating the raw api response. {arg}");
				throw;
			}
		}
		res.Request = request;
		return res;
	}
}
