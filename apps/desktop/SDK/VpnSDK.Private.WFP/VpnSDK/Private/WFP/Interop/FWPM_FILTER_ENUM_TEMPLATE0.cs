using System;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWPM_FILTER_ENUM_TEMPLATE0
{
	public IntPtr providerKey;

	public GUID layerKey;

	public FWP_FILTER_ENUM_TYPE enumType;

	public uint flags;

	public IntPtr providerContextTemplate;

	public uint numFilterConditions;

	public IntPtr filterCondition;

	public uint actionMask;

	public IntPtr calloutKey;

	public void Clear()
	{
		providerKey = IntPtr.Zero;
		layerKey = Guid.Empty;
		enumType = FWP_FILTER_ENUM_TYPE.FWP_FILTER_ENUM_FULLY_CONTAINED;
		flags = 0u;
		providerContextTemplate = IntPtr.Zero;
		numFilterConditions = 0u;
		filterCondition = IntPtr.Zero;
		actionMask = 0u;
		calloutKey = IntPtr.Zero;
	}
}
