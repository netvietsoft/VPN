using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Helpers;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Internal.Dtos;

internal class RegionProxy : BindableBase, IRegion, ILocation, INotifyPropertyChanged, IChildren<IServer>
{
	private SmartCollection<IServer> _serversBacked = new SmartCollection<IServer>();

	public ObservableCollection<IServer> Children => _serversBacked;

	public string City => Node?.Name;

	public string CityCode => Node?.Id;

	public string Country => Node?.GetParent<Location>().Name;

	public string CountryCode => Node?.GetParent<Location>().Id;

	public GeoCoordinate GeoCoordinate
	{
		get
		{
			if (Node?.GeoCoordinate != null)
			{
				return new GeoCoordinate(Node.GeoCoordinate.Item1, Node.GeoCoordinate.Item2);
			}
			return new GeoCoordinate(0.0, 0.0);
		}
	}

	public string Id => Node?.Id;

	public ushort? Load => (ushort?)Children?.Average((IServer x) => x.Load);

	public string SearchName => ToString();

	public ushort? PingMs { get; internal set; }

	public List<NetworkConnectionType> AvailableProtocols { get; }

	internal bool HasNode => Node != null;

	internal Location Node { get; private set; }

	public RegionProxy(Location nodeToProxy)
	{
		Node = nodeToProxy;
		AvailableProtocols = new List<NetworkConnectionType>();
		if (Node != null)
		{
			List<ServerProxy> list = (from x in Node.GetChildren<Server>()
				select new ServerProxy(x)).ToList();
			_serversBacked.AddRange(list);
			AvailableProtocols.AddRange(list.SelectMany((ServerProxy x) => x.AvailableProtocols).Distinct().ToList());
		}
		else
		{
			_serversBacked.Clear();
		}
	}

	public async Task<ushort?> Ping()
	{
		try
		{
			ushort?[] source = await Task.WhenAll(Children?.OrderBy((IServer x) => x.Load).Where((IServer x) => x != null && x.Hostname != null).ToList()
				.Take(2)
				.Select((IServer x) => x.Hostname.Ping())).ConfigureAwait(continueOnCapturedContext: false);
			if (!source.All((ushort? x) => !x.HasValue))
			{
				PingMs = Convert.ToUInt16(Math.Round(source.Where((ushort? x) => x.HasValue).Average((ushort? x) => x.Value)));
				return PingMs;
			}
			PingMs = null;
			OnPropertyChanged("PingMs");
		}
		catch
		{
		}
		return null;
	}

	public async Task<ushort?> PingAll()
	{
		try
		{
			ushort?[] array = await Task.WhenAll(_serversBacked.Select((IServer x) => x?.Ping())).ConfigureAwait(continueOnCapturedContext: false);
			if (array == null || (array != null && array.All((ushort? x) => !x.HasValue)))
			{
				PingMs = null;
			}
			else
			{
				PingMs = Convert.ToUInt16(Math.Round(array.Where((ushort? x) => x.HasValue).Average((ushort? x) => x.Value)));
			}
			OnPropertyChanged("PingMs");
			return PingMs;
		}
		catch
		{
		}
		return null;
	}

	public override string ToString()
	{
		return Node?.Name + ", " + Node?.GetParent<Location>()?.Name;
	}

	internal void UpdateProxiedObject(Location newNode)
	{
		Node = newNode;
		if (Node == null)
		{
			_serversBacked.Clear();
			return;
		}
		List<Server> list = (from x in Node.GetChildren<Server>()
			where !x.InMaintenance
			select x).ToList();
		List<ServerProxy> list2 = new List<ServerProxy>();
		List<ServerProxy> list3 = new List<ServerProxy>();
		foreach (ServerProxy proxyServer in _serversBacked)
		{
			Server newNode2 = list.FirstOrDefault((Server x) => x.Id == proxyServer.Id);
			proxyServer.UpdateProxiedObject(newNode2);
			if (!proxyServer.HasNode)
			{
				list2.Add(proxyServer);
			}
		}
		_serversBacked.RemoveRange(list2);
		foreach (Server newNodeServer in list)
		{
			if (_serversBacked.All((IServer x) => x.Id != newNodeServer.Id))
			{
				list3.Add(new ServerProxy(newNodeServer));
			}
		}
		_serversBacked.AddRange(list3);
		OnPropertyChanged("Load");
	}
}
