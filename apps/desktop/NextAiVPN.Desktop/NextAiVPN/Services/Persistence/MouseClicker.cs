using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NextAiVPN.Services.Persistence;

internal static class MouseClicker
{
	private const int LEFTDOWN_PRESS = 2;

	private const int MLEFTUP_PRESS = 4;

	private const int RIGHTDOWN_PRESS = 8;

	private const int RIGHTUP_PRESS = 16;

	[DllImport("User32.Dll")]
	public static extern long SetCursorPos(int x, int y);

	[DllImport("user32.dll")]
	public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

	private static void PressRight(int x, int y)
	{
		mouse_event(8, x, y, 0, 0);
		mouse_event(16, x, y, 0, 0);
	}

	private static void PressLeft(int x, int y)
	{
		mouse_event(2, x, y, 0, 0);
		mouse_event(4, x, y, 0, 0);
	}

	public static void RightClick()
	{
		Point mousePosition = Control.MousePosition;
		SetCursorPos(mousePosition.X, mousePosition.Y);
		PressRight(mousePosition.X, mousePosition.Y);
	}

	public static void LeftClick()
	{
		Point mousePosition = Control.MousePosition;
		SetCursorPos(mousePosition.X, mousePosition.Y);
		PressRight(mousePosition.X, mousePosition.Y);
	}
}
