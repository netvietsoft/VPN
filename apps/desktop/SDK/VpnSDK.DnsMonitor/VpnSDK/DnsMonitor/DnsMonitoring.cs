using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nager.PublicSuffix;
using VpnSDK.Core;
using VpnSDK.Core.Interfaces;
using VpnSDK.DnsMonitor.DTO;
using VpnSDK.DnsMonitor.Interfaces;
using VpnSDK.NetFilter.Helpers;
using VpnSDK.NetFilter.Interop;

namespace VpnSDK.DnsMonitor;

internal class DnsMonitoring : NetFilterEventHandlerBase, IDnsMonitoring, IFeature
{
	private readonly AutoResetEvent _newDnsRequestEvent = new AutoResetEvent(initialState: false);

	private readonly ILogger _logger;

	private CancellationTokenSource _cancellationToken;

	private DnsMonitoringConfig _configuration;

	private SynchronizationContext _syncContext;

	private const int _maxDnsRequestCache = 2000;

	private ConcurrentQueue<DnsRequest> _dnsRequestQueue = new ConcurrentQueue<DnsRequest>();

	private ConcurrentBag<string> _domainException = new ConcurrentBag<string>();

	private object _stateLock = new object();

	private bool _isRunning;

	public string Name => "DnsMonitor";

	public IConfig Config
	{
		get
		{
			return _configuration;
		}
		set
		{
			if (value is DnsMonitoringConfig configuration)
			{
				_configuration = configuration;
				if (_configuration.IsEnabled)
				{
					Start();
				}
				else
				{
					Stop();
				}
			}
		}
	}

	public SynchronizationContext SynchronizationContext
	{
		get
		{
			return _syncContext;
		}
		set
		{
			_syncContext = value ?? SynchronizationContext.Current ?? new SynchronizationContext();
		}
	}

	public event Action<DnsMonitoringArgs> DnsMonitoringUpdate;

	public event Action<DnsMonitoringConfig> DnsMonitoringStarted;

	public event Action<DnsMonitoringConfig> DnsMonitoringStopped;

	public DnsMonitoring(ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<DnsMonitoring>();
	}

	public OperationResult Start()
	{
		lock (_stateLock)
		{
			if (_isRunning)
			{
				return OperationResult.Success;
			}
			DnsMonitoringStarted?.Invoke(_configuration);
			_dnsRequestQueue = new ConcurrentQueue<DnsRequest>();
			_cancellationToken = new CancellationTokenSource();
			Task.Factory.StartNew(DnsMonitorThreadHandler, _cancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			_isRunning = true;
			_logger?.LogInformation("DNS monitoring started.");
			return OperationResult.Success;
		}
	}

	public OperationResult Stop()
	{
		lock (_stateLock)
		{
			if (_isRunning)
			{
				DnsMonitoringStopped?.Invoke(_configuration);
				_isRunning = false;
				_cancellationToken.Cancel();
				_logger?.LogInformation("DNS monitoring stopped.");
			}
			return OperationResult.Success;
		}
	}

	public void AddException(string[] domains)
	{
		try
		{
			if (domains == null)
			{
				throw new ArgumentNullException("domains");
			}
			DomainParser domainParser = new DomainParser(new WebTldRuleProvider());
			foreach (string text in domains)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					continue;
				}
				try
				{
					string host = new Uri(text).Host;
					DomainInfo domainInfo = domainParser.Parse(host);
					if (!string.IsNullOrWhiteSpace(domainInfo.RegistrableDomain))
					{
						_logger?.LogDebug("Adding domain exception from: " + text + " to: " + domainInfo.RegistrableDomain);
						_domainException.Add(domainInfo.RegistrableDomain);
					}
				}
				catch (Exception exception)
				{
					_logger?.LogError(exception, "Failed to include url from exception: " + text);
				}
			}
		}
		catch (Exception exception2)
		{
			_logger?.LogError(exception2, "An unexpected error occurred while processing domain exceptions.");
		}
	}

	public void DnsMonitorThreadHandler()
	{
		_logger?.LogInformation("Start Dns Monitor Thread");
		WaitHandle[] waitHandles = new WaitHandle[2]
		{
			_newDnsRequestEvent,
			_cancellationToken.Token.WaitHandle
		};
		while (!_cancellationToken.IsCancellationRequested)
		{
			if (WaitHandle.WaitAny(waitHandles) == 1)
			{
				continue;
			}
			try
			{
				List<DnsRequest> list = new List<DnsRequest>();
				DnsRequest result;
				while (_dnsRequestQueue.TryDequeue(out result))
				{
					list.Add(result);
				}
				List<string> list2 = (from dns in list
					select dns.DecodeUri() into uri
					where uri != null && !_domainException.Any((string domain) => uri.IndexOf(domain, 0, StringComparison.InvariantCultureIgnoreCase) != -1)
					select uri).ToList();
				if (list2.Count > 0)
				{
					RaiseDnsMonitorEvent(new DnsMonitoringArgs
					{
						DomainNames = list2
					});
				}
			}
			catch (Exception arg)
			{
				_logger?.LogError($"Encountered error on DnsMonitoring: {arg}");
			}
		}
		_logger?.LogInformation("Exiting Dns Monitor Thread");
	}

	private void RaiseDnsMonitorEvent(DnsMonitoringArgs args)
	{
		if (SynchronizationContext == null)
		{
			SynchronizationContext = new SynchronizationContext();
		}
		SynchronizationContext.Post(delegate
		{
			DnsMonitoringUpdate?.Invoke(args);
		}, null);
	}

	private void AddDnsRequest(IntPtr buf, int len)
	{
		if (_dnsRequestQueue.Count < 2000)
		{
			_dnsRequestQueue?.Enqueue(new DnsRequest(buf, len));
			_newDnsRequestEvent.Set();
		}
	}

	public override void tcpSend(ulong id, IntPtr buf, int len)
	{
		try
		{
			AddDnsRequest(buf, len);
		}
		catch (Exception)
		{
		}
	}

	public override void udpSend(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options, int optionsLen)
	{
		try
		{
			byte[] array = new byte[28];
			Marshal.Copy(remoteAddress, array, 0, 28);
			SocketAddress socketAddress = NetFilterInterop.NFUtil.convertAddress(array);
			IPEndPoint iPEndPoint = ((socketAddress.Family != AddressFamily.InterNetworkV6) ? new IPEndPoint(0L, 0) : new IPEndPoint(IPAddress.IPv6None, 0));
			iPEndPoint = (IPEndPoint)iPEndPoint.Create(socketAddress);
			if (iPEndPoint.Port == 53)
			{
				AddDnsRequest(buf, len);
			}
		}
		catch (Exception)
		{
		}
	}
}
