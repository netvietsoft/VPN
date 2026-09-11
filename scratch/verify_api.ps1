$body = '{"type":"socks5","host":"192.0.2.1","port":9999,"country":"US","city":"TestCity","isp":"TestOffline"}';
$res = Invoke-RestMethod -Uri 'http://127.0.0.1:6033/api/v1/proxies' -Method POST -Body $body -ContentType 'application/json';
$id = $res.data.id;
Write-Host "Created test proxy id: $id";

$testRes = Invoke-RestMethod -Uri "http://127.0.0.1:6033/api/v1/proxies/$id/test" -Method POST;
Write-Host "Status after test: $($testRes.data.status) | PingMs: $($testRes.data.pingMs)";

$batchBody = @{ ids = @($id) } | ConvertTo-Json;
$delRes = Invoke-RestMethod -Uri 'http://127.0.0.1:6033/api/v1/proxies/batch-delete' -Method POST -Body $batchBody -ContentType 'application/json';
Write-Host "Batch delete success: $($delRes.success) | Deleted: $($delRes.data.deleted)";
