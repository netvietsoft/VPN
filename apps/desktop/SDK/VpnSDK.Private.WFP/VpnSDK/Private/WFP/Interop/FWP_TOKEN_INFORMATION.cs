using System;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWP_TOKEN_INFORMATION
{
	public uint sidCount;

	public IntPtr sids;

	public uint restrictedSidCount;

	public IntPtr restrictedSids;
}
