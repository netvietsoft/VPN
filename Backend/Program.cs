using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VpnResidentialHub.Models;
using VpnResidentialHub.Services;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Kestrel lắng nghe trên cổng 6033 (Web Dashboard & REST API theo chuẩn Convertme.txt mục 14)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(6033);
});

// Đăng ký các dịch vụ Singleton độc lập
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IGeoIpService, GeoIpService>();
builder.Services.AddSingleton<INodePoolService, NodePoolService>();
builder.Services.AddSingleton<ITenantService, TenantService>();
builder.Services.AddSingleton<IProxyManagerService, ProxyManagerService>();
builder.Services.AddSingleton<ISmartProxyRotationEngine, SmartProxyRotationEngine>();
builder.Services.AddSingleton<IRelayPoolManagerService, RelayPoolManagerService>();
builder.Services.AddSingleton<ProxyHealthCheckBackgroundService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ProxyHealthCheckBackgroundService>());
builder.Services.AddSingleton<IUniversalGatewayService, UniversalGatewayService>();
builder.Services.AddSingleton<IDedicatedPortService, DedicatedPortService>();

// Kích hoạt CORS mở để các hệ thống web/antidetect bên ngoài dễ dàng gọi
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/cms_admin.html", (IWebHostEnvironment env) => 
{
    var path = Path.Combine(env.WebRootPath ?? "wwwroot", "cms_admin.html");
    if (!File.Exists(path)) path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "cms_admin.html");
    return File.Exists(path) ? Results.File(path, "text/html; charset=utf-8") : Results.NotFound("cms_admin.html not found");
});
app.MapGet("/admin", () => Results.Redirect("/cms_admin.html"));
app.MapGet("/admin/index.html", (IWebHostEnvironment env) => 
{
    var path = Path.Combine(env.WebRootPath ?? "wwwroot", "cms_admin.html");
    if (!File.Exists(path)) path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "cms_admin.html");
    return File.Exists(path) ? Results.File(path, "text/html; charset=utf-8") : Results.NotFound("cms_admin.html not found");
});
app.MapGet("/cms", () => Results.Redirect("/cms_admin.html"));
app.MapGet("/dashboard", () => Results.Redirect("/cms_admin.html"));

var cts = new CancellationTokenSource();

// Khởi chạy Cổng Universal Proxy Gateway trên cổng 10000 (SOCKS5 & HTTP CONNECT)
var gatewayService = app.Services.GetRequiredService<IUniversalGatewayService>();
gatewayService.Start(10000, cts.Token);

var startTime = DateTime.UtcNow;

#region REST API Endpoints

// 1. Health check & System Stats
app.MapGet("/api/v1/health", (
    [FromServices] INodePoolService nodePool,
    [FromServices] IUniversalGatewayService gateway,
    [FromServices] ITenantService tenantService) =>
{
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "NextAI Residential Gateway đang hoạt động ổn định.",
        Data = new
        {
            Status = "ONLINE",
            Uptime = DateTime.UtcNow - startTime,
            GatewayPort = 10000,
            DashboardPort = 6033,
            TotalPoolNodes = nodePool.TotalActiveNodes,
            ActiveConnections = gateway.ActiveConnectionsCount,
            TotalBytesServed = gateway.TotalBytesServed,
            TotalTenants = tenantService.GetAllTenants().Count
        }
    });
});

// 2. Danh sách Node / Quốc gia trong Pool cư dân
app.MapGet("/api/v1/residential/nodes", ([FromServices] INodePoolService nodePool) =>
{
    var nodes = nodePool.GetAllNodes();
    var countries = nodes.GroupBy(n => n.Country)
        .Select(g => new
        {
            CountryCode = g.Key,
            CountryName = g.First().CountryName,
            TotalNodes = g.Count(),
            Cities = g.Select(c => c.City).Distinct().ToList()
        });

    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Lấy danh sách node cư dân thành công.",
        Data = new
        {
            Countries = countries,
            Nodes = nodes
        }
    });
});

