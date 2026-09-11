using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWPM_DISPLAY_DATA0
{
	[MarshalAs(UnmanagedType.LPWStr)]
	public string Name;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string Description;
}
