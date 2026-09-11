using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Extensions;
using VpnSDK.Internal.Helpers;
using VpnSDK.Private.API.Common.Enums;
using VpnSDK.Private.API.DTO;

namespace VpnSDK.Internal.Dtos;

internal class ServerProxy : BindableBase, IServer, ILocation, INotifyPropertyChanged
{
	public bool InMaintenance => Node?.InMaintenance ?? false;

	public string Id => Node?.Id;

	public string CountryCode => Node?.Parent?.GetParent<Location>().Id;

	public string CityCode => Node?.GetParent<Location>().Id;

	public string SearchName { get; }

	public ushort? PingMs { get; internal set; }

	public string Hostname => Node?.Hostname;

	public IPAddress Ip => Node?.IP;

	public string Country => Node?.Parent?.GetParent<Location>().Name;

	public string City => Node?.GetParent<Location>().Name;

	public short Load => Node?.Load ?? (-1);

	public List<ushort> OpenVpnScramblePorts => Node?.Configuration.OpenVpn.Ports.Where((KeyValuePair<EncryptionLevel, List<ushort>> x) => x.Key == EncryptionLevel.Scrambled).SelectMany((KeyValuePair<EncryptionLevel, List<ushort>> x) => x.Value).ToList();

	public List<ushort> OpenVpnPorts => Node?.Configuration.OpenVpn.Ports.Where((KeyValuePair<EncryptionLevel, List<ushort>> x) => x.Key != EncryptionLevel.Scrambled).SelectMany((KeyValuePair<EncryptionLevel, List<ushort>> x) => x.Value).ToList();

	public List<NetworkConnectionType> AvailableProtocols => Node?.Configuration.GetNetworkConnectionTypes();

	internal bool HasNode => Node != null;

	internal Server Node { get; private set; }

	public ServerProxy(Server nodeToProxy)
	{
		Node = nodeToProxy;
	}

	public override string ToString()
	{
		return Node?.Hostname ?? "";
	}

	public async Task<ushort?> Ping()
	{
		ushort? result = (PingMs = await Hostname.Ping().ConfigureAwait(continueOnCapturedContext: false));
		OnPropertyChanged("PingMs");
		return result;
	}

	internal void UpdateProxiedObject(Server newNode)
	{
		Node = newNode;
	}
}
