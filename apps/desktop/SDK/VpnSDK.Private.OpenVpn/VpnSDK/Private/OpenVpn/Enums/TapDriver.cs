using System.ComponentModel;

namespace VpnSDK.Private.OpenVpn.Enums;

public enum TapDriver
{
	[Description("NotInstalled")]
	NotInstalled,
	[Description("WLVPN Windows Tap Adapter")]
	tapwlvpn,
	[Description("TAP-Windows Adapter V9")]
	tap0901
}
