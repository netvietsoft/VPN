using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace VpnSDK.Private.OpenVpn.Helpers;

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
				GetModuleFileName(IntPtr.Zero, stringBuilder, 1024);
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

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

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
				if (!string.IsNullOrEmpty(text) && text.EndsWith(".exe"))
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
