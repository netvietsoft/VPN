using System;
using Microsoft.Win32;

namespace VpnSDK.Helpers;

public class VersionHelper
{
	public static int GetNetFrameworkVersion()
	{
		try
		{
			using RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\");
			if (registryKey != null && registryKey.GetValue("Release") != null)
			{
				return (int)registryKey.GetValue("Release");
			}
		}
		catch
		{
		}
		return 0;
	}

	public static void EnableTls12IfNeeded()
	{
		try
		{
			string text = "SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\SCHANNEL\\Protocols\\TLS1.2\\Client";
			if (Environment.OSVersion.Version.Major != 6 || Environment.OSVersion.Version.Minor != 1)
			{
				return;
			}
			RegistryView view = (Environment.Is64BitProcess ? RegistryView.Registry64 : RegistryView.Registry32);
			using RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
			RegistryKey registryKey2 = registryKey.OpenSubKey(text, writable: true);
			if (registryKey2 == null)
			{
				registryKey2 = registryKey.CreateSubKey(text);
			}
			registryKey2.SetValue("DisabledByDefault", 0, RegistryValueKind.DWord);
		}
		catch
		{
		}
	}
}
