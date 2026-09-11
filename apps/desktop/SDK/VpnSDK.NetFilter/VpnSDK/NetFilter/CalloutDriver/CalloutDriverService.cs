using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using VpnSDK.Common.Helpers;
using VpnSDK.Core.Helpers;
using VpnSDK.Core.Interop;

namespace VpnSDK.NetFilter.CalloutDriver;

internal class CalloutDriverService : ICalloutDriverService
{
	private readonly string _serviceName;

	private static readonly string _sysFileName = "netfilter.sys";

	private static readonly string _architecture = Utils.GetSystemArchitecture();

	private readonly string _driverPath = Path.Combine(VpnSDK.Core.Helpers.PathHelper.GetApplicationDirectory(), "Drivers", _architecture, _sysFileName);

	private readonly ILogger _logger;

	private bool _isStarted;

	public Version DriverVersion => FileHelper.GetVersion(_driverPath);

	public CalloutDriverService(string serviceName, ILogger logger)
	{
		_logger = logger;
		_serviceName = serviceName;
	}

	~CalloutDriverService()
	{
		if (_isStarted)
		{
			Stop(waitForStop: true);
		}
	}

	public bool Start(bool waitForStart = true)
	{
		IntPtr intPtr = WindowsServiceHelper.OpenSCManager(null, null, WindowsServiceHelper.ScmAccessRights.AllAccess);
		if (intPtr == IntPtr.Zero)
		{
			_logger?.LogError($"Failed to open SCManager. Error: <{Marshal.GetLastWin32Error()}>");
			return false;
		}
		try
		{
			if (!Validate())
			{
				Remove(waitForStart);
				CreateDriverService(intPtr);
			}
			IntPtr service = WindowsServiceHelper.OpenService(intPtr, _serviceName, WindowsServiceHelper.ServiceAccessRights.AllAccess);
			if (service == IntPtr.Zero)
			{
				service = CreateDriverService(intPtr);
			}
			try
			{
				if (!WindowsServiceHelper.StartService(service, 0, null) && Marshal.GetLastWin32Error() != 1056)
				{
					throw new Exception($"Callout driver failed to start. Error: <{Marshal.GetLastWin32Error()}>");
				}
				WindowsServiceHelper.ServiceStatus serviceStatus = new WindowsServiceHelper.ServiceStatus();
				WaitUntil(() => waitForStart && WindowsServiceHelper.QueryServiceStatus(service, serviceStatus) && serviceStatus.dwCurrentState != WindowsServiceHelper.ServiceState.Running, 10);
				if (serviceStatus.dwCurrentState == WindowsServiceHelper.ServiceState.Running)
				{
					_isStarted = true;
				}
			}
			finally
			{
				WindowsServiceHelper.CloseServiceHandle(service);
			}
		}
		finally
		{
			WindowsServiceHelper.CloseServiceHandle(intPtr);
		}
		return _isStarted;
	}

	public void Remove(bool waitForStop)
	{
		IntPtr intPtr = WindowsServiceHelper.OpenSCManager(null, null, WindowsServiceHelper.ScmAccessRights.AllAccess);
		if (intPtr == IntPtr.Zero)
		{
			_logger?.LogError($"Failed to open SCManager. Error: <{Marshal.GetLastWin32Error()}>");
			return;
		}
		try
		{
			IntPtr service = WindowsServiceHelper.OpenService(intPtr, _serviceName, WindowsServiceHelper.ServiceAccessRights.AllAccess);
			if (service == IntPtr.Zero)
			{
				WindowsServiceHelper.CloseServiceHandle(service);
				return;
			}
			try
			{
				WindowsServiceHelper.ServiceStatus serviceStatus = new WindowsServiceHelper.ServiceStatus();
				WindowsServiceHelper.ControlService(service, WindowsServiceHelper.ServiceControl.Stop, serviceStatus);
				WaitUntil(() => waitForStop && WindowsServiceHelper.QueryServiceStatus(service, serviceStatus) && serviceStatus.dwCurrentState != WindowsServiceHelper.ServiceState.Stopped, 10);
				if (!WindowsServiceHelper.DeleteService(service) && Marshal.GetLastWin32Error() != 1072)
				{
					throw new Exception($"Callout driver removal failed. Error: <{Marshal.GetLastWin32Error()}>");
				}
			}
			finally
			{
				WindowsServiceHelper.CloseServiceHandle(service);
			}
		}
		finally
		{
			WindowsServiceHelper.CloseServiceHandle(intPtr);
		}
	}

