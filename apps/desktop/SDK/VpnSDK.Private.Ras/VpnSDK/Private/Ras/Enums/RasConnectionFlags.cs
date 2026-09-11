using System;

namespace VpnSDK.Private.Ras.Enums;

[Flags]
public enum RasConnectionFlags
{
	None = 0,
	RequireMSChap2 = 1,
	RequireChap = 2,
	RequireMSChap = 4,
	RequiredDataEncryption = 8,
	NetworkLogOn = 0x10,
	ShowDialingProcess = 0x20,
	RequireEap = 0x40,
	UsePresharedKey = 0x80,
	UseMachineCertificates = 0x100
}
