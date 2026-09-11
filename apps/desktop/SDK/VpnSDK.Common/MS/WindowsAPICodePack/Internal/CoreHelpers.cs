using System;

namespace MS.WindowsAPICodePack.Internal;

internal static class CoreHelpers
{
	public static bool RunningOnXP
	{
		get
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				return Environment.OSVersion.Version.Major >= 5;
			}
			return false;
		}
	}

	public static bool RunningOnVista => Environment.OSVersion.Version.Major >= 6;

	public static bool RunningOnWin7
	{
		get
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				return Environment.OSVersion.Version.CompareTo(new Version(6, 1)) >= 0;
			}
			return false;
		}
	}

	public static void ThrowIfNotXP()
	{
		if (!RunningOnXP)
		{
			throw new PlatformNotSupportedException("Only supported on Windows XP or newer.");
		}
	}

	public static void ThrowIfNotVista()
	{
		if (!RunningOnVista)
		{
			throw new PlatformNotSupportedException("Only supported on Windows Vista or newer.");
		}
	}

	public static void ThrowIfNotWin7()
	{
		if (!RunningOnWin7)
		{
			throw new PlatformNotSupportedException("Only supported on Windows 7 or newer.");
		}
	}
}
