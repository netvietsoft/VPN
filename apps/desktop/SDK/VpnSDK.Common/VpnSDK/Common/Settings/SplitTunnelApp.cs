using System;
using System.IO;

namespace VpnSDK.Common.Settings;

public class SplitTunnelApp
{
	public string Name { get; private set; }

	public string Path { get; private set; }

	public SplitTunnelApp(string name, string path)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException("Application name cannot be null or empty.");
		}
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("Application path cannot be null or empty.");
		}
		if (!IsValidPath(path))
		{
			throw new ArgumentException("Application path is not a valid file path.");
		}
		Name = name;
		Path = path;
	}

	private static bool HasWildCards(string filePath)
	{
		return filePath.IndexOf("*") > -1;
	}

	private static bool IsValidPath(string filePath)
	{
		bool result = false;
		if (HasWildCards(filePath))
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(System.IO.Path.GetDirectoryName(filePath));
			if (directoryInfo.Exists)
			{
				string fileName = System.IO.Path.GetFileName(filePath);
				result = directoryInfo.GetFiles(fileName).Length != 0;
			}
		}
		else
		{
			result = File.Exists(filePath);
		}
		return result;
	}
}
