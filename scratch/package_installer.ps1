$ErrorActionPreference = "Stop"
$binDir = "E:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop\NextAiVPN.Desktop\bin\Release\net10.0-windows"
$zipPath = "E:\DECOMPILER\Soft\VPN\CONVERT\apps\installer\NextAiVPN.Setup\payload.zip"

Write-Host "[*] Packing binaries from $binDir to $zipPath..."
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path "$binDir\*" -DestinationPath $zipPath -CompressionLevel Optimal
$size = (Get-Item $zipPath).Length
Write-Host "[OK] New payload.zip created. Size: $size bytes ($([math]::Round($size/1MB, 2)) MB)"

Write-Host "[*] Publishing NextAiVPN_Setup.exe..."
dotnet publish "E:\DECOMPILER\Soft\VPN\CONVERT\apps\installer\NextAiVPN.Setup\NextAiVPN.Setup.csproj" -c Release -r win-x64 --self-contained true

$outExe = "E:\DECOMPILER\Soft\VPN\CONVERT\apps\installer\NextAiVPN.Setup\bin\Release\net10.0-windows\win-x64\publish\NextAiVPN_Setup.exe"
if (Test-Path $outExe) {
    Copy-Item $outExe "E:\DECOMPILER\Soft\VPN\CONVERT\NextAiVPN_Setup.exe" -Force
    Copy-Item $outExe "E:\DECOMPILER\Soft\VPN\CONVERT\installer\NextAiVPN_Setup.exe" -Force
    Write-Host "[OK] NextAiVPN_Setup.exe updated successfully at root and installer/!"
} else {
    Write-Host "[ERROR] Output file not found at $outExe"
}