// 2.1. Phân hệ thu thập IP cư dân từ thiết bị cài đặt NextAI VPN
// [VI] Lấy địa chỉ IP kết nối của Client
app.MapGet("/api/v1/client/my-ip", (HttpContext ctx) =>
{
    var ip = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault()
             ?? ctx.Request.Headers["X-Real-IP"].FirstOrDefault()
             ?? ctx.Connection.RemoteIpAddress?.ToString()
             ?? "127.0.0.1";
    if (ip.StartsWith("::ffff:")) ip = ip.Substring(7);

    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Lấy địa chỉ IP kết nối thành công.",
        Data = new { ip = ip }
    });
});

// [VI] Đăng ký node thiết bị khách hàng cài NextAI VPN đóng góp IP
app.MapPost("/api/v1/residential/nodes/register", async (
    [FromBody] RegisterClientNodeRequest req,
    HttpContext ctx,
    [FromServices] INodePoolService nodePool) =>
{
    var detectedIp = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                     ?? ctx.Request.Headers["X-Real-IP"].FirstOrDefault()
                     ?? ctx.Connection.RemoteIpAddress?.ToString()
                     ?? "127.0.0.1";
    if (detectedIp.StartsWith("::ffff:")) detectedIp = detectedIp.Substring(7);

    var node = await nodePool.RegisterClientNodeAsync(req, detectedIp);
    return Results.Ok(new ApiResponse<ClientDeviceNode>
    {
        Success = true,
        Message = "Đăng ký thiết bị đóng góp IP dân cư thành công.",
        Data = node
    });
});

// [VI] Nhịp tim (Heartbeat) định kỳ từ thiết bị cài VPN
app.MapPost("/api/v1/residential/nodes/heartbeat", (
    [FromBody] ClientNodeHeartbeatRequest req,
    [FromServices] INodePoolService nodePool) =>
{
    bool ok = nodePool.RecordClientHeartbeat(req);
    return Results.Ok(new ApiResponse<object>
    {
        Success = ok,
        Message = ok ? "Đã ghi nhận nhịp tim thiết bị." : "Không tìm thấy DeviceId đã đăng ký.",
        Data = new { deviceId = req.DeviceId, isOnline = ok }
    });
});

// [VI] Lấy danh sách các thiết bị người dùng đã cài VPN đang online/offline
app.MapGet("/api/v1/residential/nodes/clients", ([FromServices] INodePoolService nodePool) =>
{
    var clients = nodePool.GetClientNodes();
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Lấy danh sách thiết bị cài VPN thành công.",
        Data = new
        {
            total = clients.Count,
            online = clients.Count(c => c.IsOnline),
            offline = clients.Count(c => !c.IsOnline),
            nodes = clients
        }
    });
});

// 3. Quản lý Khách hàng thuê (Tenants)
app.MapGet("/api/v1/tenants", ([FromServices] ITenantService tenantService) =>
{
    var tenants = tenantService.GetAllTenants();
    return Results.Ok(new ApiResponse<IReadOnlyList<Tenant>>
    {
        Success = true,
        Message = "Lấy danh sách khách hàng thành công.",
        Data = tenants
    });
});

app.MapPost("/api/v1/tenants", (
    [FromBody] CreateTenantRequest req,
    [FromServices] ITenantService tenantService) =>
{
    if (string.IsNullOrWhiteSpace(req.Name))
    {
        return Results.BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = "Tên khách hàng không được để trống!"
        });
    }

    var tenant = tenantService.CreateTenant(req);
    return Results.Ok(new ApiResponse<Tenant>
    {
        Success = true,
        Message = $"Đã tạo tài khoản cho thuê '{tenant.Name}' thành công.",
        Data = tenant
    });
});

