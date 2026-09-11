Add-Type -Path 'E:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop\NextAiVPN.Desktop\bin\Release\net10.0-windows\VpnSDK.dll' -ErrorAction SilentlyContinue
$asm = [System.Reflection.Assembly]::LoadFrom('E:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop\NextAiVPN.Desktop\bin\Release\net10.0-windows\VpnSDK.dll')
$loc = $asm.GetType('VpnSDK.Interfaces.ILocation')
foreach ($t in $asm.GetTypes()) {
    if ($loc.IsAssignableFrom($t)) {
        Write-Output "Type: $($t.FullName) | IsInterface: $($t.IsInterface)"
    }
}
