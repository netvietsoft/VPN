using System;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace VpnSDK.Private.WFP.Interop;

internal class SafeWFPHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	private SafeWFPHandle()
		: base(ownsHandle: true)
	{
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	protected override bool ReleaseHandle()
	{
		if (handle == IntPtr.Zero)
		{
			return true;
		}
		WFPNativeMethods.FwpmFreeMemory0(ref handle);
		handle = IntPtr.Zero;
		SetHandleAsInvalid();
		return true;
	}

	public static SafeWFPHandle CreateApplicationHandle(string applicationPath)
	{
		SafeWFPHandle safeWFPHandle = new SafeWFPHandle();
		WFPNativeMethods.GetApplicationId(applicationPath, out safeWFPHandle.handle);
		return safeWFPHandle;
	}
}
