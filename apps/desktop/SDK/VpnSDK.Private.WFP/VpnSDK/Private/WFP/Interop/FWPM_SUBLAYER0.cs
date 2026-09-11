using System;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWPM_SUBLAYER0
{
	public GUID subLayerKey;

	public FWPM_DISPLAY_DATA0 displayData;

	public uint Flags;

	public IntPtr providerKey;

	public FWP_BYTE_BLOB providerData;

	public ushort weight;

	public string Name
	{
		get
		{
			return displayData.Name;
		}
		set
		{
			displayData.Name = value;
		}
	}
}
