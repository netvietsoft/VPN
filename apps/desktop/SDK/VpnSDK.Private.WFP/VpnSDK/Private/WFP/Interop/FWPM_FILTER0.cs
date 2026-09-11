using System;
using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWPM_FILTER0
{
	[StructLayout(LayoutKind.Explicit)]
	public struct FWPM_FILTER0_Union
	{
		[FieldOffset(0)]
		public ulong rawContext;

		[FieldOffset(0)]
		public GUID providerContextKey;
	}

	public GUID filterKey;

	public FWPM_DISPLAY_DATA0 displayData;

	public WFPFilterFlag flags;

	public IntPtr providerKey;

	public FWP_BYTE_BLOB providerData;

	public GUID layerKey;

	public GUID subLayerKey;

	public FWP_VALUE0 weight;

	public uint numFilterConditions;

	public IntPtr filterCondition;

	public FWPM_ACTION0 action;

	public GUID providerContextKey;

	public IntPtr reserved;

	public ulong filterId;

	public FWP_VALUE0 effectiveWeight;
}
