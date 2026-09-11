using namespace System.Net.Sockets;
using namespace System.Text;

$client = New-Object TcpClient;
$client.Connect("127.0.0.1", 10000);
$stream = $client.GetStream();

# Gửi HTTP CONNECT request với Proxy-Authorization chuẩn xác
$authRaw = "kikilogin-country-vn-session-profile888-time-20:kiki_resident_pass_2026";
$authBase64 = [Convert]::ToBase64String([Encoding]::ASCII.GetBytes($authRaw));

$req = "CONNECT 1.1.1.1:80 HTTP/1.1`r`nHost: 1.1.1.1:80`r`nProxy-Authorization: Basic $authBase64`r`n`r`n";
$reqBytes = [Encoding]::ASCII.GetBytes($req);
$stream.Write($reqBytes, 0, $reqBytes.Length);

Start-Sleep -Milliseconds 1200;

# Kiểm tra lại sticky sessions từ API
$sessions = Invoke-RestMethod -Uri 'http://127.0.0.1:6033/api/v1/proxies/sticky-sessions';
Write-Host "Sticky sessions count:" $sessions.data.Count;
if ($sessions.data.Count -gt 0) {
    Write-Host "Session Key:" $sessions.data[0].sessionKey;
    Write-Host "Assigned Proxy:" $sessions.data[0].assignedProxyHost;
    Write-Host "Country:" $sessions.data[0].country;
    Write-Host "Created At:" $sessions.data[0].createdAt;
    Write-Host "Expires At:" $sessions.data[0].expiresAt;
}

$client.Close();
