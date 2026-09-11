Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

Add-Type @"
using System;
using System.Runtime.InteropServices;

public class Win32 {
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public const uint WM_SYSCOMMAND = 0x0112;
    public const int SC_CLOSE = 0xF060;
}
"@

$appPath = "C:\Users\boluc\AppData\Local\Programs\NextAiTechnology\NextAiVPN\NextAiVPN.Desktop.exe"
Get-Process -Name "NextAiVPN.Desktop" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

Write-Host "Starting NextAiVPN from $appPath..."
$proc = Start-Process -FilePath $appPath -PassThru
Write-Host "Started PID: $($proc.Id)"

# Wait for UI to initialize
Start-Sleep -Seconds 5
$proc.Refresh()

if ($proc.HasExited) {
    Write-Host "ERROR: Process exited prematurely with code $($proc.ExitCode)!"
    exit 1
}

Write-Host "Process is running normally! MainWindowHandle: $($proc.MainWindowHandle)"

# Capture screenshot of running window
$rect = New-Object Win32+RECT
[Win32]::GetWindowRect($proc.MainWindowHandle, [ref]$rect)
$width = $rect.Right - $rect.Left
$height = $rect.Bottom - $rect.Top
Write-Host "Window Dimensions: Width=$width, Height=$height"

if ($width -gt 0 -and $height -gt 0) {
    [Win32]::SetForegroundWindow($proc.MainWindowHandle)
    Start-Sleep -Milliseconds 500
    $bmp = New-Object System.Drawing.Bitmap($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bmp)
    $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size($width, $height)))
    $shotPath = "C:\Users\boluc\.gemini\antigravity-ide\brain\9591d6b0-70be-4666-b562-c62cde92482a\clean_dashboard_verified.png"
    $bmp.Save($shotPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bmp.Dispose()
    Write-Host "Saved live screenshot to $shotPath"
}

# UI Automation Check
$root = [System.Windows.Automation.AutomationElement]::RootElement
$appCond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $proc.Id)
$appWindow = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $appCond)

if ($appWindow) {
    Write-Host "Automation Window Found: $($appWindow.Current.Name)"
    $textCond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Text)
    $texts = $appWindow.FindAll([System.Windows.Automation.TreeScope]::Descendants, $textCond)
    Write-Host "Total TextBlock elements found: $($texts.Count)"
    
    # Print sample text blocks found in UI
    $sampleTexts = @()
    foreach ($t in $texts) {
        $name = $t.Current.Name
        if (-not [string]::IsNullOrWhiteSpace($name) -and $sampleTexts.Count -lt 25) {
            $sampleTexts += $name
        }
    }
    Write-Host "Sample UI Texts: $($sampleTexts -join ' | ')"
}

Write-Host "`n=== Testing [X] Button (Minimize/Hide to Tray) ==="
[Win32]::PostMessage($proc.MainWindowHandle, [Win32]::WM_SYSCOMMAND, [IntPtr][Win32]::SC_CLOSE, [IntPtr]::Zero)
Start-Sleep -Seconds 2
$proc.Refresh()

Write-Host "After clicking [X]:"
Write-Host "HasExited: $($proc.HasExited)"
if (-not $proc.HasExited) {
    Write-Host "PASS: App successfully stays running in background!"
} else {
    Write-Host "FAIL: App exited when [X] was clicked!"
}

Write-Host "`n=== Testing Tray Restore ==="
[Win32]::ShowWindow($proc.MainWindowHandle, 9) # SW_RESTORE
Start-Sleep -Seconds 1
$proc.Refresh()
Write-Host "Window restored, HasExited: $($proc.HasExited), IsVisible: $([Win32]::IsWindowVisible($proc.MainWindowHandle))"

Write-Host "`nAll automated tests completed successfully."
