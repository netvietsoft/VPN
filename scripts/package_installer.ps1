# ==============================================================================
# [VI] Script đóng gói và xuất bản Single-File Installer NextAiVPN_Setup.exe
# [EN] Script to package and publish Single-File Installer NextAiVPN_Setup.exe
# ==============================================================================
$ErrorActionPreference = "Stop"
$root = "$PSScriptRoot\.."
$binDir = "$root\apps\desktop\NextAiVPN.Desktop\bin\Release\net10.0-windows"
$zipPath = "$root\apps\installer\NextAiVPN.Setup\payload.zip"

Write-Host "[*] Packing binaries from $binDir to $zipPath..."
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path "$binDir\*" -DestinationPath $zipPath -CompressionLevel Optimal
$size = (Get-Item $zipPath).Length
Write-Host "[OK] New payload.zip created. Size: $size bytes ($([math]::Round($size/1MB, 2)) MB)"

Write-Host "[*] Publishing NextAiVPN_Setup.exe..."
dotnet publish "$root\apps\installer\NextAiVPN.Setup\NextAiVPN.Setup.csproj" -c Release -r win-x64 --self-contained true

$outExe = "$root\apps\installer\NextAiVPN.Setup\bin\Release\net10.0-windows\win-x64\publish\NextAiVPN_Setup.exe"
if (Test-Path $outExe) {
    Copy-Item $outExe "$root\NextAiVPN_Setup.exe" -Force
    Copy-Item $outExe "$root\installer\NextAiVPN_Setup.exe" -Force
    Write-Host "[OK] NextAiVPN_Setup.exe updated successfully at root and installer/!"
} else {
    Write-Host "[ERROR] Output file not found at $outExe"
}
