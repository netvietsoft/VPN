using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MS.WindowsAPICodePack.Internal;

namespace Microsoft.WindowsAPICodePack.Net;

internal static class NetworkListManager
{
	private static NetworkListManagerClass manager;

	public static bool IsConnectedToInternet => manager.IsConnectedToInternet;

	public static bool IsConnected => manager.IsConnected;

	public static ConnectivityStates Connectivity => manager.GetConnectivity();

	public static event ConnectivityChangedEvent ConnectivityChanged
	{
		add
		{
			SinkBase.AddSink<NetworkListManagerEventsSink>((IConnectionPointContainer)manager, typeof(INetworkListManagerEvents), value);
		}
		remove
		{
			SinkBase.RemoveSink<NetworkListManagerEventsSink>(value);
		}
	}

	public static event NetworkAddedEvent NetworkAdded
	{
		add
		{
			SinkBase.AddSink<NetworkEventsSink>((IConnectionPointContainer)manager, typeof(INetworkEvents), value);
		}
		remove
		{
			SinkBase.RemoveSink<NetworkEventsSink>(value);
		}
	}

	public static event NetworkConnectivityChangedEvent NetworkConnectivityChanged
	{
		add
		{
			SinkBase.AddSink<NetworkEventsSink>((IConnectionPointContainer)manager, typeof(INetworkEvents), value);
		}
		remove
		{
			SinkBase.RemoveSink<NetworkEventsSink>(value);
		}
	}

	public static event NetworkDeletedEvent NetworkDeleted
	{
		add
		{
			SinkBase.AddSink<NetworkEventsSink>((IConnectionPointContainer)manager, typeof(INetworkEvents), value);
		}
		remove
		{
			SinkBase.RemoveSink<NetworkEventsSink>(value);
		}
	}

	public static event NetworkPropertyChangedEvent NetworkPropertyChanged
	{
		add
		{
			SinkBase.AddSink<NetworkEventsSink>((IConnectionPointContainer)manager, typeof(INetworkEvents), value);
		}
		remove
		{
			SinkBase.RemoveSink<NetworkEventsSink>(value);
		}
	}

	public static event NetworkConnectionConnectivityChangedEvent NetworkConnectionConnectivityChanged
	{
		add
		{
			SinkBase.AddSink<NetworkConnectionEventsSink>((IConnectionPointContainer)manager, typeof(INetworkConnectionEvents), value);
		}
		remove
		{
			SinkBase.RemoveSink<NetworkConnectionEventsSink>(value);
		}
	}

	public static event NetworkConnectionPropertyChangedEvent NetworkConnectionPropertyChanged
	{
		add
		{
			SinkBase.AddSink<NetworkConnectionEventsSink>((IConnectionPointContainer)manager, typeof(INetworkConnectionEvents), value);
		}
		remove
		{
			SinkBase.RemoveSink<NetworkConnectionEventsSink>(value);
		}
	}

	static NetworkListManager()
	{
		manager = new NetworkListManagerClass();
		CoreHelpers.ThrowIfNotVista();
	}

	public static NetworkCollection GetNetworks(NetworkConnectivityLevels level)
	{
		return new NetworkCollection(manager.GetNetworks(level));
	}

	public static Network GetNetwork(Guid networkId)
	{
		try
		{
			return new Network(manager.GetNetwork(networkId));
		}
		catch (COMException ex) when (ex.ErrorCode == -2147418113)
		{
			return null;
		}
	}

	public static NetworkConnectionCollection GetNetworkConnections()
	{
		return new NetworkConnectionCollection(manager.GetNetworkConnections());
	}

	public static NetworkConnection GetNetworkConnection(Guid networkConnectionId)
	{
		INetworkConnection networkConnection;
		try
		{
			networkConnection = manager.GetNetworkConnection(networkConnectionId);
		}
		catch (COMException ex) when (ex.ErrorCode == -2147418113)
		{
			return null;
		}
		if (networkConnection == null)
		{
			return null;
		}
		return new NetworkConnection(networkConnection);
	}
}