// 4. Sinh danh sách proxy xuất cho khách hàng / profile
app.MapPost("/api/v1/proxy/generate", (
    [FromBody] GenerateProxyRequest req,
    [FromServices] ITenantService tenantService,
    HttpRequest httpReq) =>
{
    var tenant = tenantService.GetByApiKey(req.ApiKey) ?? tenantService.GetAllTenants().FirstOrDefault();
    if (tenant == null)
    {
        return Results.Unauthorized();
    }

    var host = httpReq.Host.Host;
    if (host == "localhost" || host == "0.0.0.0") host = "127.0.0.1";
    int port = 10000;

    var lines = new List<string>();
    int qty = Math.Clamp(req.Quantity, 1, 500);

    for (int i = 0; i < qty; i++)
    {
        string u = tenant.Username;
        if (!string.IsNullOrWhiteSpace(req.Country) && !req.Country.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            u += $"-country-{req.Country.ToLowerInvariant()}";
        }
        if (!string.IsNullOrWhiteSpace(req.City))
        {
            u += $"-city-{req.City.ToLowerInvariant()}";
        }
        if (req.Mode.Equals("sticky", StringComparison.OrdinalIgnoreCase))
        {
            string sess = Guid.NewGuid().ToString("N")[..8];
            int ttl = req.SessionMinutes > 0 ? req.SessionMinutes : 30;
            u += $"-session-{sess}-time-{ttl}";
        }

        lines.Add($"{host}:{port}:{u}:{tenant.Password}");
    }

    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = $"Đã sinh thành công {lines.Count} dòng proxy.",
        Data = new
        {
            Tenant = tenant.Name,
            TotalLines = lines.Count,
            Proxies = lines
        }
    });
});

// 5. Quản lý luồng trực tiếp (Live Sessions)
app.MapGet("/api/v1/gateway/sessions", ([FromServices] IUniversalGatewayService gateway) =>
{
    var sessions = gateway.GetActiveSessions();
    return Results.Ok(new ApiResponse<IReadOnlyList<ProxySession>>
    {
        Success = true,
        Message = $"Hiện có {sessions.Count} kết nối trực tiếp.",
        Data = sessions
    });
});

// 5.1. [VI] Thống kê lưu lượng Gateway thời gian thực (Real-time Bandwidth & Throughput)
//      [EN] Real-time Gateway bandwidth and throughput statistics
app.MapGet("/api/v1/gateway/stats", (
    [FromServices] IUniversalGatewayService gateway,
    [FromServices] IProxyManagerService proxyManager) =>
{
    var activeProxy = proxyManager.GetActiveVpnProxy();
    long bytesIn = gateway.TotalBytesIn;
    long bytesOut = gateway.TotalBytesOut;
    long totalBytes = gateway.TotalBytesServed;
    if (totalBytes < bytesIn + bytesOut) totalBytes = bytesIn + bytesOut;

    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Thống kê lưu lượng Universal Gateway thời gian thực.",
        Data = new
        {
            BytesIn = bytesIn,
            BytesOut = bytesOut,
            TotalBytes = totalBytes,
            DownloadMb = Math.Round(bytesIn / 1048576.0, 2),
            UploadMb = Math.Round(bytesOut / 1048576.0, 2),
            ActiveSessions = gateway.ActiveConnectionsCount,
            ActiveProxy = activeProxy != null ? new
            {
                Id = activeProxy.Id,
                Host = activeProxy.Host,
                Port = activeProxy.Port,
                Country = activeProxy.Country,
                City = activeProxy.City,
                Type = activeProxy.Type
            } : null
        }
    });
});

// 6. Cổng Dedicated Port riêng biệt
app.MapPost("/api/v1/dedicated/allocate", (
    [FromQuery] string? tenantId,
    [FromQuery] string? country,
    [FromQuery] string? city,
    [FromServices] IDedicatedPortService dedicatedService,
    [FromServices] ITenantService tenantService) =>
{
    var tenant = tenantService.GetById(tenantId ?? "tenant_kiki") ?? tenantService.GetAllTenants().First();
    try
    {
        var port = dedicatedService.AllocateDedicatedPort(tenant.Id, country, city);
        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = $"Đã mở thành công cổng riêng {port} cho Tenant '{tenant.Name}'.",
            Data = new
            {
                Port = port,
                ProxyUrl = $"socks5://127.0.0.1:{port}",
                Country = country ?? "AUTO"
            }
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
    }
});

