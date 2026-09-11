using System;
using Microsoft.Win32.SafeHandles;

namespace VpnSDK.Private.WFP.Interop;

internal class SafeWFPFilterEnumHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	private static SafeWFPEngineHandle _engine;

	private SafeWFPFilterEnumHandle()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		if (handle != IntPtr.Zero && WFPNativeMethods.FwpmFilterDestroyEnumHandle0(_engine, handle) == 0)
		{
			SetHandleAsInvalid();
			handle = IntPtr.Zero;
			return true;
		}
		return false;
	}

	public static SafeWFPFilterEnumHandle CreateHandle(SafeWFPEngineHandle engine, ref FWPM_FILTER_ENUM_TEMPLATE0 template)
	{
		SafeWFPFilterEnumHandle safeWFPFilterEnumHandle = new SafeWFPFilterEnumHandle();
		_engine = engine;
		WFPNativeMethods.FwpmFilterCreateEnumHandle0(_engine, ref template, out safeWFPFilterEnumHandle.handle);
		return safeWFPFilterEnumHandle;
	}
}
