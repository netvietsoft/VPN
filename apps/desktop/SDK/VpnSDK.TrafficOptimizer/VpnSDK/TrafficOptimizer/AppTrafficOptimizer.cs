using System;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Logging;
using VpnSDK.Core;
using VpnSDK.Core.Interfaces;
using VpnSDK.NetFilter.Helpers;
using VpnSDK.NetFilter.Interop;
using VpnSDK.TrafficOptimizer.DTO;
using VpnSDK.TrafficOptimizer.Interfaces;

namespace VpnSDK.TrafficOptimizer;

internal class AppTrafficOptimizer : NetFilterEventHandlerBase, ITrafficOptimizer, IFeature
{
	private readonly ILogger _logger;

	private TrafficOptimizerConfig _configuration;

	private NetworkInterface networkInterface;

	private TrafficShaping _trafficShaping;

	private SynchronizationContext _syncContext;

	private readonly string[] _excludedApps = new string[2] { "C:\\Windows\\System32\\svchost.exe", "System" };

	public string Name => "Traffic Optimizer";

	public IConfig Config
	{
		get
		{
			return _configuration;
		}
		set
		{
			if (value is TrafficOptimizerConfig trafficOptimizerConfig && ValidateConfiguration(trafficOptimizerConfig))
			{
				_configuration = trafficOptimizerConfig;
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

	public event Action<TrafficOptimizerArgs> TrafficUpdate;

	public AppTrafficOptimizer(ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<AppTrafficOptimizer>();
	}

	private bool ValidateConfiguration(TrafficOptimizerConfig config)
	{
		string[] prioritizedApps = config.PrioritizedApps;
		if (prioritizedApps != null && prioritizedApps.Length > 5)
		{
			throw new Exception($"Max of {5} applications can be prioritized.");
		}
		if (config.PrioritizedAppsMaxBandwidth < 0 || config.PrioritizedAppsMaxBandwidth > 100)
		{
			throw new ArgumentOutOfRangeException("PrioritizedAppsMaxBandwidth", config.PrioritizedAppsMaxBandwidth, $"Value must be in the range of {0} to {100}.");
		}
		return true;
	}

	private void AddProcessToTrafficControl(string appPath)
	{
		if (_trafficShaping != null)
		{
			_logger?.LogInformation("Add application " + appPath + " to traffic shaping.");
			_trafficShaping.AddApplication(appPath, 0uL, 0uL);
		}
		else
		{
			_logger?.LogError("Set Traffic Mode first.");
		}
	}

	private void ClearTrafficControlProcesses()
	{
		if (_trafficShaping != null)
		{
			_trafficShaping.ClearApplication();
		}
	}

	private void AddNetworkInterface(NetworkInterface _networkInterface)
	{
		networkInterface = _networkInterface;
	}

	private void ExcludePathTrafficControl(string appPath)
	{
		_trafficShaping.ExcludeApplication(appPath);
	}

	private void ResetAndConfigure()
	{
		_trafficShaping?.Stop();
		_trafficShaping = null;
		ClearTrafficControlProcesses();
		TrafficOptimizerConfig configuration = _configuration;
		if (configuration != null && configuration.IsEnabled)
		{
			switch (_configuration.Mode)
			{
			case TrafficMode.ApplicationPriority:
				_trafficShaping = new PrioritizedAppTrafficShaping(this, networkInterface);
				break;
			case TrafficMode.PerApplicationLimiting:
				_trafficShaping = new IndividualAppTrafficShaping();
				break;
			}
			_trafficShaping.Logger = _logger;
			string[] prioritizedApps = _configuration.PrioritizedApps;
			foreach (string appPath in prioritizedApps)
			{
				AddProcessToTrafficControl(appPath);
			}
			prioritizedApps = _excludedApps;
			foreach (string appPath2 in prioritizedApps)
			{
				ExcludePathTrafficControl(appPath2);
			}
		}
	}

	public OperationResult Start()
	{
		OperationResult result = OperationResult.Success;
		ResetAndConfigure();
		if (_trafficShaping != null)
		{
			if (!_trafficShaping.IsEnabled)
			{
				_trafficShaping.Start();
				_logger?.LogInformation("Traffic shaping control started.");
			}
			else
			{
				_logger?.LogInformation("Traffic shaping control already started.");
				result = OperationResult.Failure(1);
			}
		}
		else
		{
			_logger?.LogError("Conditions have not been met to start Traffic Shaping");
			result = OperationResult.Failure(2);
		}
		return result;
	}

	public OperationResult Stop()
	{
		_trafficShaping?.Stop();
		return OperationResult.Success;
	}

	public void RaiseTrafficUpdateEvent(TrafficOptimizerArgs args)
	{
		if (SynchronizationContext == null)
		{
			SynchronizationContext = new SynchronizationContext();
		}
		SynchronizationContext.Post(delegate
		{
			TrafficUpdate?.Invoke(args);
		}, null);
	}

	public override void tcpConnected(ulong id, NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
		if (_trafficShaping != null)
		{
			string processName = NetFilterInterop.NFAPI.nf_getProcessNameFromKernel(connInfo.processId);
			_trafficShaping.OnTcpConnected(id, processName, connInfo);
		}
	}

	public override void tcpClosed(ulong id, NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
		if (_trafficShaping != null && _trafficShaping.IsEnabled)
		{
			_trafficShaping.OnTcpDisconnect(id, connInfo);
		}
	}

	public override void udpCreated(ulong id, NetFilterInterop.NF_UDP_CONN_INFO connInfo)
	{
		if (_trafficShaping != null && _trafficShaping.IsEnabled)
		{
			string processName = NetFilterInterop.NFAPI.nf_getProcessNameFromKernel(connInfo.processId);
			_trafficShaping.OnUdpConnected(id, processName, connInfo);
		}
	}

	public override void udpClosed(ulong id, NetFilterInterop.NF_UDP_CONN_INFO connInfo)
	{
		if (_trafficShaping != null && _trafficShaping.IsEnabled)
		{
			_trafficShaping.OnUdpDisconnect(id, connInfo);
		}
	}
}
