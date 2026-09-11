using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using VpnSDK.NetFilter.Interop;

namespace VpnSDK.TrafficOptimizer;

internal abstract class TrafficShaping : IDisposable
{
	protected readonly List<ulong> _idsTcp = new List<ulong>();

	protected readonly List<ulong> _idsUdp = new List<ulong>();

	protected AutoResetEvent _stopSignal = new AutoResetEvent(initialState: false);

	protected object _startLock = new object();

	public abstract TrafficMode TrafficMode { get; }

	public bool IsEnabled { get; protected set; }

	protected virtual double OtherAppsBandwidthPercentage { get; set; }

	public ILogger Logger { get; set; }

	public virtual bool AddApplication(string applicationName, ulong downloadLimit = 0uL, ulong uploadLimit = 0uL)
	{
		return false;
	}

	public virtual bool ExcludeApplication(string applicationName)
	{
		return false;
	}

	public virtual void RemoveApplication(string applicationName)
	{
		Logger?.LogDebug("Base::RemoveApplication called...");
	}

	public virtual void ClearApplication()
	{
		Logger?.LogDebug("Base::ClearApplication called...");
	}

	public virtual void Start()
	{
		Logger?.LogDebug("Base::Start called...");
	}

	public virtual void Stop()
	{
		Logger?.LogDebug("Base::Stop called...");
	}

	public virtual void OnTcpConnected(ulong id, string processName, NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
		Logger?.LogDebug("Base::OnTcpConnected called...");
	}

	public virtual void OnUdpConnected(ulong id, string processName, NetFilterInterop.NF_UDP_CONN_INFO connInfo)
	{
		Logger?.LogDebug("Base::OnUdpConnected called...");
	}

	public virtual void OnTcpDisconnect(ulong id, NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
		NetFilterInterop.NFAPI.nf_setTCPFlowCtl(id, 0u);
		_idsTcp.Remove(id);
	}

	public virtual void OnUdpDisconnect(ulong id, NetFilterInterop.NF_UDP_CONN_INFO connInfo)
	{
		NetFilterInterop.NFAPI.nf_setUDPFlowCtl(id, 0u);
		_idsUdp.Remove(id);
	}

	public void Dispose()
	{
		Stop();
	}
}
