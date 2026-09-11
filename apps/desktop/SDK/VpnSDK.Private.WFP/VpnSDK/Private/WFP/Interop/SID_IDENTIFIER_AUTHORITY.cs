using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal struct SID_IDENTIFIER_AUTHORITY
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
	public byte[] Value;
}
