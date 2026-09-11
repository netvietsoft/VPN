using System;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Net;

namespace MS.WindowsAPICodePack.Internal;

internal sealed class NetworkConnectionEventsSink : SinkBase, INetworkConnectionEvents
{
	public void NetworkConnectionConnectivityChanged([In] Guid connectionId, [In] ConnectivityStates newConnectivity)
	{
		(base.ForwardingDelegate as NetworkConnectionConnectivityChangedEvent)?.Invoke(connectionId, newConnectivity);
	}

	public void NetworkConnectionPropertyChanged([In] Guid connectionId, [In] NetworkConnectionPropertyChange flags)
	{
		(base.ForwardingDelegate as NetworkConnectionPropertyChangedEvent)?.Invoke(connectionId, flags);
	}
}
