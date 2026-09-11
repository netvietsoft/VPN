using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal struct SID
{
	public byte Revision;

	public byte SubAuthorityCount;

	public SID_IDENTIFIER_AUTHORITY IdentifierAuthority;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
	public uint[] SubAuthority;
}
