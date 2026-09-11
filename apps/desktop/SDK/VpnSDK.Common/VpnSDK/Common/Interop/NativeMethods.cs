using System.Runtime.InteropServices;
using System.Text;

namespace VpnSDK.Common.Interop;

internal class NativeMethods
{
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static extern uint GetModuleFileName(nint hModule, StringBuilder lpFilename, int nSize);
}