app.MapGet("/api/v1/dedicated/ports", ([FromServices] IDedicatedPortService dedicatedService) =>
{
    var ports = dedicatedService.GetActivePorts();
    return Results.Ok(new ApiResponse<IReadOnlyList<DedicatedPortInfo>>
    {
        Success = true,
        Message = $"Có {ports.Count} cổng riêng đang mở.",
        Data = ports
    });
});

app.MapDelete("/api/v1/dedicated/{port:int}", (int port, [FromServices] IDedicatedPortService dedicatedService) =>
{
    var released = dedicatedService.ReleaseDedicatedPort(port);
    return Results.Ok(new ApiResponse<object>
    {
        Success = released,
        Message = released ? $"Đã đóng cổng {port} thành công." : $"Không tìm thấy cổng {port}."
    });
});

// [VI] API truy vấn danh sách Xray Reality Servers từ CSDL
// [EN] API endpoint to query Xray Reality Servers from Database
app.MapGet("/api/xray/servers", () =>
{
    string filePath = Path.Combine(AppContext.BaseDirectory, "data", "xray_servers.json");
    if (!File.Exists(filePath)) filePath = Path.Combine(Directory.GetCurrentDirectory(), "Backend", "data", "xray_servers.json");
    if (File.Exists(filePath))
    {
        var json = File.ReadAllText(filePath);
        return Results.Content(json, "application/json");
    }
    return Results.Ok(new List<object>());
});

// [VI] API truy vấn danh sách Xray Users từ CSDL
// [EN] API endpoint to query Xray Users from Database
app.MapGet("/api/xray/users", () =>
{
    string filePath = Path.Combine(AppContext.BaseDirectory, "data", "xray_users.json");
    if (!File.Exists(filePath)) filePath = Path.Combine(Directory.GetCurrentDirectory(), "Backend", "data", "xray_users.json");
    if (File.Exists(filePath))
    {
        var json = File.ReadAllText(filePath);
        return Results.Content(json, "application/json");
    }
    return Results.Ok(new List<object>());
});

// [VI] API kiểm tra trực tiếp (Live Test) tình trạng kết nối các máy chủ Xray VPN
// [EN] API endpoint to perform real-time live connectivity test for Xray VPN servers
app.MapPost("/api/xray/test-live", async ([FromServices] IProxyManagerService proxyManager) =>
{
    string filePath = Path.Combine(AppContext.BaseDirectory, "data", "xray_servers.json");
    if (!File.Exists(filePath)) filePath = Path.Combine(Directory.GetCurrentDirectory(), "Backend", "data", "xray_servers.json");
    if (!File.Exists(filePath))
    {
        return Results.NotFound(new { success = false, message = "Không tìm thấy file cấu hình xray_servers.json" });
    }

    var json = File.ReadAllText(filePath);
    using var doc = System.Text.Json.JsonDocument.Parse(json);
    var servers = new List<Dictionary<string, object>>();

    foreach (var el in doc.RootElement.EnumerateArray())
    {
        var dict = new Dictionary<string, object>();
        foreach (var prop in el.EnumerateObject())
        {
            if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
                dict[prop.Name] = prop.Value.GetInt32();
            else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.True || prop.Value.ValueKind == System.Text.Json.JsonValueKind.False)
                dict[prop.Name] = prop.Value.GetBoolean();
            else
                dict[prop.Name] = prop.Value.GetString() ?? "";
        }

        string host = dict.TryGetValue("connect_address", out var ca) ? ca?.ToString() ?? "" : dict.GetValueOrDefault("host")?.ToString() ?? "";
        int port = dict.TryGetValue("vless_port", out var vp) && vp is int p ? p : 63821;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(3500);

            var finished = await Task.WhenAny(connectTask, timeoutTask);
            if (finished == connectTask && client.Connected)
            {
                sw.Stop();
                dict["status"] = "ONLINE";
                dict["vpn_usable"] = 1;
                dict["ping_ms"] = (int)Math.Max(1, sw.ElapsedMilliseconds);
            }
            else
            {
                dict["status"] = "OFFLINE";
                dict["vpn_usable"] = 0;
            }
        }
        catch
        {
            dict["status"] = "OFFLINE";
            dict["vpn_usable"] = 0;
        }

        servers.Add(dict);
    }

    // Ghi lại kết quả test vào file
    var updatedJson = System.Text.Json.JsonSerializer.Serialize(servers, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(filePath, updatedJson);

    // Đồng bộ vào bộ nhớ ProxyManager
    proxyManager.Reload();

    return Results.Ok(new
    {
        success = true,
        message = $"Đã kiểm tra trực tiếp xong {servers.Count} máy chủ Xray VPN.",
        data = servers
    });
});

