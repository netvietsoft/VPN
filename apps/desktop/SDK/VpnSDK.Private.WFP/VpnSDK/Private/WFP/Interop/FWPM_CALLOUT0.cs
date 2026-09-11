using System;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWPM_CALLOUT0
{
	public GUID calloutKey;

	public FWPM_DISPLAY_DATA0 displayData;

	public uint flags;

	public IntPtr providerKey;

	public FWP_BYTE_BLOB providerData;

	public GUID applicableLayer;

	public uint calloutId;
}
