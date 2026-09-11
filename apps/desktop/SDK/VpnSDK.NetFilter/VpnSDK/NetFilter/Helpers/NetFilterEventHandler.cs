using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VpnSDK.NetFilter.Interop;

namespace VpnSDK.NetFilter.Helpers;

internal class NetFilterEventHandler : NetFilterEventHandlerBase
{
	private readonly List<NetFilterEventHandlerBase> _handlers = new List<NetFilterEventHandlerBase>();

	private readonly ILogger _logger;

	public NetFilterEventHandler(ILogger logger)
	{
		_logger = logger;
	}

	public void AddHandler(NetFilterEventHandlerBase handler)
	{
		if (!_handlers.Contains(handler))
		{
			_handlers.Add(handler);
		}
	}

	public override void threadStart()
	{
		_logger?.LogInformation("Event handling thread started.");
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.threadStart();
		}
	}

	public override void threadEnd()
	{
		_logger?.LogInformation("Event handling thread stopped.");
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.threadStart();
		}
	}

	public override void tcpConnectRequest(ulong id, ref NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.tcpConnectRequest(id, ref connInfo);
		}
	}

	public override void tcpConnected(ulong id, NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.tcpConnected(id, connInfo);
		}
	}

	public override void tcpClosed(ulong id, NetFilterInterop.NF_TCP_CONN_INFO connInfo)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.tcpClosed(id, connInfo);
		}
	}

	public override void tcpReceive(ulong id, IntPtr buf, int len)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.tcpReceive(id, buf, len);
		}
		NetFilterInterop.NFAPI.nf_tcpPostReceive(id, buf, len);
	}

	public override void tcpSend(ulong id, IntPtr buf, int len)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.tcpSend(id, buf, len);
		}
		NetFilterInterop.NFAPI.nf_tcpPostSend(id, buf, len);
	}

	public override void tcpCanReceive(ulong id)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.tcpCanReceive(id);
		}
	}

	public override void tcpCanSend(ulong id)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.tcpCanSend(id);
		}
	}

	public override void udpCreated(ulong id, NetFilterInterop.NF_UDP_CONN_INFO connInfo)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.udpCreated(id, connInfo);
		}
	}

	public override void udpConnectRequest(ulong id, ref NetFilterInterop.NF_UDP_CONN_REQUEST connReq)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.udpConnectRequest(id, ref connReq);
		}
	}

	public override void udpClosed(ulong id, NetFilterInterop.NF_UDP_CONN_INFO connInfo)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.udpClosed(id, connInfo);
		}
	}

	public override void udpReceive(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options, int optionsLen)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.udpReceive(id, remoteAddress, buf, len, options, optionsLen);
		}
		NetFilterInterop.NFAPI.nf_udpPostReceive(id, remoteAddress, buf, len, options);
	}

	public override void udpSend(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options, int optionsLen)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.udpSend(id, remoteAddress, buf, len, options, optionsLen);
		}
		NetFilterInterop.NFAPI.nf_udpPostSend(id, remoteAddress, buf, len, options);
	}

	public override void udpCanReceive(ulong id)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.udpCanReceive(id);
		}
	}

	public override void udpCanSend(ulong id)
	{
		foreach (NetFilterEventHandlerBase handler in _handlers)
		{
			handler.udpCanSend(id);
		}
	}
}
