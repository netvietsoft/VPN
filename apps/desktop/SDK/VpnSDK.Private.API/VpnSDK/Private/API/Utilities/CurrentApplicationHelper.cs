using System;
using System.Diagnostics;
using System.IO;
using VpnSDK.Common.Helpers;

namespace VpnSDK.Private.API.Utilities;

internal class CurrentApplicationHelper
{
	internal static string GetName()
	{
		return Path.GetFileNameWithoutExtension(PathHelper.GetApplicationFilePath());
	}

	internal static string GetVersion()
	{
		string result = "0.0.0.0";
		string applicationFilePath = PathHelper.GetApplicationFilePath();
		try
		{
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(applicationFilePath);
			if (versionInfo != null && versionInfo.FileVersion != null)
			{
				result = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart).ToString();
			}
		}
		catch
		{
		}
		return result;
	}
}
