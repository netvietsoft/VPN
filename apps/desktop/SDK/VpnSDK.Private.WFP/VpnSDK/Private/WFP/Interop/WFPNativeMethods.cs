using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal class WFPNativeMethods
{
	[DllImport("Fwpuclnt.dll")]
	public static extern void FwpmFreeMemory0(ref IntPtr p);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmEngineOpen0([In][MarshalAs(UnmanagedType.LPWStr)] string serverName, [In] uint authnService, IntPtr authIdentity, ref FWPM_SESSION0 session, out IntPtr engineHandle);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmEngineOpen0([In][MarshalAs(UnmanagedType.LPWStr)] string serverName, [In] uint authnService, ref SEC_WINNT_AUTH_IDENTITY_W authIdentity, ref FWPM_SESSION0 session, out IntPtr engineHandle);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmEngineClose0([In] IntPtr engineHandle);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmTransactionBegin0([In] SafeWFPEngineHandle engineHandle, [In] uint flags);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmTransactionCommit0([In] SafeWFPEngineHandle engineHandle);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmTransactionAbort0([In] SafeWFPEngineHandle engineHandle);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmProviderAdd0([In] SafeWFPEngineHandle engineHandle, ref FWPM_PROVIDER0 provider, [In] IntPtr sd);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmProviderDeleteByKey0([In] SafeWFPEngineHandle engineHandle, ref GUID key);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmSubLayerAdd0([In] SafeWFPEngineHandle engineHandle, ref FWPM_SUBLAYER0 subLayer, [In] IntPtr sd);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmSubLayerDeleteByKey0([In] SafeWFPEngineHandle engineHandle, ref GUID key);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmFilterAdd0([In] SafeWFPEngineHandle engineHandle, ref FWPM_FILTER0 filter, [In] IntPtr sd, ref ulong id);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmFilterDeleteById0([In] SafeWFPEngineHandle engineHandle, [In] ulong id);

	[DllImport("Fwpuclnt.dll")]
	public static extern uint FwpmFilterDeleteByKey0([In] SafeWFPEngineHandle engineHandle, [In] ref GUID key);

	[DllImport("Fwpuclnt.dll", CharSet = CharSet.Unicode)]
	public static extern uint FwpmGetAppIdFromFileName0([In][MarshalAs(UnmanagedType.LPWStr)] string fileName, ref IntPtr appId);

	[DllImport("Fwpuclnt.dll", CharSet = CharSet.Unicode)]
	public static extern uint FwpmProviderCreateEnumHandle0([In] SafeWFPEngineHandle engineHandle, IntPtr enumTemplate, out IntPtr enumHandle);

	[DllImport("Fwpuclnt.dll", CharSet = CharSet.Unicode)]
	internal static extern uint FwpmProviderDestroyEnumHandle0([In] SafeWFPEngineHandle engineHandle, [In] IntPtr enumHandle);

	[DllImport("Fwpuclnt.dll", CharSet = CharSet.Unicode)]
	public static extern uint FwpmSubLayerCreateEnumHandle0([In] SafeWFPEngineHandle engineHandle, IntPtr enumTemplate, out IntPtr enumHandle);

	[DllImport("Fwpuclnt.dll", CharSet = CharSet.Unicode)]
	internal static extern uint FwpmSubLayerDestroyEnumHandle0([In] SafeWFPEngineHandle engineHandle, [In] IntPtr enumHandle);

	[DllImport("Fwpuclnt.dll", CharSet = CharSet.Unicode)]
	internal static extern uint FwpmProviderEnum0([In] SafeWFPEngineHandle engineHandle, [In] SafeWFPProviderEnumHandle enumHandle, [In] uint numEntriesRequested, out IntPtr entries, out uint numEntriesReturned);

	[DllImport("Fwpuclnt.dll", CharSet = CharSet.Unicode)]
	internal static extern uint FwpmSubLayerEnum0([In] SafeWFPEngineHandle engineHandle, [In] SafeWFPSublayerEnumHandle enumHandle, [In] uint numEntriesRequested, out IntPtr entries, out uint numEntriesReturned);

	[DllImport("Fwpuclnt.dll", CharSet = CharSet.Unicode)]
	internal static extern uint FwpmFilterCreateEnumHandle0([In] SafeWFPEngineHandle engineHandle, [In] ref FWPM_FILTER_ENUM_TEMPLATE0 enumTemplate, out IntPtr enumHandle);

	[DllImport("Fwpuclnt.dll", CharSet = CharSet.Unicode)]
	internal static extern uint FwpmFilterDestroyEnumHandle0([In] SafeWFPEngineHandle engineHandle, [In] IntPtr enumHandle);

	[DllImport("Fwpuclnt.dll", CharSet = CharSet.Unicode)]
	internal static extern uint FwpmFilterEnum0([In] SafeWFPEngineHandle engineHandle, [In] SafeWFPFilterEnumHandle enumHandle, [In] uint numEntriesRequested, out IntPtr entries, out uint numEntriesReturned);

	public static void OpenEngine(ref FWPM_SESSION0 session, out IntPtr engineHandle)
	{
		ErrorCheck(FwpmEngineOpen0(null, 10u, IntPtr.Zero, ref session, out engineHandle));
	}

	public static void StartTransaction(SafeWFPEngineHandle engineHandle)
	{
		ErrorCheck(FwpmTransactionBegin0(engineHandle, 0u));
	}

	public static void EndTransaction(SafeWFPEngineHandle engineHandle)
	{
		try
		{
			ErrorCheck(FwpmTransactionCommit0(engineHandle));
		}
		catch
		{
			FwpmTransactionAbort0(engineHandle);
			throw;
		}
	}

	public static void AddProvider(SafeWFPEngineHandle engineHandle, ref FWPM_PROVIDER0 provider)
	{
		ErrorCheck(FwpmProviderAdd0(engineHandle, ref provider, IntPtr.Zero));
	}

	public static void AddSublayer(SafeWFPEngineHandle engineHandle, ref FWPM_SUBLAYER0 sublayer)
	{
		ErrorCheck(FwpmSubLayerAdd0(engineHandle, ref sublayer, IntPtr.Zero));
	}

	public static void AddFilter(SafeWFPEngineHandle engineHandle, ref FWPM_FILTER0 filter, out ulong id)
	{
		id = 0uL;
		ErrorCheck(FwpmFilterAdd0(engineHandle, ref filter, IntPtr.Zero, ref id));
	}

	public static void RemoveFilter(SafeWFPEngineHandle engineHandle, ulong id)
	{
		ErrorCheck(FwpmFilterDeleteById0(engineHandle, id));
	}

	public static void GetApplicationId(string applicationPath, out IntPtr ptr)
	{
		ptr = IntPtr.Zero;
		ErrorCheck(FwpmGetAppIdFromFileName0(applicationPath, ref ptr));
	}

	public static void ErrorCheck(uint result, bool allowExists = false)
	{
		if ((!allowExists || result != 2150760457u) && result != 0)
		{
			if (Enum.IsDefined(typeof(WFPError), result))
			{
				throw new WFPException((WFPError)result);
			}
			throw new Win32Exception((int)result);
		}
	}
}
