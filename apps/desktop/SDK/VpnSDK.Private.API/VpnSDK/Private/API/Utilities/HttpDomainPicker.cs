using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VpnSDK.Private.API.Common.Extensions;
using VpnSDK.Private.API.Common.Helper;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Private.API.Utilities;

internal class HttpDomainPicker : DelegatingHandler
{
	public delegate void ProxyErrorDelegate(ProxyError error);

	private readonly ProxyErrorDelegate _proxyErrorDelegate;

	private readonly Dictionary<string, string> _headers;

	private static readonly ILogger _logger = LogProvider.GetLogger("VpnSDK::Private::API");

	private readonly TimeSpan _reEvaluateServersTime = TimeSpan.FromMinutes(15.0);

	private readonly List<string> _baseUrls;

	private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

	private Uri _currentUrl;

	private readonly Uri _mainUrl;

	private DateTime _lastServerRaceDateTime = DateTime.Now;

	private volatile bool _serverRaced;

	public Uri OverrideUrlForRequest { get; set; }

	public HttpDomainPicker(List<string> baseUrls, ProxyErrorDelegate proxyErrorDelegate, Dictionary<string, string> headers, HttpMessageHandler innerHandler = null)
		: base(innerHandler ?? new HttpClientHandler())
	{
		_proxyErrorDelegate = proxyErrorDelegate;
		_baseUrls = baseUrls;
		_currentUrl = new Uri(_baseUrls.First());
		_mainUrl = new Uri(_baseUrls.First());
		_headers = headers;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		TimeSpan timeSpan = DateTime.Now - _lastServerRaceDateTime;
		if (_serverRaced && timeSpan.TotalMinutes > _reEvaluateServersTime.TotalMinutes)
		{
			_lastServerRaceDateTime = DateTime.Now;
			_serverRaced = false;
		}
		try
		{
			_logger?.LogDebug(() => "Waiting on semaphore");
			await Semaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
			if (OverrideUrlForRequest != null && !string.IsNullOrEmpty(OverrideUrlForRequest.ToString()))
			{
				try
				{
					return await SendRequestWithOverrideUrl(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				finally
				{
					OverrideUrlForRequest = null;
					_logger?.LogDebug(() => "Releasing semaphore.");
					Semaphore.Release();
					_logger?.LogDebug(() => "Semaphore released.");
				}
			}
			_logger?.LogDebug(() => $"Semaphore wait complete. Servers to race: {_baseUrls.Count} | Should Race: {!_serverRaced}");
			if (_baseUrls.Count > 1 && !_serverRaced)
			{
				await RaceServers();
			}
		}
		catch (VpnApiException ex) when (ex.Error.Equals(ApiError.AllApiEndpointsUnreachable))
		{
			_serverRaced = false;
			_logger?.LogDebug(() => "Releasing semaphore due to VpnApiException.");
			Semaphore.Release();
			_logger?.LogDebug(() => "Semaphore released due to VpnApiException.");
			throw;
		}
		catch
		{
			_serverRaced = false;
		}
		try
		{
			return await SendRequest(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch
		{
			_serverRaced = false;
			throw;
		}
		finally
		{
			_logger?.LogDebug(() => "Releasing semaphore.");
			Semaphore.Release();
			_logger?.LogDebug(() => "Semaphore released.");
		}
	}

	private async Task<HttpResponseMessage> SendRequestWithOverrideUrl(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		_logger?.LogDebug(() => "Using specific URL for single call: " + OverrideUrlForRequest.AbsoluteUri);
		request.RequestUri = UriHelper.TransformUri(request.RequestUri, OverrideUrlForRequest.Host);
		_logger?.LogDebug(() => "Sending the request to HTTPApiHandler with AbsoluteURI " + request?.RequestUri?.AbsoluteUri + ".");
		HttpResponseMessage result = await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		_logger?.LogDebug(() => "Received the request from HTTPApiHandler with AbsoluteURI " + request?.RequestUri?.AbsoluteUri + ".");
		return result;
	}

	private async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (request.RequestUri.Host != _currentUrl.Host)
		{
			request.RequestUri = UriHelper.ReplaceHost(request.RequestUri, _currentUrl.Host);
		}
		_logger?.LogDebug(() => "Sending the request to HTTPApiHandler with AbsoluteURI " + request?.RequestUri?.AbsoluteUri + ".");
		HttpResponseMessage result = await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		_logger?.LogDebug(() => "Received the request from HTTPApiHandler with AbsoluteURI " + request?.RequestUri?.AbsoluteUri + ".");
		return result;
	}

	private async Task RaceServers()
	{
		_logger?.LogDebug(() => "Racing servers started.");
		string workingUrl = null;
		short blockedServersCount = 0;
		foreach (string baseUrl in _baseUrls)
		{
			var (text, flag) = await CheckServer(baseUrl);
			if (!string.IsNullOrEmpty(text))
			{
				workingUrl = text;
				break;
			}
			if (flag)
			{
				blockedServersCount++;
			}
		}
		_serverRaced = true;
		if (workingUrl != null)
		{
			_currentUrl = ((workingUrl == _mainUrl.AbsoluteUri) ? _mainUrl : new Uri(workingUrl));
			_logger?.LogInformation("API is set to " + Hasher.SHA256HashString(_currentUrl.Host, trimHash: true));
			_logger?.LogDebug(() => "The current URL is " + _currentUrl.Host);
		}
		else if (blockedServersCount > 0)
		{
			throw new VpnApiException(ApiError.AllApiEndpointsUnreachable, "All API endpoints are unreachable. Possible network restrictions or regional blocking detected.");
		}
		_logger?.LogDebug(() => "Racing servers complete.");
	}

	private async Task<(string, bool)> CheckServer(string baseUrl)
	{
		if (string.IsNullOrEmpty(baseUrl))
		{
			_logger?.LogWarning("BaseUrl is empty");
			return (string.Empty, false);
		}
		Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri requestUri);
		using HttpClient http = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(10.0)
		};
		foreach (KeyValuePair<string, string> header in _headers)
		{
			http.DefaultRequestHeaders.Add(header.Key, header.Value);
		}
		Stopwatch responseStopwatch = Stopwatch.StartNew();
		SocketException innerException = default(SocketException);
		try
		{
			HttpResponseMessage httpResponseMessage = await http.GetAsync(baseUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(continueOnCapturedContext: false);
			responseStopwatch.Stop();
			if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
			{
				_logger?.LogWarning($"Request to {requestUri} failed with status code {httpResponseMessage.StatusCode}.");
				_proxyErrorDelegate?.Invoke(new ProxyError
				{
					RequestUri = requestUri,
					HttpResponse = httpResponseMessage,
					ResponseTime = responseStopwatch.Elapsed
				});
				return (string.Empty, false);
			}
			return (baseUrl, false);
		}
		catch (HttpRequestException ex) when (((Func<bool>)delegate
		{
			// Could not convert BlockContainer to single expression
			innerException = ex.GetInnerException<SocketException>();
			return innerException != null && ((innerException != null && innerException.SocketErrorCode == SocketError.ConnectionReset) || (innerException != null && innerException.SocketErrorCode == SocketError.ConnectionAborted) || (innerException != null && innerException.SocketErrorCode == SocketError.TimedOut) || (innerException != null && innerException.SocketErrorCode == SocketError.ConnectionRefused));
		}).Invoke())
		{
			_logger?.LogWarning(string.Format("API call to {0} failed due to a network-related error. Socket Error: {1} ({2}). Error details: {3}", new object[4]
			{
				baseUrl,
				(int)innerException.SocketErrorCode,
				innerException.SocketErrorCode,
				ex.GetFormattedExceptionMessages()
			}));
			responseStopwatch.Stop();
			HandleException(ex, requestUri, responseStopwatch.Elapsed);
			return (string.Empty, true);
		}
		catch (Exception ex2)
		{
			_logger?.LogWarning("Api call to " + baseUrl + " failed with an error " + ex2.ToString() + ".");
			responseStopwatch.Stop();
			HandleException(ex2, requestUri, responseStopwatch.Elapsed);
			return (string.Empty, false);
		}
	}

	private void HandleException(Exception ex, Uri requestUri, TimeSpan responseTime)
	{
		_proxyErrorDelegate?.Invoke(new ProxyError
		{
			RequestUri = requestUri,
			SystemException = ex,
			ResponseTime = responseTime
		});
	}
}
