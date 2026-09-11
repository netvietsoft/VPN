using System;
using System.IO;
using VpnSDK.Core.Helpers;

namespace VpnSDK.DTO;

public class WireguardConfiguration
{
	private const string WireGuardFolder = "WireGuard";

	public string TunDeviceDescription { get; set; } = "WireGuard Tunnel";

	public string ConnectionName { get; set; }

	public string ConfigDirectory { get; set; }

	public WireguardConfiguration()
	{
		ConfigDirectory = Path.Combine(path2: ConnectionName = Path.GetFileNameWithoutExtension(PathHelper.GetApplicationFilePath()), path1: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), path3: "WireGuard");
	}
}
