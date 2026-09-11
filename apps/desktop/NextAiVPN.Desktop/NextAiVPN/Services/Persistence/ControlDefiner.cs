using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace NextAiVPN.Services.Persistence;

internal static class ControlDefiner
{
	[DllImport("user32.dll")]
	private static extern nint WindowFromPoint(Point p);

	public static bool IsNextAiVPNControl(Point point)
	{
		nint hwnd = WindowFromPoint(point);
		try
		{
			if (HwndSource.FromHwnd(hwnd) == null)
			{
				return false;
			}
		}
		catch (Exception exception)
		{
			Utils.Logger.Error(exception, "IsNextAiVPNControl", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\ControlDefiner.cs", 33);
		}
		return true;
	}
}
