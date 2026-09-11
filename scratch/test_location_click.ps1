Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

Add-Type @"
using System;
using System.Runtime.InteropServices;

public class Win32Helper {
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    public const uint MOUSEEVENTF_LEFTUP = 0x0004;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
"@

$proc = Get-Process -Name "NextAiVPN.Desktop" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) {
    Write-Host "Process NextAiVPN.Desktop not found!"
    exit 1
}

[Win32Helper]::ShowWindow($proc.MainWindowHandle, 9) # Restore
[Win32Helper]::SetForegroundWindow($proc.MainWindowHandle)
Start-Sleep -Seconds 1

$root = [System.Windows.Automation.AutomationElement]::RootElement
$appCond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $proc.Id)
$appWindow = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $appCond)

if (-not $appWindow) {
    Write-Host "Automation element not found!"
    exit 1
}

Write-Host "Window title: $($appWindow.Current.Name)"

# Find all ListItems in the UI
$itemCond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::ListItem)
$items = $appWindow.FindAll([System.Windows.Automation.TreeScope]::Descendants, $itemCond)
Write-Host "Found $($items.Count) ListItems in UI."

# Let's inspect each item's bounds and click it
for ($i = 0; $i -lt [Math]::Min(5, $items.Count); $i++) {
    $item = $items[$i]
    $rect = $item.Current.BoundingRectangle
    Write-Host "ListItem $i : Bounds=($($rect.X), $($rect.Y), $($rect.Width), $($rect.Height))"
}

if ($items.Count -gt 0) {
    $targetItem = $items[0]
    $rect = $targetItem.Current.BoundingRectangle
    $clickX = [int]($rect.X + 100)
    $clickY = [int]($rect.Y + 20)
    Write-Host "Simulating mouse click at ($clickX, $clickY)..."
    [Win32Helper]::SetCursorPos($clickX, $clickY)
    Start-Sleep -Milliseconds 100
    [Win32Helper]::mouse_event([Win32Helper]::MOUSEEVENTF_LEFTDOWN, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 50
    [Win32Helper]::mouse_event([Win32Helper]::MOUSEEVENTF_LEFTUP, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Seconds 1
    Write-Host "Mouse click sent!"
}

# Capture screenshot of the entire primary screen to verify the window visual appearance
$screenBounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bmp = New-Object System.Drawing.Bitmap($screenBounds.Width, $screenBounds.Height)
$graphics = [System.Drawing.Graphics]::FromImage($bmp)
$graphics.CopyFromScreen(0, 0, 0, 0, $screenBounds.Size)
$savePath = "C:\Users\boluc\.gemini\antigravity-ide\brain\9591d6b0-70be-4666-b562-c62cde92482a\desktop_ui_after_click.png"
$bmp.Save($savePath, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bmp.Dispose()
Write-Host "Screenshot saved to $savePath"
