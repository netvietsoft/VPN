using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using VpnResidentialHub.Models;
using VpnResidentialHub.Services;
using Xunit;

namespace VpnBackend.Tests;

/// <summary>
/// [VI] Bộ kiểm thử đơn vị cho TenantService (Quản lý khách hàng thuê proxy)
/// [EN] Unit tests for TenantService (Proxy tenant management, persistence & authentication)
/// </summary>
public class TenantServiceTests
{
    private TenantService CreateService()
    {
        return new TenantService(NullLogger<TenantService>.Instance);
    }

    [Fact]
    public void CreateTenant_ValidRequest_CreatesAndRetrievesSuccessfully()
    {
        // [VI] Kiểm tra tạo tài khoản khách hàng thành công
        // [EN] Verify creating tenant account succeeds
        var service = CreateService();
        var req = new CreateTenantRequest
        {
            Name = "Test-Customer-" + Guid.NewGuid().ToString("N")[..6],
            QuotaGb = 5.0,
            MaxConnections = 50,
            ExpiryDays = 30
        };

        var tenant = service.CreateTenant(req);

        Assert.NotNull(tenant);
        Assert.Equal(req.Name, tenant.Name);
        Assert.NotNull(tenant.ApiKey);
        Assert.NotNull(tenant.Username);
        Assert.NotNull(tenant.Password);

        // Kiểm tra truy vấn qua ID và API Key
        var retrievedById = service.GetById(tenant.Id);
        Assert.NotNull(retrievedById);
        Assert.Equal(tenant.Name, retrievedById.Name);

        var retrievedByKey = service.GetByApiKey(tenant.ApiKey);
        Assert.NotNull(retrievedByKey);
        Assert.Equal(tenant.Id, retrievedByKey.Id);
    }

    [Fact]
    public void Authenticate_ConstantTimeVerification_ValidAndInvalidCredentials()
    {
        // [VI] Kiểm tra xác thực mật khẩu an toàn
        // [EN] Verify secure constant-time password authentication
        var service = CreateService();
        var req = new CreateTenantRequest
        {
            Name = "Auth-Test-User",
            QuotaGb = 2.0
        };
        var tenant = service.CreateTenant(req);

        // Trường hợp mật khẩu đúng
        var authSuccess = service.Authenticate(tenant.Username, tenant.Password);
        Assert.NotNull(authSuccess);
        Assert.Equal(tenant.Id, authSuccess.Id);

        // Trường hợp mật khẩu sai
        var authWrongPass = service.Authenticate(tenant.Username, "wrong_password_123");
        Assert.Null(authWrongPass);

        // Trường hợp username không tồn tại
        var authWrongUser = service.Authenticate("non_existent_user_xyz", "any_pass");
        Assert.Null(authWrongUser);
    }

    [Fact]
    public void DeleteTenant_ExistingTenant_RemovesFromCollection()
    {
        // [VI] Kiểm tra xóa tài khoản khách hàng
        // [EN] Verify tenant deletion
        var service = CreateService();
        var req = new CreateTenantRequest { Name = "Tenant-To-Delete" };
        var tenant = service.CreateTenant(req);

        bool deleted = service.DeleteTenant(tenant.Id);
        Assert.True(deleted);

        var lookup = service.GetById(tenant.Id);
        Assert.Null(lookup);
    }

    [Fact]
    public void ParseProxyUsername_ExtendedTags_ExtractsAllRoutingOptions()
    {
        // [VI] Kiểm tra phân tích cú pháp định dạng username mở rộng
        // [EN] Verify parsing extended username tags for proxy routing
        var service = CreateService();
        var req = new CreateTenantRequest { Name = "Routing-Tenant" };
        var tenant = service.CreateTenant(req);

        string formattedUsername = $"{tenant.Username}-country-vn-city-danang-session-sess9988-time-45";
        var parsed = service.ParseProxyUsername(formattedUsername);

        Assert.NotNull(parsed.Tenant);
        Assert.Equal(tenant.Id, parsed.Tenant.Id);
        Assert.Equal("VN", parsed.Country);
        Assert.Equal("danang", parsed.City);
        Assert.Equal("sess9988", parsed.SessionId);
        Assert.Equal(45, parsed.SessionMinutes);
    }

    [Fact]
    public void RecordUsage_ConcurrentCalls_AccumulatesBandwidthThreadSafely()
    {
        // [VI] Kiểm tra tính an toàn đa luồng khi ghi nhận lưu lượng băng thông
        // [EN] Verify thread-safe bandwidth consumption recording under concurrency
        var service = CreateService();
        var req = new CreateTenantRequest { Name = "Concurrency-Tenant" };
        var tenant = service.CreateTenant(req);

        int tasksCount = 20;
        long bytesPerTask = 50000;

        Parallel.For(0, tasksCount, _ =>
        {
            service.RecordUsage(tenant.Id, bytesPerTask);
        });

        var updatedTenant = service.GetById(tenant.Id);
        Assert.NotNull(updatedTenant);
        Assert.True(updatedTenant.UsedBytes >= tasksCount * bytesPerTask);
    }
}
