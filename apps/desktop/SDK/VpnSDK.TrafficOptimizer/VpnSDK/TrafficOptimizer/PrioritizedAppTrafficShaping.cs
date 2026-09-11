using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Logging;
using VpnSDK.NetFilter.Interop;
using VpnSDK.TrafficOptimizer.DTO;
using VpnSDK.TrafficOptimizer.Enum;
using VpnSDK.TrafficOptimizer.Interfaces;

namespace VpnSDK.TrafficOptimizer;

internal class PrioritizedAppTrafficShaping : TrafficShaping
{
	private readonly TrafficStatistics _networkTrafficStatistics = new TrafficStatistics();

	private readonly FlowControlData _otherAppsFlow = new FlowControlData();

	private readonly FlowControlData _prioritizedAppFlow = new FlowControlData();

	private readonly List<string> _excludeList = new List<string>();

	private readonly List<string> _processList = new List<string>();

	private readonly NetworkInterface _networkInterface;

	private readonly TrafficDataMode _dataMode;

	private ulong _bytesReceived;

	private ulong _bytesSent;

	private int _lastTick;

	private readonly ITrafficOptimizer _context;

	protected override double OtherAppsBandwidthPercentage { get; set; } = 0.2;

	public override TrafficMode TrafficMode => TrafficMode.ApplicationPriority;

	public PrioritizedAppTrafficShaping(ITrafficOptimizer context, NetworkInterface networkInterface)
	{
		_context = context;
		_networkInterface = networkInterface;
	}

	private bool IsApplicationPrioritized(string processName)
	{
		return _processList.Contains(processName);
	}

	private bool IsApplicationExcluded(string processName)
	{
		return _excludeList.Contains(processName);
	}

	public override bool AddApplication(string applicationName, ulong downloadLimit = 0uL, ulong uploadLimit = 0uL)
	{
		if (!_processList.Contains(applicationName))
		{
			_processList.Add(applicationName);
			return true;
		}
		return false;
	}

	public override bool ExcludeApplication(string applicationName)
	{
		if (!_excludeList.Contains(applicationName))
		{
			_excludeList.Add(applicationName);
			return true;
		}
		return false;
	}

	public override void Start()
	{
		bool flag = false;
		lock (_startLock)
		{
			if (base.IsEnabled)
			{
				return;
			}
			flag = (base.IsEnabled = _otherAppsFlow.Register(0uL, 0uL) & _prioritizedAppFlow.Register(0uL, 0uL));
		}
		base.Logger.LogInformation($"IsEnabled = {base.IsEnabled}; OtherApps: {_otherAppsFlow.Handle}, Prioritized: {_prioritizedAppFlow.Handle}");
		if (flag)
		{
			if (_context?.Config is TrafficOptimizerConfig trafficOptimizerConfig)
			{
				OtherAppsBandwidthPercentage = (double)(100 - trafficOptimizerConfig.PrioritizedAppsMaxBandwidth) / 100.0;
			}
			Thread thread = new Thread(TrafficShapingThreadHandler);
			thread.IsBackground = true;
			thread.Start();
		}
	}

	public override void Stop()
	{
		if (!base.IsEnabled)
		{
			return;
		}
		foreach (ulong item in _idsUdp)
		{
			NetFilterInterop.NFAPI.nf_setUDPFlowCtl(item, 0u);
		}
		_idsUdp.Clear();
		foreach (ulong item2 in _idsTcp)
		{
			NetFilterInterop.NFAPI.nf_setTCPFlowCtl(item2, 0u);
		}
		_idsTcp.Clear();
		_stopSignal.Set();
		_otherAppsFlow.Unregister();
		_prioritizedAppFlow.Unregister();
		base.IsEnabled = false;
	}

