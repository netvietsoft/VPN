using System;
using VpnSDK.NetFilter.Interop;

namespace VpnSDK.TrafficOptimizer.DTO;

internal class FlowControlData
{
	private NetFilterInterop.NF_FLOWCTL_STAT _statistics;

	private NetFilterInterop.NF_FLOWCTL_DATA _flowData;

	private int _lastTick;

	private uint _handle;

	public uint Handle => _handle;

	public ulong DownloadLimit => _flowData.inLimit;

	public ulong UploadLimit => _flowData.outLimit;

	public TrafficStatistics TrafficStatistics { get; private set; } = new TrafficStatistics();

	public void RefreshStatistics()
	{
		int tickCount = Environment.TickCount;
		NetFilterInterop.NF_FLOWCTL_STAT pStat = default(NetFilterInterop.NF_FLOWCTL_STAT);
		if (_handle == 0 || NetFilterInterop.NFAPI.nf_getFlowCtlStat(_handle, ref pStat) != NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS)
		{
			return;
		}
		if (tickCount - _lastTick < 2000)
		{
			ulong num = (ulong)((tickCount - _lastTick) / 1000);
			if (num != 0)
			{
				ulong inSpeed = (pStat.inBytes - _statistics.inBytes) / num;
				ulong outSpeed = (pStat.outBytes - _statistics.outBytes) / num;
				TrafficStatistics.Store(outSpeed, inSpeed);
			}
		}
		_statistics = pStat;
		_lastTick = tickCount;
	}

	public bool Register(ulong downloadLimit = 0uL, ulong uploadLimit = 0uL)
	{
		if (_handle != 0)
		{
			return true;
		}
		_lastTick = Environment.TickCount;
		_flowData = new NetFilterInterop.NF_FLOWCTL_DATA
		{
			inLimit = downloadLimit,
			outLimit = uploadLimit
		};
		TrafficStatistics.Clear();
		_statistics = default(NetFilterInterop.NF_FLOWCTL_STAT);
		if (NetFilterInterop.NFAPI.nf_addFlowCtl(ref _flowData, ref _handle) != NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS)
		{
			return false;
		}
		return true;
	}

	public void Unregister()
	{
		if (_handle != 0)
		{
			NetFilterInterop.NFAPI.nf_deleteFlowCtl(_handle);
			_handle = 0u;
		}
	}

	public void SetSpeed(ulong outSpeed, ulong inSpeed)
	{
		_flowData.inLimit = inSpeed;
		_flowData.outLimit = outSpeed;
		NetFilterInterop.NFAPI.nf_modifyFlowCtl(_handle, ref _flowData);
	}

	public override string ToString()
	{
		return string.Join(",", new object[6]
		{
			TrafficStatistics.Download.CurrentKBps(),
			TrafficStatistics.Download.MaxAverageKBps(),
			(uint)(DownloadLimit / 1024),
			TrafficStatistics.Upload.CurrentKBps(),
			TrafficStatistics.Upload.MaxAverageKBps(),
			(uint)(UploadLimit / 1024)
		});
	}
}
