using System;

namespace VpnSDK.Private.WFP.Interop;

[Flags]
internal enum WFPFilterFlag : uint
{
	None = 0u,
	Persistent = 1u,
	BootTime = 2u,
	HasProviderContext = 4u,
	ClearActionRight = 8u,
	PermitIfCalloutUnregistered = 0x10u,
	Disabled = 0x20u,
	Indexed = 0x40u,
	HasSecureityRealmProviderContext = 0x80u
}
