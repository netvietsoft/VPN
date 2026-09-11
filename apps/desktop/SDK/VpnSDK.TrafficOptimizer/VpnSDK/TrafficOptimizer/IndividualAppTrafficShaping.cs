using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using VpnSDK.NetFilter.Interop;
using VpnSDK.TrafficOptimizer.DTO;

namespace VpnSDK.TrafficOptimizer;

internal class IndividualAppTrafficShaping : TrafficShaping
{
	private readonly Dictionary<string, FlowControlData> _flowControlData = new Dictionary<string, FlowControlData>();

	public override TrafficMode TrafficMode => TrafficMode.PerApplicationLimiting;

	protected override double OtherAppsBandwidthPercentage
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public override bool AddApplication(string applicationName, ulong downloadLimit = 0uL, ulong uploadLimit = 0uL)
	{
		if (_flowControlData.ContainsKey(applicationName))
		{
			_flowControlData[applicationName].SetSpeed(uploadLimit, downloadLimit);
			return true;
		}
		_flowControlData.Add(applicationName, new FlowControlData());
		if (base.IsEnabled)
		{
			_flowControlData[applicationName].Register(downloadLimit, uploadLimit);
		}
		return false;
	}

	public override void ClearApplication()
	{
		foreach (KeyValuePair<string, FlowControlData> flowControlDatum in _flowControlData)
		{
			flowControlDatum.Value.Unregister();
		}
		_flowControlData.Clear();
	}

	public override void OnTcpConnected(ulong id, string processName, NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
		if (_flowControlData.ContainsKey(processName))
		{
			FlowControlData flowControlData = _flowControlData[processName];
			NetFilterInterop.NF_STATUS nF_STATUS = NetFilterInterop.NFAPI.nf_setTCPFlowCtl(id, flowControlData.Handle);
			if (nF_STATUS != NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS)
			{
				base.Logger.LogError($"error on setting TCP Flow Control id={id} name={processName} error: {nF_STATUS}");
			}
		}
		_idsTcp.Add(id);
	}

	public override void OnUdpConnected(ulong id, string processName, NetFilterInterop.NF_UDP_CONN_INFO connInfo)
	{
		if (_flowControlData.ContainsKey(processName))
		{
			FlowControlData flowControlData = _flowControlData[processName];
			NetFilterInterop.NF_STATUS nF_STATUS = NetFilterInterop.NFAPI.nf_setUDPFlowCtl(id, flowControlData.Handle);
			if (nF_STATUS != NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS)
			{
				base.Logger.LogError($"error on setting UDP Flow Control id={id} name={processName} error: {nF_STATUS}");
			}
		}
		_idsUdp.Add(id);
	}

	public override void RemoveApplication(string applicationName)
	{
		if (_flowControlData.ContainsKey(applicationName))
		{
			FlowControlData flowControlData = _flowControlData[applicationName];
			if (flowControlData.Handle != 0)
			{
				flowControlData.Unregister();
			}
			_flowControlData.Remove(applicationName);
		}
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
			flag = (base.IsEnabled = _flowControlData.Values.All((FlowControlData flow) => flow.Register(0uL, 0uL)));
		}
		if (flag)
		{
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
		foreach (string key in _flowControlData.Keys)
		{
			RemoveApplication(key);
		}
		_stopSignal.Set();
		base.IsEnabled = false;
	}

	public void TrafficShapingThreadHandler()
	{
		base.Logger.LogInformation("Start Traffic Shaping Thread");
		uint num = 0u;
		while (!_stopSignal.WaitOne(1000))
		{
			uint tickCount = (uint)Environment.TickCount;
			if (tickCount - num < 1000)
			{
				continue;
			}
			num = tickCount;
			foreach (KeyValuePair<string, FlowControlData> flowControlDatum in _flowControlData)
			{
				flowControlDatum.Value?.RefreshStatistics();
				ILogger logger = base.Logger;
				string[] obj = new string[14]
				{
					"TS_STAT ",
					flowControlDatum.Key,
					":",
					(flowControlDatum.Value?.TrafficStatistics.Download.CurrentKBps()).ToString(),
					",",
					(flowControlDatum.Value?.TrafficStatistics.Download.MaxAverageKBps()).ToString(),
					",",
					null,
					null,
					null,
					null,
					null,
					null,
					null
				};
				FlowControlData value = flowControlDatum.Value;
				obj[7] = ((uint)((value != null) ? new ulong?(value.DownloadLimit / 1024) : ((ulong?)null)).Value).ToString();
				obj[8] = ",";
				obj[9] = (flowControlDatum.Value?.TrafficStatistics.Upload.CurrentKBps()).ToString();
				obj[10] = ",";
				obj[11] = (flowControlDatum.Value?.TrafficStatistics.Upload.MaxAverageKBps()).ToString();
				obj[12] = ",";
				FlowControlData value2 = flowControlDatum.Value;
				obj[13] = ((uint)((value2 != null) ? new ulong?(value2.UploadLimit / 1024) : ((ulong?)null)).Value).ToString();
				logger.LogInformation(string.Concat(obj));
			}
		}
		base.Logger.LogInformation("Exiting Traffic Shaping Thread");
	}
}