	public void Stop(bool waitForStop)
	{
		IntPtr intPtr = WindowsServiceHelper.OpenSCManager(null, null, WindowsServiceHelper.ScmAccessRights.AllAccess);
		if (intPtr == IntPtr.Zero)
		{
			_logger?.LogError($"Failed to open SCManager. Error: <{Marshal.GetLastWin32Error()}>");
			return;
		}
		try
		{
			IntPtr service = WindowsServiceHelper.OpenService(intPtr, _serviceName, WindowsServiceHelper.ServiceAccessRights.AllAccess);
			if (service == IntPtr.Zero)
			{
				WindowsServiceHelper.CloseServiceHandle(service);
				return;
			}
			try
			{
				WindowsServiceHelper.ServiceStatus serviceStatus = new WindowsServiceHelper.ServiceStatus();
				WindowsServiceHelper.ControlService(service, WindowsServiceHelper.ServiceControl.Stop, serviceStatus);
				WaitUntil(() => waitForStop && WindowsServiceHelper.QueryServiceStatus(service, serviceStatus) && serviceStatus.dwCurrentState != WindowsServiceHelper.ServiceState.Stopped, 10);
			}
			finally
			{
				WindowsServiceHelper.CloseServiceHandle(service);
			}
		}
		finally
		{
			WindowsServiceHelper.CloseServiceHandle(intPtr);
		}
	}

	public bool IsServiceRunning()
	{
		IntPtr intPtr = WindowsServiceHelper.OpenSCManager(null, null, WindowsServiceHelper.ScmAccessRights.AllAccess);
		if (intPtr == IntPtr.Zero)
		{
			_logger?.LogError($"Failed to open SCManager. Error: <{Marshal.GetLastWin32Error()}>");
			return false;
		}
		try
		{
			IntPtr intPtr2 = WindowsServiceHelper.OpenService(intPtr, _serviceName, WindowsServiceHelper.ServiceAccessRights.AllAccess);
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

	private IntPtr CreateDriverService(IntPtr scm)
	{
		IntPtr intPtr = WindowsServiceHelper.CreateService(scm, _serviceName, _serviceName, WindowsServiceHelper.ServiceAccessRights.AllAccess, WindowsServiceHelper.ServiceType.KernelDriver, WindowsServiceHelper.ServiceStartType.Demand, WindowsServiceHelper.ServiceError.Normal, _driverPath, "PNP_TDI", IntPtr.Zero, null, null, null);
		if (intPtr == IntPtr.Zero)
		{
			throw new Exception($"Failed to create callout service. Error: <{Marshal.GetLastWin32Error()}>");
		}
		if (!WindowsServiceHelper.ChangeServiceConfig(intPtr, WindowsServiceHelper.ServiceType.NoChange, WindowsServiceHelper.ServiceStartType.NoChange, WindowsServiceHelper.ServiceError.NoChange, _driverPath, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
		{
			throw new Exception($"Failed to change callout service config. Error: <{Marshal.GetLastWin32Error()}>");
		}
		return intPtr;
	}

	private bool Validate()
	{
		try
		{
			using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" + _serviceName, writable: false);
			if (registryKey != null)
			{
				string text = registryKey.GetValue("ImagePath")?.ToString();
				if (!string.IsNullOrEmpty(text) && text.Contains(_driverPath))
				{
					return true;
				}
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	private void WaitUntil(Func<bool> condition, int maxRetryCount)
	{
		int num = 0;
		while (num < maxRetryCount && condition())
		{
			num++;
			Thread.Sleep(1000);
		}
	}
}
