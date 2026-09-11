using System;

namespace VpnSDK.Private.WFP.Interop;

internal struct SID_AND_ATTRIBUTES
{
	public IntPtr Sid;

	public uint Attributes;
}
