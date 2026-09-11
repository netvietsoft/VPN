using System;
using System.IO;
using System.Linq;
using System.Text;
using VpnSDK.Common.Interop;

namespace VpnSDK.Common.Helpers;

public static class PathHelper
{
	private const int MaxPath = 1024;

	private static readonly Func<string?>[] PathFindingFuncs = new Func<string>[3]
	{
		delegate
		{
			string result = null;
			try
			{
				StringBuilder stringBuilder = new StringBuilder(1024);
				NativeMethods.GetModuleFileName(IntPtr.Zero, stringBuilder, 1024);
				result = stringBuilder.ToString();
			}
			catch
			{
			}
			return result;
		},
		() => Environment.ProcessPath,
		delegate
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			string? text = commandLineArgs.FirstOrDefault();
			if (text != null && text.Contains(".exe"))
			{
				return commandLineArgs.First();
			}
			throw new Exception();
		}
	};

	public static string GetApplicationDirectory()
	{
		string text = Path.GetDirectoryName(Path.GetFullPath(GetApplicationFilePath()));
		if (text.Last() != Path.DirectorySeparatorChar)
		{
			text += Path.DirectorySeparatorChar;
		}
		return text;
	}

	public static string GetApplicationFilePath()
	{
		Func<string>[] pathFindingFuncs = PathFindingFuncs;
		foreach (Func<string> func in pathFindingFuncs)
		{
			try
			{
				string text = func();
				if (text != null && text.EndsWith(".exe"))
				{
					return text.TrimEnd('\\');
				}
			}
			catch
			{
			}
		}
		return string.Empty;
	}
}
