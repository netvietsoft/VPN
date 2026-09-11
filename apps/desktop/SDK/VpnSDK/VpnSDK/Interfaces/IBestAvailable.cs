using System.ComponentModel;

namespace VpnSDK.Interfaces;

public interface IBestAvailable : ILocation, INotifyPropertyChanged
{
	IRegion BestRegion { get; }
}
