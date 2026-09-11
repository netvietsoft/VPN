using System.Diagnostics;

namespace VpnSDK.DTO;

public class RasConfiguration
{
	public string RasDeviceDescription { get; set; } = Process.GetCurrentProcess().ProcessName;

	public bool UseConnectionTypeInName { get; set; }
}
