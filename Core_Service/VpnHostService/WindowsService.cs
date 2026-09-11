using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace VpnHostService;

internal static class WindowsService
{
	[Flags]
	public enum ScmAccessRights
	{
		Connect = 1,
		CreateService = 2,
		EnumerateService = 4,
		Lock = 8,
		QueryLockStatus = 0x10,
		ModifyBootConfig = 0x20,
		StandardRightsRequired = 0xF0000,
		AllAccess = StandardRightsRequired | Connect | CreateService | EnumerateService | Lock | QueryLockStatus | ModifyBootConfig
	}

	[Flags]
	public enum ServiceAccessRights
	{
		QueryConfig = 1,
		ChangeConfig = 2,
		QueryStatus = 4,
		EnumerateDependants = 8,
		Start = 0x10,
		Stop = 0x20,
		PauseContinue = 0x40,
		Interrogate = 0x80,
		UserDefinedControl = 0x100,
		Delete = 0x10000,
		StandardRightsRequired = 0xF0000,
		AllAccess = StandardRightsRequired | QueryConfig | ChangeConfig | QueryStatus | EnumerateDependants | Start | Stop | PauseContinue | Interrogate | UserDefinedControl
	}

	[Flags]
	public enum ServiceStartType
	{
		Boot = 0,
		System = 1,
		Auto = 2,
		Demand = System | Auto,
		Disabled = 4
	}

	[Flags]
	public enum ServiceControl
	{
		Stop = 1,
		Pause = 2,
		Continue = Stop | Pause,
		Interrogate = 4,
		Shutdown = Stop | Interrogate,
		ParamChange = Pause | Interrogate,
		NetBindAdd = Continue | Interrogate,
		NetBindRemove = 8,
		NetBindEnable = Stop | NetBindRemove,
		NetBindDisable = Pause | NetBindRemove
	}

	[Flags]
	public enum ServiceError
	{
		Ignore = 0,
		Normal = 1,
		Severe = 2,
		Critical = Normal | Severe
	}

	[Flags]
	public enum ServiceSidType
	{
		None = 0,
		Unrestricted = 1,
		Restricted = 3
	}

	[Flags]
	public enum ServiceType
	{
		KernelDriver = 1,
		FileSystemDriver = 2,
		Win32OwnProcess = 0x10,
		Win32ShareProcess = 0x20,
		InteractiveProcess = 0x100
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Size = 8192)]
	[ComVisible(false)]
	public struct ServiceSidInfo
	{
		public ServiceSidType serviceSidType;
	}

	public enum ServiceState
	{
		Unknown = -1,
		NotFound,
		Stopped,
		StartPending,
		StopPending,
		Running,
		ContinuePending,
		PausePending,
		Paused
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ServiceStatus
	{
		public int dwServiceType;

		public ServiceState dwCurrentState;

		public int dwControlsAccepted;

		public int dwWin32ExitCode;

		public int dwServiceSpecificExitCode;

		public int dwCheckPoint;

		public int dwWaitHint;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Size = 8192)]
	[ComVisible(false)]
	public struct ServiceDescription
	{
		public string lpDescription;
	}

	public enum ServiceConfigType
	{
		Description = 1,
		SidInfo = 5
	}

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "OpenSCManagerW", ExactSpelling = true, SetLastError = true)]
	public static extern IntPtr OpenSCManager(string machineName, string databaseName, ScmAccessRights dwDesiredAccess);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, ServiceAccessRights dwDesiredAccess);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool CloseServiceHandle(IntPtr hSCObject);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool StartService(IntPtr hService, int dwNumServiceArgs, string[] lpServiceArgVectors);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName, ServiceAccessRights dwDesiredAccess, ServiceType dwServiceType, ServiceStartType dwStartType, ServiceError dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lp, string lpPassword);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeleteService(IntPtr hService);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ControlService(IntPtr hService, ServiceControl dwControl, ServiceStatus lpServiceStatus);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool QueryServiceStatus(IntPtr hService, ServiceStatus lpServiceStatus);

	[DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ChangeServiceConfig2(IntPtr hService, ServiceConfigType dwInfoLevel, ref ServiceSidType lpInfo);

	[DllImport("advapi32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ChangeServiceConfig2(IntPtr hService, ServiceConfigType dwInfoLevel, ref ServiceDescription lpInfo);

	public static void Remove(string configFile, bool waitForStop)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(configFile);
		string lpServiceName = $"WireGuardTunnel${fileNameWithoutExtension}";
		IntPtr intPtr = OpenSCManager(null, null, ScmAccessRights.AllAccess);
		if (intPtr == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		try
		{
			IntPtr intPtr2 = OpenService(intPtr, lpServiceName, ServiceAccessRights.AllAccess);
			if (intPtr2 == IntPtr.Zero)
			{
				CloseServiceHandle(intPtr2);
				return;
			}
			try
			{
				ServiceStatus serviceStatus = new ServiceStatus();
				ControlService(intPtr2, ServiceControl.Stop, serviceStatus);
				int num = 0;
				while (waitForStop && num < 180 && QueryServiceStatus(intPtr2, serviceStatus) && serviceStatus.dwCurrentState != ServiceState.Stopped)
				{
					Thread.Sleep(1000);
					num++;
				}
				if (!DeleteService(intPtr2) && Marshal.GetLastWin32Error() != 1072)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
			finally
			{
				CloseServiceHandle(intPtr2);
			}
		}
		finally
		{
			CloseServiceHandle(intPtr);
		}
	}
}
