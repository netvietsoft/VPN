using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VpnSDK.Private.API.Common.Extensions;
using VpnSDK.Private.API.Common.Helper;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Private.API.Utilities;

internal class HttpAPIHandler : DelegatingHandler
{
	public delegate void RequestDelegate(RawApiRequest request);

	public delegate void ResponseDelegate(RawApiResponse message);

	public delegate void HttpErrorDelegate(HttpError error);

	private static readonly ILogger _logger = LogProvider.GetLogger("VpnSDK::Private::API");

	private readonly RequestDelegate _requestDelegate;

	private readonly ResponseDelegate _responseDelegate;

	private readonly HttpErrorDelegate _httpErrorDelegate;

	private TimeSpan _clientTimespan;

	public HttpAPIHandler(HttpMessageHandler innerHandler = null, HttpErrorDelegate httpErrorDelegate = null, RequestDelegate requestDelegate = null, ResponseDelegate responseDelegate = null, TimeSpan timeout = default(TimeSpan))
		: base(innerHandler ?? new HttpClientHandler())
	{
		_requestDelegate = requestDelegate;
		_responseDelegate = responseDelegate;
		_httpErrorDelegate = httpErrorDelegate;
		_clientTimespan = timeout;
	}

	internal void SetApiTimeout(TimeSpan timespan)
	{
		_clientTimespan = timespan;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		RawApiRequest rawApiRequest = await RawApiRequest.Create(request).ConfigureAwait(continueOnCapturedContext: false);
		_requestDelegate?.Invoke(rawApiRequest);
		HttpResponseMessage response = null;
		Stopwatch responseStopwatch = Stopwatch.StartNew();
		using (CancellationTokenSource cts = GetCancellationTokenSource(request, cancellationToken))
		{
			_ = 1;
			try
			{
				_logger?.LogDebug(() => "Sending request from API handler.");
				response = await base.SendAsync(request, cts?.Token ?? cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				_logger?.LogDebug(() => "Received response from from API handler.");
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				_logger?.LogWarning("Operation cancelled in API handler due to a timeout.");
				responseStopwatch.Stop();
				_responseDelegate?.Invoke(RawApiResponse.CreateFailure(ex, rawApiRequest, responseStopwatch.Elapsed));
				_httpErrorDelegate(new HttpError
				{
					SystemException = ex,
					RequestUri = request.RequestUri,
					ResponseTime = responseStopwatch.Elapsed
				});
				throw;
			}
			catch (Exception ex2)
			{
				Exception ex3 = ex2;
				Exception e = ex3;
				_logger?.LogDebug(() => $"Exception in API handler. {e}");
				responseStopwatch.Stop();
				_responseDelegate?.Invoke(RawApiResponse.CreateFailure(e, rawApiRequest, responseStopwatch.Elapsed));
				_httpErrorDelegate(new HttpError
				{
					SystemException = e,
					RequestUri = request.RequestUri,
					ResponseTime = responseStopwatch.Elapsed
				});
				throw;
			}
			finally
			{
				request.Dispose();
				_logger?.LogDebug(() => "Request disposed in API handler.");
			}
		}
		responseStopwatch.Stop();
		RawApiResponse message = await RawApiResponse.Create(response, rawApiRequest, responseStopwatch.Elapsed).ConfigureAwait(continueOnCapturedContext: false);
		_logger?.LogDebug(() => "Response created in API handler.");
		_responseDelegate(message);
		if (!response.IsSuccessStatusCode && response.Content != null)
		{
			_httpErrorDelegate(new HttpError
			{
				HttpResponse = response,
				RequestUri = request.RequestUri,
				ResponseTime = responseStopwatch.Elapsed
			});
			byte[] array = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(continueOnCapturedContext: false);
			if (array.Length != 0 && array[0] == 123)
			{
				response.StatusCode = HttpStatusCode.OK;
			}
		}
		return response;
	}

	private CancellationTokenSource GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (_clientTimespan == Timeout.InfiniteTimeSpan || _clientTimespan == default(TimeSpan))
		{
			CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { cancellationToken });
			cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(60.0));
			ILogger logger = _logger;
			if (logger != null)
			{
				logger.LogDebug(() => "CancellationTokenSource set to 60 seconds.");
				return cancellationTokenSource;
			}
			return cancellationTokenSource;
		}
		CancellationTokenSource cancellationTokenSource2 = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { cancellationToken });
		cancellationTokenSource2.CancelAfter(_clientTimespan);
		ILogger logger2 = _logger;
		if (logger2 != null)
		{
			logger2.LogDebug(() => $"CancellationTokenSource set to {_clientTimespan.TotalSeconds} seconds.");
			return cancellationTokenSource2;
		}
		return cancellationTokenSource2;
	}
}
