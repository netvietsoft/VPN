using System;

namespace NextAiVPN.Services.Persistence;

public static class OSDetector
{
	public static int GetOSVersion()
	{
		return Environment.OSVersion.Version.Major;
	}
}
