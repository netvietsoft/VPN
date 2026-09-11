using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWP_V6_ADDR_AND_MASK
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public byte[] addr;

	public byte prefixLength;
}