// [VI] API đồng bộ các node Xray VPN vào kho Upstream Proxy
// [EN] API endpoint to sync Xray VPN nodes into Upstream Proxy pool
app.MapPost("/api/xray/sync-to-proxies", ([FromServices] IProxyManagerService proxyManager) =>
{
    proxyManager.Reload();
    return Results.Ok(new
    {
        success = true,
        message = "Đã đồng bộ toàn bộ máy chủ Xray VPN vào danh sách Upstream Proxy và Locations của Desktop VPN thành công.",
        total = proxyManager.GetAll().Count
    });
});

// 7. Quản lý Proxy Thật (Real Upstream Proxies)
app.MapGet("/api/v1/proxies", ([FromServices] IProxyManagerService proxyManager) =>
{
    var list = proxyManager.GetAll();
    return Results.Ok(new ApiResponse<IReadOnlyList<UpstreamProxy>>
    {
        Success = true,
        Message = $"Có {list.Count} proxy trong hệ thống.",
        Data = list
    });
});

app.MapPost("/api/v1/proxies", (
    [FromBody] UpstreamProxy proxy,
    [FromServices] IProxyManagerService proxyManager) =>
{
    if (string.IsNullOrWhiteSpace(proxy.Host) || proxy.Port <= 0)
    {
        return Results.BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = "Host và Port không hợp lệ!"
        });
    }

    var added = proxyManager.Add(proxy);
    return Results.Ok(new ApiResponse<UpstreamProxy>
    {
        Success = true,
        Message = "Đã thêm proxy thành công.",
        Data = added
    });
});

app.MapGet("/api/v1/proxies/summary", ([FromServices] IProxyManagerService proxyManager) =>
{
    var summary = proxyManager.GetCountrySummary();
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Lấy tổng hợp quốc gia thành công.",
        Data = summary
    });
});

app.MapPost("/api/v1/proxies/bulk", async (
    [FromBody] BulkImportRequest req,
    [FromServices] IProxyManagerService proxyManager) =>
{
    if (string.IsNullOrWhiteSpace(req.Text))
    {
        return Results.BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = "Nội dung proxy rỗng!"
        });
    }

    var result = await proxyManager.AddBulkAsync(req.Text, req.DefaultType ?? "socks5", req.ReplaceExisting, req.AutoGeoIp);
    return Results.Ok(new ApiResponse<BulkImportResult>
    {
        Success = true,
        Message = $"Đã nhập thành công {result.Added} proxy từ {result.CountryBreakdown.Count} quốc gia.",
        Data = result
    });
});

app.MapPost("/api/v1/proxies/clear", ([FromServices] IProxyManagerService proxyManager) =>
{
    proxyManager.ClearAll();
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Đã xóa toàn bộ proxy trong kho thành công."
    });
});

app.MapPost("/api/v1/proxies/delete-offline", ([FromServices] IProxyManagerService proxyManager) =>
{
    int deleted = proxyManager.DeleteOffline();
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = $"Đã xóa {deleted} proxy bị lỗi (OFFLINE).",
        Data = new { DeletedCount = deleted }
    });
});

