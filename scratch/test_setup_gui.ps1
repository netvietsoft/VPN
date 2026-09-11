Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$exePath = "E:\DECOMPILER\Soft\VPN\CONVERT\installer\NextAiVPN_Setup.exe"
Write-Host "Launching: $exePath"
$p = Start-Process -FilePath $exePath -PassThru
Start-Sleep -Seconds 4

$proc = Get-Process -Id $p.Id -ErrorAction SilentlyContinue
if ($proc) {
    Write-Host "Process running: ID=$($proc.Id), Title='$($proc.MainWindowTitle)', Responding=$($proc.Responding)"
} else {
    Write-Host "Process exited."
}

# Capture screen
$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
$screenshotPath = "C:\Users\boluc\.gemini\antigravity-ide\brain\9591d6b0-70be-4666-b562-c62cde92482a\setup_wizard_screen.png"
$bitmap.Save($screenshotPath, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bitmap.Dispose()
Write-Host "Screenshot saved to: $screenshotPath"
