using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Helpers;

namespace VpnSDK.Internal.Dtos;

internal class BestAvailable : BindableBase, IBestAvailable, ILocation, INotifyPropertyChanged
{
	public string Id { get; } = "bestavailable";

	public string CountryCode => string.Empty;

	public string CityCode => string.Empty;

	public string SearchName { get; internal set; }

	public ushort? PingMs { get; }

	public IRegion BestRegion => null;

	public string Country => string.Empty;

	public string City => string.Empty;

	public List<NetworkConnectionType> AvailableProtocols { get; }

	internal BestAvailable(string searchName = "Best Available")
	{
		SearchName = searchName;
	}

	public Task<ushort?> Ping()
	{
		return Task.FromResult<ushort?>(null);
	}

	public override string ToString()
	{
		return SearchName;
	}
}
