using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWP_BYTE_ARRAY16
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public byte[] byteArray16;
}
