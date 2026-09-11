using System;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Net;

namespace MS.WindowsAPICodePack.Internal;

internal sealed class NetworkEventsSink : SinkBase, INetworkEvents
{
	public void NetworkAdded([In] Guid networkId)
	{
		(base.ForwardingDelegate as NetworkAddedEvent)?.Invoke(networkId);
	}

	public void NetworkConnectivityChanged([In] Guid networkId, [In] ConnectivityStates newConnectivity)
	{
		(base.ForwardingDelegate as NetworkConnectivityChangedEvent)?.Invoke(networkId, newConnectivity);
	}

	public void NetworkDeleted([In] Guid networkId)
	{
		(base.ForwardingDelegate as NetworkDeletedEvent)?.Invoke(networkId);
	}

	public void NetworkPropertyChanged(Guid networkId, NetworkPropertyChange flags)
	{
		(base.ForwardingDelegate as NetworkPropertyChangedEvent)?.Invoke(networkId, flags);
	}
}