app.MapPost("/api/v1/proxies/batch-delete", (
    [FromBody] BatchIdsRequest req,
    [FromServices] IProxyManagerService proxyManager) =>
{
    if (req.Ids == null || req.Ids.Count == 0)
    {
        return Results.BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = "Danh sách ID rỗng!"
        });
    }

    int deleted = proxyManager.BatchDelete(req.Ids);
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = $"Đã xóa thành công {deleted} proxy đã chọn.",
        Data = new { DeletedCount = deleted }
    });
});

app.MapPost("/api/v1/proxies/batch-test", async (
    [FromBody] BatchIdsRequest req,
    [FromServices] IProxyManagerService proxyManager) =>
{
    if (req.Ids == null || req.Ids.Count == 0)
    {
        return Results.BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = "Danh sách ID rỗng!"
        });
    }

    var tested = await proxyManager.BatchTestAsync(req.Ids);
    return Results.Ok(new ApiResponse<IReadOnlyList<UpstreamProxy>>
    {
        Success = true,
        Message = $"Đã kiểm tra ping {tested.Count} proxy được chọn.",
        Data = tested
    });
});

app.MapGet("/api/v1/proxies/health-check/status", ([FromServices] ProxyHealthCheckBackgroundService healthService) =>
{
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Lấy trạng thái chu kỳ kiểm tra sức khỏe 10 phút thành công.",
        Data = healthService.GetStatus()
    });
});

app.MapPost("/api/v1/proxies/health-check/run", async ([FromServices] ProxyHealthCheckBackgroundService healthService) =>
{
    var res = await healthService.RunHealthCheckAsync();
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Đã thực thi kiểm tra sức khỏe toàn bộ kho proxy.",
        Data = res
    });
});

app.MapGet("/api/v1/proxies/sticky-sessions", ([FromServices] ISmartProxyRotationEngine rotationEngine) =>
{
    var sessions = rotationEngine.GetActiveStickySessions();
    return Results.Ok(new ApiResponse<IReadOnlyList<StickySessionEntry>>
    {
        Success = true,
        Message = $"Có {sessions.Count} sticky session đang hoạt động.",
        Data = sessions
    });
});

app.MapDelete("/api/v1/proxies/{id}", (string id, [FromServices] IProxyManagerService proxyManager) =>
{
    bool deleted = proxyManager.Delete(id);
    return Results.Ok(new ApiResponse<object>
    {
        Success = deleted,
        Message = deleted ? "Đã xóa proxy thành công." : "Không tìm thấy proxy."
    });
});

app.MapPost("/api/v1/proxies/{id}/test", async (string id, [FromServices] IProxyManagerService proxyManager) =>
{
    var tested = await proxyManager.TestProxyAsync(id);
    if (tested == null) return Results.NotFound(new ApiResponse<object> { Success = false, Message = "Không tìm thấy proxy." });

    return Results.Ok(new ApiResponse<UpstreamProxy>
    {
        Success = true,
        Message = tested.Status == "LIVE" ? $"Proxy LIVE ({tested.PingMs}ms)" : "Proxy OFFLINE",
        Data = tested
    });
});

app.MapPost("/api/v1/proxies/test-all", async ([FromServices] IProxyManagerService proxyManager) =>
{
    var all = await proxyManager.TestAllAsync();
    return Results.Ok(new ApiResponse<IReadOnlyList<UpstreamProxy>>
    {
        Success = true,
        Message = $"Đã kiểm tra xong {all.Count} proxy.",
        Data = all
    });
});

app.MapPost("/api/v1/proxies/{id}/select", (string id, [FromServices] IProxyManagerService proxyManager) =>
{
    bool selected = proxyManager.SelectActiveVpn(id);
    return Results.Ok(new ApiResponse<object>
    {
        Success = selected,
        Message = selected ? "Đã kích hoạt proxy cho Desktop VPN Client." : "Không tìm thấy proxy."
    });
});

