using System.Runtime.InteropServices;
using System.Windows;

namespace NextAiVPN.Services.Persistence;

internal static class CursorPosition
{
	public struct PointInter
	{
		public int X;

		public int Y;

		public static explicit operator Point(PointInter point)
		{
			return new Point(point.X, point.Y);
		}
	}

	[DllImport("user32.dll")]
	public static extern bool GetCursorPos(out PointInter lpPoint);

	public static Point GetCursorPosition()
	{
		GetCursorPos(out var lpPoint);
		return (Point)lpPoint;
	}
}
