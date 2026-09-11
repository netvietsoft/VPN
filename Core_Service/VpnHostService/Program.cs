using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace VpnHostService;

internal class Program
{
	[DllImport("tunnel.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WireGuardTunnelService")]
	public static extern bool Run([MarshalAs(UnmanagedType.LPWStr)] string configFile);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool SetDllDirectory(string path);

	[STAThread]
	public static void Main(string[] args)
	{
		if (args.Length != 3 || !(args[0] == "/service"))
		{
			return;
		}
		new Thread((ThreadStart)delegate
		{
			try
			{
				Process.GetProcessById(int.Parse(args[2])).WaitForExit();
				WindowsService.Remove(args[1], waitForStop: false);
			}
			catch
			{
			}
		}).Start();
		Run(args[1]);
	}
}
