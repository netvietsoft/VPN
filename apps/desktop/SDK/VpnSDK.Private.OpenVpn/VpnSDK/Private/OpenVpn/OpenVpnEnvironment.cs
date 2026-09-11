using System.IO;

namespace VpnSDK.Private.OpenVpn;

internal class OpenVpnEnvironment
{
	public string ApplicationDirectory { get; set; }

	public string ExecutableName { get; set; } = "openvpn.exe";

	public bool Validate()
	{
		if (Directory.Exists(ApplicationDirectory))
		{
			return File.Exists(Path.Combine(ApplicationDirectory, ExecutableName));
		}
		return false;
	}
}
