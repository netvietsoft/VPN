using System;
using VpnSDK.NetFilter.Interop;

namespace VpnSDK.NetFilter.Helpers;

internal abstract class NetFilterEventHandlerBase : NetFilterInterop.NF_EventHandler
{
	public virtual void threadStart()
	{
	}

	public virtual void threadEnd()
	{
	}

	public virtual void tcpConnectRequest(ulong id, ref NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
	}

	public virtual void tcpConnected(ulong id, NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
	}

	public virtual void tcpClosed(ulong id, NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
	}

	public virtual void tcpReceive(ulong id, IntPtr buf, int len)
	{
	}

	public virtual void tcpSend(ulong id, IntPtr buf, int len)
	{
	}

	public virtual void tcpCanReceive(ulong id)
	{
	}

	public virtual void tcpCanSend(ulong id)
	{
	}

	public virtual void udpCreated(ulong id, NetFilterInterop.NF_UDP_CONN_INFO connInfo)
	{
	}

	public virtual void udpConnectRequest(ulong id, ref NetFilterInterop.NF_UDP_CONN_REQUEST connReq)
	{
	}

	public virtual void udpClosed(ulong id, NetFilterInterop.NF_UDP_CONN_INFO connInfo)
	{
	}

	public virtual void udpReceive(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options, int optionsLen)
	{
	}

	public virtual void udpSend(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options, int optionsLen)
	{
	}

	public virtual void udpCanReceive(ulong id)
	{
	}

	public virtual void udpCanSend(ulong id)
	{
	}
}
