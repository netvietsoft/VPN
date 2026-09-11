using System;
using System.Runtime.InteropServices;

namespace VpnSDK.Core.Helpers;

internal class Utils
{
	public const string Arm = "arm";

	public const string Arm64 = "arm64";

	public const string X86 = "x86";

	public const string X64 = "x64";

	public const string Amd64 = "amd64";

	internal static string GetProcessArchitecture(bool mapAsX64 = false)
	{
		Architecture processArchitecture = RuntimeInformation.ProcessArchitecture;
		string result = string.Empty;
		switch (processArchitecture)
		{
		case Architecture.Arm:
		case Architecture.Arm64:
			result = (Environment.Is64BitProcess ? "arm64" : string.Empty);
			break;
		case Architecture.X86:
		case Architecture.X64:
			result = ((!Environment.Is64BitProcess) ? "x86" : (mapAsX64 ? "x64" : "amd64"));
			break;
		}
		return result;
	}

	internal static string GetSystemArchitecture(bool mapAsAmd64 = false)
	{
		if (!((RuntimeInformation.OSArchitecture == Architecture.X64) & mapAsAmd64))
		{
			return $"{RuntimeInformation.OSArchitecture}".ToLower();
		}
		return "amd64";
	}
}
