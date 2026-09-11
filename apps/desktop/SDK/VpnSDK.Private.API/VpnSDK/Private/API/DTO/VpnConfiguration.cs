using System.ComponentModel;

namespace VpnSDK.Private.API.DTO;

public class VpnConfiguration
{
	public VpnType Protocols { get; set; } = VpnType.IKEv2 | VpnType.OpenVPN | VpnType.WireGuard;

	public OpenVpnServerConfiguration OpenVpn { get; internal set; } = new OpenVpnServerConfiguration();

	public string PresharedKey { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializePresharedKey()
	{
		return !string.IsNullOrEmpty(PresharedKey);
	}
}
