using System.Linq;
using DotRas;

namespace VpnSDK.Private.Ras.Utilities;

internal static class RasDeviceFinder
{
	internal static RasDevice Find(RasConnectionType connectionType)
	{
		try
		{
			return RasDevice.GetDevices().FirstOrDefault((RasDevice x) => x.DeviceType == RasDeviceType.Vpn && x.Name.Contains("(" + connectionType.ToString() + ")"));
		}
		catch
		{
			return null;
		}
	}
}
