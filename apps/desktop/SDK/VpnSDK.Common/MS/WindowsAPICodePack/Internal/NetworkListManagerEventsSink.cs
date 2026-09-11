using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Net;

namespace MS.WindowsAPICodePack.Internal;

internal sealed class NetworkListManagerEventsSink : SinkBase, INetworkListManagerEvents
{
	public void ConnectivityChanged([In] ConnectivityStates newConnectivity)
	{
		(base.ForwardingDelegate as ConnectivityChangedEvent)?.Invoke(newConnectivity);
	}
}
