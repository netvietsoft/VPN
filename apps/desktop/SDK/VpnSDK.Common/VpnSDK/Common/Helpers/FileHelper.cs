using System;
using System.Diagnostics;
using System.IO;

namespace VpnSDK.Common.Helpers;

public static class FileHelper
{
	public static Version GetVersion(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException("File was not found.", filePath);
		}
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);
		return new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
	}
}
