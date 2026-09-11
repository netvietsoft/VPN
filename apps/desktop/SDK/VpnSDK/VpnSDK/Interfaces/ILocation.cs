using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using VpnSDK.Enums;

namespace VpnSDK.Interfaces;

public interface ILocation : INotifyPropertyChanged
{
	string Id { get; }

	string CountryCode { get; }

	string CityCode { get; }

	string SearchName { get; }

	ushort? PingMs { get; }

	string Country { get; }

	string City { get; }

	List<NetworkConnectionType> AvailableProtocols { get; }

	Task<ushort?> Ping();
}
