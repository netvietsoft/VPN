using System;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NextAiVPN;

public static class SystemInfo
{
	public static string AppName { get; }

	public static string AppVersion { get; }

	public static string OSName { get; }

	public static string OSArchitecture { get; }

	public static string Framework { get; }

	public static string MachineId { get; }

	public static string UserAgent { get; }

	public static string MachineName { get; }

	public static string ReleaseStage { get; }

	static SystemInfo()
	{
		AppName = "NextAiVPN";
		MachineName = Environment.MachineName;
		AppVersion = GetAppVersion();
		OSName = RuntimeInformation.OSDescription;
		OSArchitecture = RuntimeInformation.OSArchitecture.ToString();
		Framework = RuntimeInformation.FrameworkDescription;
		MachineId = GetMachineId();
		UserAgent = AppVersion + " " + OSName;
		ReleaseStage = "production";
	}

	public static string AsString()
	{
		return $"{AppName} v{AppVersion} | {OSName} ({OSArchitecture}) | {Framework} | MachineId: {MachineId}";
	}

	private static string GetAppVersion()
	{
		return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
	}

	private static string GetMachineId()
	{
		try
		{
			object value = null;
			using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
			{
				using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
				using ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectCollection.GetEnumerator();
				if (managementObjectEnumerator.MoveNext())
				{
					ManagementObject managementObject = (ManagementObject)managementObjectEnumerator.Current;
					using (managementObject)
					{
						value = managementObject.GetPropertyValue("ProcessorId");
					}
				}
			}
			return $"{Environment.MachineName}/{value}";
		}
		catch
		{
			return "unknown";
		}
	}
}
