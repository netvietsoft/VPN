using System;

namespace Microsoft.WindowsAPICodePack.Net;

internal class Network
{
	private INetwork network;

	public NetworkCategory Category
	{
		get
		{
			return network.GetCategory();
		}
		set
		{
			network.SetCategory(value);
		}
	}

	public DateTime ConnectedTime
	{
		get
		{
			network.GetTimeCreatedAndConnected(out var _, out var _, out var pdwLowDateTimeConnected, out var pdwHighDateTimeConnected);
			return DateTime.FromFileTimeUtc((long)(((ulong)pdwHighDateTimeConnected << 32) | pdwLowDateTimeConnected));
		}
	}

	public NetworkConnectionCollection Connections => new NetworkConnectionCollection(network.GetNetworkConnections());

	public ConnectivityStates Connectivity => network.GetConnectivity();

	public DateTime CreatedTime
	{
		get
		{
			network.GetTimeCreatedAndConnected(out var pdwLowDateTimeCreated, out var pdwHighDateTimeCreated, out var _, out var _);
			return DateTime.FromFileTimeUtc((long)(((ulong)pdwHighDateTimeCreated << 32) | pdwLowDateTimeCreated));
		}
	}

	public string Description
	{
		get
		{
			return network.GetDescription();
		}
		set
		{
			network.SetDescription(value);
		}
	}

	public DomainType DomainType => network.GetDomainType();

	public bool IsConnected => network.IsConnected;

	public bool IsConnectedToInternet => network.IsConnectedToInternet;

	public string Name
	{
		get
		{
			return network.GetName();
		}
		set
		{
			network.SetName(value);
		}
	}

	public Guid NetworkId => network.GetNetworkId();

	public Network(INetwork network)
	{
		this.network = network;
	}
}
