using System.Runtime.InteropServices;

namespace VpnSDK.Common.Interop;

internal struct MibIpForwardTable
{
	internal uint Size;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
	internal MibIpForwardRow[] Table;
}
