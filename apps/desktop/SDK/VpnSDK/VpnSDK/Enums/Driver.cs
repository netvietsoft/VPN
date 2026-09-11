using System;

namespace VpnSDK.Enums;

[Flags]
public enum Driver
{
	None = 0,
	OpenVPN = 1,
	Callout = 2
}
