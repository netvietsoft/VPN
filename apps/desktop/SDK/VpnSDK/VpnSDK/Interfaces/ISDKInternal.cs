using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using VpnSDK.Private.API.DTO;
using VpnSDK.TrafficOptimizer.DTO;

namespace VpnSDK.Interfaces;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ISDKInternal : ISDK, INotifyPropertyChanged, IDisposable
{
	event Action<TrafficOptimizerArgs> TrafficUpdate;

	List<string> GetActiveRasConnections();

	void DebugDropServerInfo(int ignoreNumber = 0);

	void DebugDropLocation(ILocation location);

	Task RefreshServerInfoForced();

	Task InvalidateUser();

	void SetTokenExpire(DateTime expireDateTime);

	void SetApiTimeout(int timeoutInSeconds);

	void SetUserLocation(double latitude, double longitude, string countryCode);

	IList<Server> GetOptimalServers();
}