	public void TrafficShapingThreadHandler()
	{
		base.Logger.LogInformation("Start Traffic Shaping Thread");
		uint num = 0u;
		uint num2 = 0u;
		bool flag = false;
		while (!_stopSignal.WaitOne(1000))
		{
			uint tickCount = (uint)Environment.TickCount;
			if (tickCount - num < 1000)
			{
				continue;
			}
			try
			{
				num = tickCount;
				NetFilterInterop.NF_FLOWCTL_DATA? nF_FLOWCTL_DATA = ((_dataMode == TrafficDataMode.NetworkInterface) ? MonitorMaxBandwidthUsingNetworkInterface() : MonitorMaxBandwidthUsingFlowData());
				SendTrafficUpdate();
				if (_prioritizedAppFlow.TrafficStatistics.Upload.Current > 131072 || _prioritizedAppFlow.TrafficStatistics.Download.Current > 131072)
				{
					num2 = tickCount;
				}
				if (nF_FLOWCTL_DATA.HasValue && (nF_FLOWCTL_DATA.Value.inLimit > _otherAppsFlow.DownloadLimit || nF_FLOWCTL_DATA.Value.outLimit > _otherAppsFlow.UploadLimit))
				{
					_otherAppsFlow.SetSpeed(nF_FLOWCTL_DATA.Value.outLimit, nF_FLOWCTL_DATA.Value.inLimit);
					base.Logger.LogInformation($"Shaping active: in {nF_FLOWCTL_DATA.Value.inLimit}, out {nF_FLOWCTL_DATA.Value.outLimit}");
					flag = true;
				}
				else if (flag && tickCount - num2 > 5000)
				{
					_otherAppsFlow.SetSpeed(0uL, 0uL);
					base.Logger.LogInformation("Shaping off");
					flag = false;
				}
			}
			catch (Exception exception)
			{
				base.Logger.LogError(exception, "Error in Traffic Shaping Thread handler");
			}
		}
		base.Logger.LogInformation("Exiting Traffic Shaping Thread");
	}

	public override void OnTcpConnected(ulong id, string processName, NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
		if (!IsApplicationExcluded(processName))
		{
			bool flag = IsApplicationPrioritized(processName);
			HandleConnection("TCP", id, processName, flag, flag ? _prioritizedAppFlow : _otherAppsFlow, (ulong connId, FlowControlData flow) => NetFilterInterop.NFAPI.nf_setTCPFlowCtl(connId, flow.Handle));
			_idsTcp.Add(id);
		}
	}

	public override void OnUdpConnected(ulong id, string processName, NetFilterInterop.NF_UDP_CONN_INFO connInfo)
	{
		if (!IsApplicationExcluded(processName))
		{
			bool flag = IsApplicationPrioritized(processName);
			HandleConnection("UDP", id, processName, flag, flag ? _prioritizedAppFlow : _otherAppsFlow, (ulong connId, FlowControlData flow) => NetFilterInterop.NFAPI.nf_setUDPFlowCtl(connId, flow.Handle));
			_idsUdp.Add(id);
		}
	}

	public override void RemoveApplication(string applicationName)
	{
		if (_processList.Contains(applicationName))
		{
			_processList.Remove(applicationName);
		}
	}

	public override void ClearApplication()
	{
		_processList.Clear();
	}

	private void HandleConnection(string connectionType, ulong id, string processName, bool isPrioritized, FlowControlData flowData, Func<ulong, FlowControlData, NetFilterInterop.NF_STATUS> setFlowControl)
	{
		if (isPrioritized)
		{
			base.Logger.LogInformation($"Prio {connectionType} Flow Control id={id} name={processName}");
		}
		else
		{
			base.Logger.LogInformation($"{connectionType} Flow Control id={id} name={processName}");
		}
		NetFilterInterop.NF_STATUS nF_STATUS = setFlowControl(id, flowData);
		if (nF_STATUS != NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS)
		{
			base.Logger.LogError(string.Format("error on setting {0} Flow Control id={1} name={2} error: {3}", new object[4] { connectionType, id, processName, nF_STATUS }));
		}
	}

	private NetFilterInterop.NF_FLOWCTL_DATA MonitorMaxBandwidthUsingFlowData()
	{
		_otherAppsFlow.RefreshStatistics();
		_prioritizedAppFlow.RefreshStatistics();
		base.Logger.LogDebug($"TS_STAT:{_otherAppsFlow},{_prioritizedAppFlow}");
		NetFilterInterop.NF_FLOWCTL_DATA result = new NetFilterInterop.NF_FLOWCTL_DATA
		{
			inLimit = _otherAppsFlow.DownloadLimit,
			outLimit = _otherAppsFlow.UploadLimit
		};
		if (_prioritizedAppFlow.TrafficStatistics.Upload.Current > 131072)
		{
			ulong speed = ((_prioritizedAppFlow.TrafficStatistics.Upload.MaxAverage > _otherAppsFlow.TrafficStatistics.Upload.MaxAverage) ? _prioritizedAppFlow.TrafficStatistics.Upload.MaxAverage : _otherAppsFlow.TrafficStatistics.Upload.MaxAverage);
			result.outLimit = CalculateLimit(speed);
			if (result.outLimit < 131072)
			{
				result.outLimit = 131072uL;
			}
		}
		if (_prioritizedAppFlow.TrafficStatistics.Download.Current > 131072)
		{
			ulong speed2 = ((_prioritizedAppFlow.TrafficStatistics.Download.MaxAverage > _otherAppsFlow.TrafficStatistics.Download.MaxAverage) ? _prioritizedAppFlow.TrafficStatistics.Download.MaxAverage : _otherAppsFlow.TrafficStatistics.Download.MaxAverage);
			result.inLimit = CalculateLimit(speed2);
			if (result.inLimit < 131072)
			{
				result.inLimit = 131072uL;
			}
		}
		return result;
	}

