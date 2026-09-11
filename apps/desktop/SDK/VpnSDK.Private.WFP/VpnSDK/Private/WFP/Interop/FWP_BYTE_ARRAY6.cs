using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWP_BYTE_ARRAY6
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
	public byte[] byteArray6;
}
