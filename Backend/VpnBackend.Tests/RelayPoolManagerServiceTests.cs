using System;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using VpnResidentialHub.Services;
using Xunit;

namespace VpnBackend.Tests;

/// <summary>
/// [VI] Bộ kiểm thử đơn vị cho RelayPoolManagerService (Quản lý dàn VPS trung chuyển và Master Switch)
/// [EN] Unit tests for RelayPoolManagerService (Relay fleet management, master switch, routing policies, stream tracking)
/// </summary>
public class RelayPoolManagerServiceTests
{
    private RelayPoolManagerService CreateService()
    {
        return new RelayPoolManagerService(NullLogger<RelayPoolManagerService>.Instance);
    }

    [Fact]
    public void ActiveMode_PropertyChange_PersistsMode()
    {
        // [VI] Kiểm tra thay đổi chế độ trung chuyển (Relay Mode Switch)
        // [EN] Verify changing relay routing mode updates state
        var service = CreateService();

        service.ActiveMode = RelayRoutingMode.FullRelay;
        Assert.Equal(RelayRoutingMode.FullRelay, service.ActiveMode);

        service.ActiveMode = RelayRoutingMode.SemiRelay;
        Assert.Equal(RelayRoutingMode.SemiRelay, service.ActiveMode);

        service.ActiveMode = RelayRoutingMode.DirectBypass;
        Assert.Equal(RelayRoutingMode.DirectBypass, service.ActiveMode);
    }

    [Fact]
    public void ShouldUseRelay_RespectsRoutingPolicyModes()
    {
        // [VI] Kiểm tra logic chính sách chuyển mạch cho từng chế độ
        // [EN] Verify routing policy decisions based on active mode
        var service = CreateService();

        // 1. FullRelay: 100% lưu lượng đi qua Relay
        service.ActiveMode = RelayRoutingMode.FullRelay;
        Assert.True(service.ShouldUseRelay(isVipUser: true, targetCountry: "US"));
        Assert.True(service.ShouldUseRelay(isVipUser: false, targetCountry: "VN"));

        // 2. DirectBypass: Bỏ qua toàn bộ Relay
        service.ActiveMode = RelayRoutingMode.DirectBypass;
        Assert.False(service.ShouldUseRelay(isVipUser: true, targetCountry: "US"));
        Assert.False(service.ShouldUseRelay(isVipUser: false, targetCountry: "VN"));

        // 3. SemiRelay: VIP đi qua Relay
        service.ActiveMode = RelayRoutingMode.SemiRelay;
        Assert.True(service.ShouldUseRelay(isVipUser: true, targetCountry: "US"));
    }

    [Fact]
    public void AddAndDeleteRelay_ManagesRelayFleet()
    {
        // [VI] Kiểm tra thêm và xóa VPS trung chuyển trong cụm
        // [EN] Verify adding and deleting relay nodes in fleet
        var service = CreateService();
        var node = new RelayNode
        {
            Name = "Relay-Test-Tokyo",
            Ip = "140.82.1.99",
            Port = 10000,
            Country = "JP",
            CountryName = "Japan",
            Region = "Asia",
            Weight = 80,
            Status = "ONLINE"
        };

        var added = service.AddRelay(node);
        Assert.NotNull(added);
        Assert.Equal(node.Ip, added.Ip);

        var lookup = service.GetById(added.Id);
        Assert.NotNull(lookup);
        Assert.Equal("Relay-Test-Tokyo", lookup.Name);

        bool deleted = service.DeleteRelay(added.Id);
        Assert.True(deleted);

        var afterDelete = service.GetById(added.Id);
        Assert.Null(afterDelete);
    }

    [Fact]
    public void TrackStreamStartAndEnd_UpdatesActiveStreamsAndByteCounters()
    {
        // [VI] Kiểm tra theo dõi số luồng kết nối và cộng dồn dung lượng chuyển tiếp
        // [EN] Verify active stream counting and byte transfer tracking
        var service = CreateService();
        var allRelays = service.GetAllRelays();
        Assert.NotEmpty(allRelays);

        var targetRelay = allRelays[0];
        int initialStreams = targetRelay.ActiveStreams;
        long initialBytes = targetRelay.TotalBytesRelayed;

        // Bắt đầu luồng
        service.TrackStreamStart(targetRelay.Id);
        var afterStart = service.GetById(targetRelay.Id);
        Assert.NotNull(afterStart);
        Assert.Equal(initialStreams + 1, afterStart.ActiveStreams);

        // Kết thúc luồng với 50KB chuyển tiếp
        long transferred = 51200;
        service.TrackStreamEnd(targetRelay.Id, transferred);
        var afterEnd = service.GetById(targetRelay.Id);
        Assert.NotNull(afterEnd);
        Assert.Equal(initialStreams, afterEnd.ActiveStreams);
        Assert.Equal(initialBytes + transferred, afterEnd.TotalBytesRelayed);
    }
}
