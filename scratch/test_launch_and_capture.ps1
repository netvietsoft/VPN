Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# 1. Kill previous instances
Get-Process NextAiVPN* -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 800

# 2. Start the fresh Release executable
$exePath = "E:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop\NextAiVPN.Desktop\bin\Release\net10.0-windows\NextAiVPN.Desktop.exe"
$workingDir = "E:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop\NextAiVPN.Desktop\bin\Release\net10.0-windows"
Write-Host "[*] Launching $exePath..."
$proc = Start-Process -FilePath $exePath -WorkingDirectory $workingDir -PassThru

# 3. Wait for window to load
Start-Sleep -Seconds 5

# 4. Capture screenshot
$screen = [System.Windows.Forms.Screen]::PrimaryScreen
$bounds = $screen.Bounds
$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)

$savePath = "C:\Users\boluc\.gemini\antigravity-ide\brain\d99f4cd0-bb44-4023-a706-cdfde43ff58e\desktop_app_live_emerald_test.png"
$bitmap.Save($savePath, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bitmap.Dispose()

Write-Host "[OK] Screenshot saved to $savePath"
Write-Host "[*] Process ID: $($proc.Id), Responding: $($proc.Responding)"