// 8. Cung cấp danh sách Locations cho Desktop Client NextAiVPN
app.MapGet("/api/v1/locations", ([FromServices] IProxyManagerService proxyManager) =>
{
    var locations = proxyManager.GetVpnLocations();
    return Results.Ok(locations);
});

// 9. Quản trị Dàn VPS Trung Chuyển (Multi-Relay Shield Fleet & Master Switch)
app.MapGet("/api/v1/relays", ([FromServices] IRelayPoolManagerService relayPool) =>
{
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Lấy danh sách VPS Trung chuyển thành công.",
        Data = new
        {
            ActiveMode = relayPool.ActiveMode.ToString(),
            TotalRelays = relayPool.GetAllRelays().Count,
            OnlineRelays = relayPool.GetAllRelays().Count(r => r.Status == "ONLINE"),
            Relays = relayPool.GetAllRelays()
        }
    });
});

app.MapPost("/api/v1/relays/mode", ([FromBody] JsonElement body, [FromServices] IRelayPoolManagerService relayPool) =>
{
    if (body.TryGetProperty("mode", out var modeProp))
    {
        string modeStr = modeProp.GetString() ?? "";
        if (Enum.TryParse<RelayRoutingMode>(modeStr, true, out var newMode))
        {
            relayPool.ActiveMode = newMode;
            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Đã chuyển chế độ Trung chuyển sang: {newMode}",
                Data = new { ActiveMode = newMode.ToString() }
            });
        }
    }
    return Results.BadRequest(new ApiResponse<object> { Success = false, Message = "Chế độ không hợp lệ. Cho phép: FullRelay, SemiRelay, DirectBypass" });
});

app.MapPost("/api/v1/relays/{id}/update", (string id, [FromBody] JsonElement body, [FromServices] IRelayPoolManagerService relayPool) =>
{
    int weight = body.TryGetProperty("weight", out var w) ? w.GetInt32() : 100;
    bool isEnabled = !body.TryGetProperty("isEnabled", out var e) || e.GetBoolean();
    int maxStreams = body.TryGetProperty("maxStreams", out var m) ? m.GetInt32() : 5000;

    bool updated = relayPool.UpdateRelay(id, weight, isEnabled, maxStreams);
    return Results.Ok(new ApiResponse<object>
    {
        Success = updated,
        Message = updated ? "Đã cập nhật thông số Relay VPS." : "Không tìm thấy Relay VPS."
    });
});

app.MapPost("/api/v1/relays/health-check", async ([FromServices] IRelayPoolManagerService relayPool) =>
{
    await relayPool.HealthCheckAllAsync();
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = "Đã hoàn thành kiểm tra độ trễ và sức khỏe toàn bộ Relay VPS.",
        Data = relayPool.GetAllRelays()
    });
});

// 10. Quản trị Hạn Mức 10GB/Tháng ~ 300MB/Ngày & Tự Động Đảo Proxy (Per-Proxy Quota Accounting)
app.MapGet("/api/v1/proxies/quotas", ([FromServices] IProxyManagerService proxyManager) =>
{
    return Results.Ok(proxyManager.GetQuotaOverview());
});

app.MapPost("/api/v1/proxies/quotas/reset", ([FromServices] IProxyManagerService proxyManager) =>
{
    int resetCount = proxyManager.ResetDailyQuotas();
    return Results.Ok(new ApiResponse<object>
    {
        Success = true,
        Message = $"Đã reset hạn mức 300MB ngày cho {resetCount} proxy trong kho.",
        Data = new { ResetCount = resetCount }
    });
});

#endregion

Console.WriteLine("=================================================================");
Console.WriteLine("  NEXTAI RESIDENTIAL PROXY & VPN GATEWAY PLATFORM (NET 10)");
Console.WriteLine("  Web Admin & API Docs: http://127.0.0.1:6033");
Console.WriteLine("  Universal Proxy Port: 10000 (SOCKS5 & HTTP CONNECT)");
Console.WriteLine("  Dedicated Port Range: 10001 - 10500");
Console.WriteLine("=================================================================");

app.Run();
