Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

# Win32 API functions for window state
Add-Type @"
using System;
using System.Runtime.InteropServices;

public class Win32 {
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    public const uint WM_CLOSE = 0x0010;
    public const uint WM_SYSCOMMAND = 0x0112;
    public const int SC_CLOSE = 0xF060;
}
"@

Write-Host "=== TEST 1: Launch NextAiVPN from Installed Path ==="
$appPath = "C:\Users\boluc\AppData\Local\Programs\NextAiTechnology\NextAiVPN\NextAiVPN.Desktop.exe"

# Stop any previous instances
Get-Process -Name "NextAiVPN.Desktop" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

$proc = Start-Process -FilePath $appPath -PassThru
Write-Host "Process started with PID: $($proc.Id)"

# Wait for window to initialize
Start-Sleep -Seconds 5

$proc.Refresh()
Write-Host "MainWindowHandle: $($proc.MainWindowHandle)"
Write-Host "MainWindowTitle: $($proc.MainWindowTitle)"
Write-Host "IsWindowVisible: $([Win32]::IsWindowVisible($proc.MainWindowHandle))"

# Check UI Automation Element
$root = [System.Windows.Automation.AutomationElement]::RootElement
$appCond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $proc.Id)
$appWindow = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $appCond)

if ($appWindow) {
    Write-Host "Automation Window found: $($appWindow.Current.Name), Class: $($appWindow.Current.ClassName)"
} else {
    Write-Host "Automation Window not immediately found in root children, checking all windows..."
}

Write-Host "`n=== TEST 2: Test [X] Click (Close to Tray) ==="
Write-Host "Sending WM_SYSCOMMAND (SC_CLOSE) / WM_CLOSE to window handle $($proc.MainWindowHandle)..."
[Win32]::PostMessage($proc.MainWindowHandle, [Win32]::WM_SYSCOMMAND, [IntPtr][Win32]::SC_CLOSE, [IntPtr]::Zero)

Start-Sleep -Seconds 2
$proc.Refresh()

Write-Host "After clicking [X]:"
Write-Host "HasExited: $($proc.HasExited)"
Write-Host "MainWindowHandle: $($proc.MainWindowHandle)"
$isVisibleAfterClose = [Win32]::IsWindowVisible($proc.MainWindowHandle)
Write-Host "IsWindowVisible: $isVisibleAfterClose"

if (-not $proc.HasExited) {
    Write-Host "SUCCESS: Process is STILL RUNNING IN BACKGROUND! (Did not exit)"
} else {
    Write-Host "FAILED: Process exited!"
}

Write-Host "`n=== TEST 3: Restore Window (Simulate Tray Restore) ==="
# In TaskBarIconService, ShowWindow does:
# window.Show(); window.Visibility = Visible; window.WindowState = Normal;
# Via Win32 ShowWindow(hWnd, SW_RESTORE / SW_SHOW) = 9
[Win32]::ShowWindow($proc.MainWindowHandle, 9)
Start-Sleep -Seconds 1
$proc.Refresh()
Write-Host "Restored Window Visible: $([Win32]::IsWindowVisible($proc.MainWindowHandle))"

Write-Host "`nTest completed successfully."
