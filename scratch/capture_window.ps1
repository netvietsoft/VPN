Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

Add-Type @"
using System;
using System.Runtime.InteropServices;

public class User32 {
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

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

[User32]::SetForegroundWindow($proc.MainWindowHandle)
Start-Sleep -Milliseconds 500

$rect = New-Object User32+RECT
[User32]::GetWindowRect($proc.MainWindowHandle, [ref]$rect)

$width = $rect.Right - $rect.Left
$height = $rect.Bottom - $rect.Top

Write-Host "Window Bounds: $($rect.Left), $($rect.Top), Width=$width, Height=$height"

if ($width -gt 0 -and $height -gt 0) {
    $bmp = New-Object System.Drawing.Bitmap($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bmp)
    $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size($width, $height)))
    
    $outPath = "C:\Users\boluc\.gemini\antigravity-ide\brain\9591d6b0-70be-4666-b562-c62cde92482a\desktop_live_test.png"
    $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bmp.Dispose()
    Write-Host "Screenshot saved to $outPath"
}

# Inspect UI Automation
$root = [System.Windows.Automation.AutomationElement]::RootElement
$appCond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $proc.Id)
$appWindow = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $appCond)

if ($appWindow) {
    Write-Host "Root Window: $($appWindow.Current.Name)"
    $listCond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::List)
    $lists = $appWindow.FindAll([System.Windows.Automation.TreeScope]::Descendants, $listCond)
    Write-Host "Found $($lists.Count) List controls in window."
    
    $itemCond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::ListItem)
    $items = $appWindow.FindAll([System.Windows.Automation.TreeScope]::Descendants, $itemCond)
    Write-Host "Found $($items.Count) ListItem controls in window."

    # Print first 5 items
    for ($i = 0; $i -lt [Math]::Min(5, $items.Count); $i++) {
        Write-Host "Item $i : $($items[$i].Current.Name)"
    }
}
