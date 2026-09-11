using System.Collections.ObjectModel;

namespace VpnSDK.Interfaces;

public interface IChildren<T>
{
	ObservableCollection<T> Children { get; }
}
