using Microsoft.AspNetCore.Mvc;
using VpnResidentialHub.Models;
using VpnResidentialHub.Services;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Kestrel lắng nghe trên cổng 6033 (Web Dashboard & REST API theo chuẩn AGENTS.md)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(6033);
});

// Đăng ký các dịch vụ Singleton độc lập
builder.Services.AddSingleton<INodePoolService, NodePoolService>();
builder.Services.AddSingleton<ITenantService, TenantService>();
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

#endregion

Console.WriteLine("=================================================================");
Console.WriteLine("  NEXTAI RESIDENTIAL PROXY & VPN GATEWAY PLATFORM (NET 10)");
Console.WriteLine("  Web Admin & API Docs: http://127.0.0.1:5005");
Console.WriteLine("  Universal Proxy Port: 10000 (SOCKS5 & HTTP CONNECT)");
Console.WriteLine("  Dedicated Port Range: 10001 - 10500");
Console.WriteLine("=================================================================");

app.Run();
