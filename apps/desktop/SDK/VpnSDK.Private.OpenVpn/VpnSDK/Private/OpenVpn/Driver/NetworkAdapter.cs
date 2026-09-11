using VpnSDK.Private.OpenVpn.Enums;

namespace VpnSDK.Private.OpenVpn.Driver;

public class NetworkAdapter
{
	public TapDriver Driver { get; set; }

	public string FriendlyName { get; set; }

	public NetworkAdapter(TapDriver driver, string friendlyName)
	{
		Driver = driver;
		FriendlyName = friendlyName;
	}
}
