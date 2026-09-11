using System;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWP_BYTE_BLOB
{
	public uint size;

	public IntPtr data;
}
