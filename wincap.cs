using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

public class WinCap {
    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public static void Main(string[] args) {
        uint pid = 0;
        if (args.Length > 0) uint.TryParse(args[0], out pid);
        string outDir = @"E:\DECOMPILER\Soft\VPN\CONVERT\Report\screenshots";
        Directory.CreateDirectory(outDir);

        List<IntPtr> windows = new List<IntPtr>();
        EnumWindows((hWnd, lParam) => {
            uint wPid;
            GetWindowThreadProcessId(hWnd, out wPid);
            if ((pid == 0 || wPid == pid) && IsWindowVisible(hWnd)) {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, 256);
                string title = sb.ToString();
                RECT r;
                GetWindowRect(hWnd, out r);
                int w = r.Right - r.Left;
                int h = r.Bottom - r.Top;
                if (w > 200 && h > 200) {
                    Console.WriteLine("Window found: " + hWnd + " Title: '" + title + "' (" + w + "x" + h + ") PID: " + wPid);
                    windows.Add(hWnd);
                }
            }
            return true;
        }, IntPtr.Zero);

        int idx = 0;
        foreach (var hWnd in windows) {
            idx++;
            ShowWindow(hWnd, 9);
            SetForegroundWindow(hWnd);
            System.Threading.Thread.Sleep(300);
            RECT rect;
            GetWindowRect(hWnd, out rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            if (width > 0 && height > 0) {
                using (Bitmap bmp = new Bitmap(width, height)) {
                    using (Graphics g = Graphics.FromImage(bmp)) {
                        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
                    }
                    string savePath = Path.Combine(outDir, "wpf_window_" + idx + ".png");
                    bmp.Save(savePath, ImageFormat.Png);
                    Console.WriteLine("Saved: " + savePath);
                }
            }
        }
    }
}
