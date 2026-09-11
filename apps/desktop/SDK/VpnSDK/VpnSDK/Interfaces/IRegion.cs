using System.ComponentModel;
using System.Threading.Tasks;
using GeoCoordinatePortable;

namespace VpnSDK.Interfaces;

public interface IRegion : ILocation, INotifyPropertyChanged
{
	GeoCoordinate GeoCoordinate { get; }

	ushort? Load { get; }

	Task<ushort?> PingAll();
}
