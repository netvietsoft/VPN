using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using VpnSDK.Core.Helpers;
using VpnSDK.Core.Interop;
using VpnSDK.Helpers;

namespace VpnSDK.Internal.WireGuard;

internal class WireGuardService
{
	private const string LongName = "VPN Host Service";

	private const string Description = "Host service for VPN applications to run WireGuard services.";

	private const string TunnelNamePrefix = "WireGuardTunnel$";

	private static readonly ILogger _logger = LogProvider.GetLogger("VpnSDK::WireGuardService");

	[DllImport("tunnel.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WireGuardTunnelService")]
	public static extern bool Run([MarshalAs(UnmanagedType.LPWStr)] string configFile);

	public static NamedPipeClientStream GetPipe(string configFile)
	{
		return new NamedPipeClientStream("ProtectedPrefix\\Administrators\\WireGuard\\" + Path.GetFileNameWithoutExtension(configFile));
	}

	public static Driver.Adapter GetAdapter(string configFile)
	{
		return new Driver.Adapter(Path.GetFileNameWithoutExtension(configFile));
	}

	public static (bool exists, WindowsServiceHelper.ServiceState state) GetServiceStatus(string configFile)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(configFile);
		string lpServiceName = "WireGuardTunnel$" + fileNameWithoutExtension;
		IntPtr intPtr = IntPtr.Zero;
		IntPtr intPtr2 = IntPtr.Zero;
		try
		{
			intPtr = WindowsServiceHelper.OpenSCManager(null, null, WindowsServiceHelper.ScmAccessRights.Connect);
			if (intPtr == IntPtr.Zero)
			{
				_logger?.LogWarning("Failed to open SCManager");
				return (exists: false, state: WindowsServiceHelper.ServiceState.Unknown);
			}
			intPtr2 = WindowsServiceHelper.OpenService(intPtr, lpServiceName, WindowsServiceHelper.ServiceAccessRights.QueryStatus);
			if (intPtr2 == IntPtr.Zero)
			{
				return (exists: false, state: WindowsServiceHelper.ServiceState.Unknown);
			}
			WindowsServiceHelper.ServiceStatus serviceStatus = new WindowsServiceHelper.ServiceStatus();
			if (!WindowsServiceHelper.QueryServiceStatus(intPtr2, serviceStatus))
			{
				_logger?.LogWarning("Failed to query service status");
				return (exists: true, state: WindowsServiceHelper.ServiceState.Unknown);
			}
			return (exists: true, state: serviceStatus.dwCurrentState);
		}
		catch (Exception ex)
		{
			_logger?.LogError("Error checking service status: " + ex.Message);
			return (exists: false, state: WindowsServiceHelper.ServiceState.Unknown);
		}
		finally
		{
			SafeCloseHandle(intPtr2);
			SafeCloseHandle(intPtr);
		}
	}

	public static void Add(string configFile, bool ephemeral, short serviceStartTimeOutInSeconds, bool waitForStart = true)
	{
		string systemArchitecture = Utils.GetSystemArchitecture(mapAsAmd64: true);
		if (string.IsNullOrEmpty(systemArchitecture))
		{
			throw new NotSupportedException("Unsupported CPU or OS architecture");
		}
		if (!File.Exists(configFile))
		{
			throw new FileNotFoundException("Config file don't exist at " + configFile + ".");
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(configFile);
		string text = "WireGuardTunnel$" + fileNameWithoutExtension;
		string lpDisplayName = "VPN Host Service: " + fileNameWithoutExtension;
		string text2 = Path.Combine(PathHelper.GetApplicationDirectory(), "WireGuard", systemArchitecture, "VpnHostService.exe");
		if (!File.Exists(text2))
		{
			throw new VpnHostServiceFileNotFoundException("VpnHostService file don't exist at " + text2 + ".");
		}
		string lpBinaryPathName = $"\"{text2}\" /service \"{configFile}\" {Process.GetCurrentProcess().Id}";
		IntPtr intPtr = WindowsServiceHelper.OpenSCManager(null, null, WindowsServiceHelper.ScmAccessRights.AllAccess);
		if (intPtr == IntPtr.Zero)
		{
			LogWarningAndRaiseWin32Exception("Failed to open SCManager");
		}
		try
		{
			IntPtr intPtr2 = WindowsServiceHelper.OpenService(intPtr, text, WindowsServiceHelper.ServiceAccessRights.AllAccess);
			if (intPtr2 != IntPtr.Zero)
			{
				_logger?.LogInformation("Service already exists. Removing existing service.");
				WindowsServiceHelper.CloseServiceHandle(intPtr2);
				Remove(configFile, waitForStop: true);
				_logger?.LogInformation("Existing service removed.");
			}
			intPtr2 = WindowsServiceHelper.CreateService(intPtr, text, lpDisplayName, WindowsServiceHelper.ServiceAccessRights.AllAccess, WindowsServiceHelper.ServiceType.Win32OwnProcess, WindowsServiceHelper.ServiceStartType.Demand, WindowsServiceHelper.ServiceError.Normal, lpBinaryPathName, null, IntPtr.Zero, "Nsi\0TcpIp\0", null, null);
			if (intPtr2 == IntPtr.Zero)
			{
				LogWarningAndRaiseWin32Exception("Failed to create service " + text);
			}
			try
			{
				_logger?.LogDebug("Configuring service SID type");
				WindowsServiceHelper.ServiceSidType lpInfo = WindowsServiceHelper.ServiceSidType.Unrestricted;
				if (!WindowsServiceHelper.ChangeServiceConfig2(intPtr2, WindowsServiceHelper.ServiceConfigType.SidInfo, ref lpInfo))
				{
					LogWarningAndRaiseWin32Exception("Failed to change service SID type");
				}
				_logger?.LogDebug("Setting service description");
				WindowsServiceHelper.ServiceDescription lpInfo2 = new WindowsServiceHelper.ServiceDescription
				{
					lpDescription = "Host service for VPN applications to run WireGuard services."
				};
				if (!WindowsServiceHelper.ChangeServiceConfig2(intPtr2, WindowsServiceHelper.ServiceConfigType.Description, ref lpInfo2))
				{
					LogWarningAndRaiseWin32Exception("Failed to set service description");
				}
				_logger?.LogDebug($"Starting service. The service start timeout is set to {serviceStartTimeOutInSeconds} seconds.");
				if (!WindowsServiceHelper.StartService(intPtr2, 0, null))
				{
					LogWarningAndRaiseWin32Exception("Failed to start service " + text);
				}
				WindowsServiceHelper.ServiceStatus serviceStatus = new WindowsServiceHelper.ServiceStatus();
				int num = 0;
				while (waitForStart && num < serviceStartTimeOutInSeconds && WindowsServiceHelper.QueryServiceStatus(intPtr2, serviceStatus) && serviceStatus.dwCurrentState != WindowsServiceHelper.ServiceState.Running)
				{
					_logger?.LogWarning($"Waiting for service to start. Attempt: {num + 1}");
					Thread.Sleep(1000);
					num++;
				}
				if (serviceStatus.dwCurrentState != WindowsServiceHelper.ServiceState.Running)
				{
					_logger?.LogWarning($"Service failed to start within the expected time. The service status is : {serviceStatus.dwCurrentState}");
					throw new VpnHostServiceException("Failed to start VpnHostService.Please check Event Viewer for more info.");
				}
				_logger?.LogInformation("WireGuard service started.");
				if (ephemeral && !WindowsServiceHelper.DeleteService(intPtr2))
				{
					LogWarningAndRaiseWin32Exception("Failed to delete ephemeral service " + text);
				}
			}
			finally
			{
				WindowsServiceHelper.CloseServiceHandle(intPtr2);
				_logger?.LogDebug("Closed service handle");
			}
		}
		finally
		{
			WindowsServiceHelper.CloseServiceHandle(intPtr);
			_logger?.LogDebug("Closed SCManager handle");
		}
	}

	public static void Remove(string configFile, bool waitForStop)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(configFile);
		string text = "WireGuardTunnel$" + fileNameWithoutExtension;
		_logger?.LogInformation("Attempting to remove service: " + text);
		IntPtr intPtr = WindowsServiceHelper.OpenSCManager(null, null, WindowsServiceHelper.ScmAccessRights.AllAccess);
		if (intPtr == IntPtr.Zero)
		{
			LogWarningAndRaiseWin32Exception("Failed to open Service Control Manager");
		}
		try
		{
			IntPtr intPtr2 = WindowsServiceHelper.OpenService(intPtr, text, WindowsServiceHelper.ServiceAccessRights.AllAccess);
			if (intPtr2 == IntPtr.Zero)
			{
				_logger?.LogWarning("Service " + text + " not found or cannot be opened");
				return;
			}
			try
			{
				_logger?.LogInformation("Attempting to stop service: " + text);
				WindowsServiceHelper.ServiceStatus serviceStatus = new WindowsServiceHelper.ServiceStatus();
				WindowsServiceHelper.ControlService(intPtr2, WindowsServiceHelper.ServiceControl.Stop, serviceStatus);
				int num = 0;
				while (waitForStop && num < 180 && WindowsServiceHelper.QueryServiceStatus(intPtr2, serviceStatus) && serviceStatus.dwCurrentState != WindowsServiceHelper.ServiceState.Stopped)
				{
					Thread.Sleep(1000);
					num++;
				}
				_logger.LogInformation($"Attempting to delete service: {text}, Final State: {serviceStatus.dwCurrentState}");
				if (!WindowsServiceHelper.DeleteService(intPtr2) && Marshal.GetLastWin32Error() != 1072)
				{
					LogWarningAndRaiseWin32Exception("Error while deleting service: " + text);
				}
			}
			finally
			{
				WindowsServiceHelper.CloseServiceHandle(intPtr2);
			}
		}
		finally
		{
			WindowsServiceHelper.CloseServiceHandle(intPtr);
		}
	}

	public static bool IsServiceRunning(string configFile)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(configFile);
		string lpServiceName = "WireGuardTunnel$" + fileNameWithoutExtension;
		IntPtr intPtr = WindowsServiceHelper.OpenSCManager(null, null, WindowsServiceHelper.ScmAccessRights.AllAccess);
		if (intPtr == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		try
		{
			IntPtr intPtr2 = WindowsServiceHelper.OpenService(intPtr, lpServiceName, WindowsServiceHelper.ServiceAccessRights.AllAccess);
			if (intPtr2 == IntPtr.Zero)
			{
				WindowsServiceHelper.CloseServiceHandle(intPtr2);
				return false;
			}
			try
			{
				WindowsServiceHelper.ServiceStatus serviceStatus = new WindowsServiceHelper.ServiceStatus();
				return WindowsServiceHelper.QueryServiceStatus(intPtr2, serviceStatus) && serviceStatus.dwCurrentState == WindowsServiceHelper.ServiceState.Running;
			}
			finally
			{
				WindowsServiceHelper.CloseServiceHandle(intPtr2);
			}
		}
		finally
		{
			WindowsServiceHelper.CloseServiceHandle(intPtr);
		}
	}

	private static void LogWarningAndRaiseWin32Exception(string message)
	{
		int lastWin32Error = Marshal.GetLastWin32Error();
		_logger?.LogWarning($"{message}. Error: 0x{lastWin32Error:X}");
		throw new Win32Exception(lastWin32Error);
	}

	private static void SafeCloseHandle(IntPtr handle)
	{
		if (handle != IntPtr.Zero)
		{
			try
			{
				WindowsServiceHelper.CloseServiceHandle(handle);
			}
			catch (Exception ex)
			{
				_logger?.LogWarning("Error closing handle: " + ex.Message);
			}
		}
	}
}
