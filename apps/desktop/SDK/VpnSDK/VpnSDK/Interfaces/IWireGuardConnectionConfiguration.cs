namespace VpnSDK.Interfaces;

public interface IWireGuardConnectionConfiguration : IConnectionConfiguration, IDoubleHopConfiguration
{
	bool AllowLan { get; set; }

	int? Mtu { get; set; }

	short ServiceStartTimeoutInSeconds { get; set; }
}
