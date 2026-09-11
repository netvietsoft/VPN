using System;
using Microsoft.Win32.SafeHandles;

namespace VpnSDK.Private.WFP.Interop;

internal class SafeWFPProviderEnumHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	private static SafeWFPEngineHandle _engine;

	private SafeWFPProviderEnumHandle()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		if (handle != IntPtr.Zero && WFPNativeMethods.FwpmProviderDestroyEnumHandle0(_engine, handle) == 0)
		{
			SetHandleAsInvalid();
			handle = IntPtr.Zero;
			return true;
		}
		return false;
	}

	public static SafeWFPProviderEnumHandle CreateHandle(SafeWFPEngineHandle engine)
	{
		SafeWFPProviderEnumHandle safeWFPProviderEnumHandle = new SafeWFPProviderEnumHandle();
		_engine = engine;
		WFPNativeMethods.FwpmProviderCreateEnumHandle0(_engine, IntPtr.Zero, out safeWFPProviderEnumHandle.handle);
		return safeWFPProviderEnumHandle;
	}
}