	private NetFilterInterop.NF_FLOWCTL_DATA MonitorMaxBandwidthUsingNetworkInterface()
	{
		NetFilterInterop.NF_FLOWCTL_DATA result = new NetFilterInterop.NF_FLOWCTL_DATA
		{
			inLimit = _otherAppsFlow.DownloadLimit,
			outLimit = _otherAppsFlow.UploadLimit
		};
		_prioritizedAppFlow.RefreshStatistics();
		if (_networkInterface != null)
		{
			int tickCount = Environment.TickCount;
			if (tickCount - _lastTick < 2000)
			{
				ulong num = (ulong)((tickCount - _lastTick) / 1000);
				if (num != 0)
				{
					ulong bytesReceived = (ulong)_networkInterface.GetIPStatistics().BytesReceived;
					ulong bytesSent = (ulong)_networkInterface.GetIPStatistics().BytesSent;
					ulong inSpeed = (bytesReceived - _bytesReceived) / num;
					ulong outSpeed = (bytesSent - _bytesSent) / num;
					_networkTrafficStatistics.Store(outSpeed, inSpeed);
					_bytesReceived = bytesReceived;
					_bytesSent = bytesSent;
				}
			}
			else
			{
				_bytesReceived = (ulong)_networkInterface.GetIPStatistics().BytesReceived;
				_bytesSent = (ulong)_networkInterface.GetIPStatistics().BytesSent;
			}
			_lastTick = tickCount;
		}
		base.Logger.LogDebug("TS_STAT:" + _networkTrafficStatistics.Download.CurrentKBps() + "," + _networkTrafficStatistics.Download.MaxAverageKBps() + "," + (uint)(_otherAppsFlow.DownloadLimit / 1024) + "," + _networkTrafficStatistics.Upload.CurrentKBps() + "," + _networkTrafficStatistics.Upload.MaxAverageKBps() + "," + (uint)(_otherAppsFlow.UploadLimit / 1024) + "," + _prioritizedAppFlow.ToString());
		if (_prioritizedAppFlow.TrafficStatistics.Upload.Current > 131072)
		{
			result.outLimit = CalculateLimit(_networkTrafficStatistics.Upload.MaxAverage);
			if (result.outLimit < 131072)
			{
				result.outLimit = 131072uL;
			}
		}
		if (_prioritizedAppFlow.TrafficStatistics.Download.Current > 131072)
		{
			result.inLimit = CalculateLimit(_networkTrafficStatistics.Download.MaxAverage);
			if (result.inLimit < 131072)
			{
				result.inLimit = 131072uL;
			}
		}
		return result;
	}

	private ulong CalculateLimit(ulong speed)
	{
		return (ulong)((double)speed * OtherAppsBandwidthPercentage);
	}

	private void SendTrafficUpdate()
	{
		_context.RaiseTrafficUpdateEvent(new TrafficOptimizerArgs
		{
			Download = new TrafficSpeedData
			{
				Current = _otherAppsFlow.TrafficStatistics.Download.Current,
				Maximum = _otherAppsFlow.TrafficStatistics.Download.Maximum,
				MaxAverage = _otherAppsFlow.TrafficStatistics.Download.MaxAverage,
				Limit = _otherAppsFlow.DownloadLimit
			},
			Upload = new TrafficSpeedData
			{
				Current = _otherAppsFlow.TrafficStatistics.Upload.Current,
				Maximum = _otherAppsFlow.TrafficStatistics.Upload.Maximum,
				MaxAverage = _otherAppsFlow.TrafficStatistics.Upload.MaxAverage,
				Limit = _otherAppsFlow.UploadLimit
			},
			PrioritizedAppDownload = new TrafficSpeedData
			{
				Current = _prioritizedAppFlow.TrafficStatistics.Download.Current,
				Maximum = _prioritizedAppFlow.TrafficStatistics.Download.Maximum,
				MaxAverage = _prioritizedAppFlow.TrafficStatistics.Download.MaxAverage,
				Limit = _prioritizedAppFlow.DownloadLimit
			},
			PrioritizedAppUpload = new TrafficSpeedData
			{
				Current = _prioritizedAppFlow.TrafficStatistics.Upload.Current,
				Maximum = _prioritizedAppFlow.TrafficStatistics.Upload.Maximum,
				MaxAverage = _prioritizedAppFlow.TrafficStatistics.Upload.MaxAverage,
				Limit = _prioritizedAppFlow.UploadLimit
			}
		});
	}
}
