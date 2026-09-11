using System;
using Microsoft.Win32.SafeHandles;

namespace VpnSDK.Private.WFP.Interop;

internal class SafeWFPEngineHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	private SafeWFPEngineHandle()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		if (WFPNativeMethods.FwpmEngineClose0(handle) == 0)
		{
			SetHandleAsInvalid();
			handle = IntPtr.Zero;
			return true;
		}
		return false;
	}

	public static SafeWFPEngineHandle CreateEngine(ref FWPM_SESSION0 session)
	{
		SafeWFPEngineHandle safeWFPEngineHandle = new SafeWFPEngineHandle();
		WFPNativeMethods.OpenEngine(ref session, out safeWFPEngineHandle.handle);
		return safeWFPEngineHandle;
	}
}
