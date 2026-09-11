using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWPM_PROVIDER0
{
	public GUID providerKey;

	public FWPM_DISPLAY_DATA0 displayData;

	public uint Flags;

	public FWP_BYTE_BLOB providerData;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string serviceName;

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
