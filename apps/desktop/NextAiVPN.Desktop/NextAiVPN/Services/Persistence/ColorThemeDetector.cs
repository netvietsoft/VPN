using System;
using Microsoft.Win32;

namespace NextAiVPN.Services.Persistence;

internal class ColorThemeDetector : IColorThemeDetector
{
	private const string Key = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";

	private const string AppsUseLightTheme = "AppsUseLightTheme";

	private const string SystemUsesLightTheme = "SystemUsesLightTheme";

	public Style GetAppCurrentTheme()
	{
		return StyleModeDefiner.DefineAppStyle();
	}

	public Style GetSystemCurrentTheme()
	{
		return GetThemeIndicator(GetRegistryValue("SystemUsesLightTheme"));
	}

	private static Style GetThemeIndicator(string registryValue)
	{
		if (OSDetector.GetOSVersion() < 10)
		{
			return Style.Light;
		}
		if (!registryValue.Equals("0"))
		{
			return Style.Light;
		}
		return Style.Dark;
	}

	private static string GetRegistryValue(string variableName)
	{
		return GetRegistryKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", variableName);
	}

	private static string GetRegistryKey(string keyPath, string variableName, bool writable = false)
	{
		try
		{
			RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
			return (string.IsNullOrEmpty(keyPath) ? registryKey : registryKey.OpenSubKey(keyPath, writable)).GetValue(variableName).ToString();
		}
		catch (Exception)
		{
			return "1";
		}
	}
}
