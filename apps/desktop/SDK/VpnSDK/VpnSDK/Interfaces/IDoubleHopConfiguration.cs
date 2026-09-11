using VpnSDK.Common.Settings;

namespace VpnSDK.Interfaces;

public interface IDoubleHopConfiguration
{
	DoubleHopSettings DoubleHopSettings { get; }
}
