using System;
using Microsoft.Win32.SafeHandles;

namespace VpnSDK.Private.WFP.Interop;

internal class SafeWFPSublayerEnumHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	private static SafeWFPEngineHandle _engine;

	private SafeWFPSublayerEnumHandle()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		if (handle != IntPtr.Zero && WFPNativeMethods.FwpmSubLayerDestroyEnumHandle0(_engine, handle) == 0)
		{
			SetHandleAsInvalid();
			handle = IntPtr.Zero;
			return true;
		}
		return false;
	}

	public static SafeWFPSublayerEnumHandle CreateHandle(SafeWFPEngineHandle engine)
	{
		SafeWFPSublayerEnumHandle safeWFPSublayerEnumHandle = new SafeWFPSublayerEnumHandle();
		_engine = engine;
		WFPNativeMethods.FwpmSubLayerCreateEnumHandle0(_engine, IntPtr.Zero, out safeWFPSublayerEnumHandle.handle);
		return safeWFPSublayerEnumHandle;
	}
}
