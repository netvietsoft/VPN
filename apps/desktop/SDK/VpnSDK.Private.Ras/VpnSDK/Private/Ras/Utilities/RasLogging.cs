using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;

namespace VpnSDK.Private.Ras.Utilities;

public static class RasLogging
{
	private static readonly string DEFAULT_LOG_LOCATION = "%windir%\\tracing";

	private static readonly string[] RAS_COMPONENT_NAMES = new string[10] { "RASAPI32", "RASMANCS", "RASTAPI", "RASPLAP", "RASMAN", "RASIPHLP", "RASEAP", "IpHlpSvc", "VPNIKE", "RASLSA" };

	private static readonly string ProcessName = Process.GetCurrentProcess().ProcessName;

	public static string Directory { get; set; } = DEFAULT_LOG_LOCATION;

	public static bool Enabled
	{
		get
		{
			return Convert.ToBoolean(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Tracing\\RASEAP", "Active", false));
		}
		set
		{
			foreach (string item in RAS_COMPONENT_NAMES.Concat(RAS_COMPONENT_NAMES.Select((string x) => ProcessName + "_" + x)))
			{
				if (value)
				{
					string keyName = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Tracing\\" + item;
					Registry.SetValue(keyName, "EnableFileTracing", 1, RegistryValueKind.DWord);
					Registry.SetValue(keyName, "FileTracingMask", -65536, RegistryValueKind.DWord);
					Registry.SetValue(keyName, "MaxFileSize", 1048576, RegistryValueKind.DWord);
					Registry.SetValue(keyName, "FileDirectory", Directory, RegistryValueKind.ExpandString);
				}
				else
				{
					Registry.LocalMachine.DeleteSubKeyTree("SOFTWARE\\Microsoft\\Tracing\\" + item, throwOnMissingSubKey: false);
				}
			}
		}
	}
}
