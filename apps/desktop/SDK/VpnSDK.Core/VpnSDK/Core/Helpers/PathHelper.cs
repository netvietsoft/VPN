using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VpnSDK.Core.Interop;

namespace VpnSDK.Core.Helpers;

internal static class PathHelper
{
	private const int MaxPath = 1024;

	private static readonly Func<string>[] PathFindingFuncs = new Func<string>[3]
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
		() => Process.GetCurrentProcess().MainModule.FileName,
		delegate
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			if (commandLineArgs.FirstOrDefault().Contains(".exe"))
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
			string text2 = text;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			text = text2 + directorySeparatorChar;
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
				if (!string.IsNullOrEmpty(text) && text.EndsWith(".exe"))
				{
					return text.TrimEnd(new char[1] { '\\' });
				}
			}
			catch
			{
			}
		}
		return string.Empty;
	}
}
