using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VpnSDK.Core.Interop;

internal class NativeMethods
{
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern bool SetDllDirectory(string lpPathName);
}
