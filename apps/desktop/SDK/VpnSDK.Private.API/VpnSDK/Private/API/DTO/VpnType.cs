using System;

namespace VpnSDK.Private.API.DTO;

[Flags]
public enum VpnType : byte
{
	None = 0,
	IKEv2 = 1,
	SSTP = 2,
	L2TP = 4,
	PPTP = 8,
	OpenVPN = 0x10,
	WireGuard = 0x20
}
