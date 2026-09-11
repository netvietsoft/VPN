using System;
using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal struct FWPM_SESSION0
{
	public GUID sessionKey;

	public FWPM_DISPLAY_DATA0 displayData;

	public uint Flags;

	public uint txnWaitTimeoutInMSec;

	public uint processId;

	public IntPtr sid;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string username;

	public int kernelMode;

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
