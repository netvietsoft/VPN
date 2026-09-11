using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using VpnSDK.Common.Dns;
using VpnSDK.Common.Enums;
using VpnSDK.Common.Settings;
using VpnSDK.DTO;
using VpnSDK.DnsMonitor.DTO;
using VpnSDK.Enums;
using VpnSDK.TrafficOptimizer.DTO;

namespace VpnSDK.Interfaces;

public interface ISDK : INotifyPropertyChanged, IDisposable
{
	bool IsDisposed { get; }

	bool IsElevated { get; }

	bool IsTapDriverInstalled { get; }

	string TapDriverDescription { get; }

	bool IsConnected { get; }

	bool IsConnecting { get; }

	bool IsDisconnecting { get; }

	bool IsConnectionCancelled { get; }

	bool AllowLANTraffic { get; set; }

	bool AllowLocalAdaptersWhenConnected { get; set; }

	bool AllowOnlyVPNConnectivity { get; set; }

	bool DisableDNSLeakProtection { get; set; }

	bool DisableIPv6LeakProtection { get; set; }

	DnsFilteringMode DnsFilterMode { get; set; }

	uint? VpnNetworkInterfaceMetricValue { get; set; }

	ReadOnlyObservableCollection<ILocation> Locations { get; }

	IUser User { get; }

	IBrandingInfo BrandingInfo { get; }

	IConnectionInfo ActiveConnectionInformation { get; }

	NetworkGeolocation CurrentNetworkGeolocation { get; }

	Dictionary<NetworkConnectionType, bool> AvailableProtocols { get; }

	bool IsSplitTunnelEnabled { get; set; }

	SplitTunnelMode SplitTunnelMode { get; set; }

	List<SplitTunnelApp> SplitTunnelAllowedApps { get; set; }

	List<SplitTunnelDomain> SplitTunnelAllowedDomains { get; set; }

	IDnsRequestResolver DnsRequestResolver { get; set; }

	event SDKChangeEventHandler<ConnectionStatus> VpnConnectionStatusChanged;

	event SDKEventHandler<AuthenticationStatus> AuthenticationStatusChanged;

	event SDKEventHandler AuthenticationCredentialsExpired;

	event SDKEventHandler<ConnectionStatus> BeforeConnectionStateFiltersApplied;

	event SDKOperationEventHandler<NetworkGeolocation> UserLocationStatusChanged;

	event SDKOperationEventHandler TapDeviceInstallationStatusChanged;

	event SDKEventHandler<RefreshLocationListStatus> LocationsRefreshStatusChanged;

	event SDKEventHandler<ISDKError> ErrorRaised;

	event SDKEventHandler<DataTransferEventArgs> DataTransferUpdate;

	event EventHandler<IUser> TokensRefreshed;

	event Action<DnsMonitoringArgs> DnsMonitoringUpdate;

	Task Connect(ILocation location, IConnectionConfiguration connectionConfiguration, CancellationToken cancellationToken = default(CancellationToken));

	Task Connect(IEnumerable<ILocation> locations, IConnectionConfiguration connectionConfiguration, CancellationToken cancellationToken = default(CancellationToken));

	Task Connect(IEnumerable<ILocation> locations, IEnumerable<IConnectionConfiguration> connectionConfigurations, CancellationToken cancellationToken = default(CancellationToken));

	Task Connect(ILocation location, IEnumerable<IConnectionConfiguration> connectionConfigurations, CancellationToken cancellationToken = default(CancellationToken));

	Task Disconnect();

	Task<DriverInstallResult> InstallTapDriver();

	Task<DriverUninstallResult> UninstallTapDriver();

	Task<IBrandingInfo> GetBrandingInfo(string slugOrDomain);

	Version GetDriverVersion(Driver driver);

	Task Login(string username, string password);

	Task Login(string username, string password, string slugOrDomain);

	Task LoginWithAuthTokens(string accessToken, string refreshToken);

	Task LoginWithAuthTokens(string accessToken, string refreshToken, string slugOrDomain);

	Task Logout();

	Task RefreshUserToken();

	Task<bool> RefreshServerInfo();

	Task<Dictionary<string, string>> GetAccountMetadata();

	Task<bool> SaveAccountMetadata(Dictionary<string, string> metadata);

	Task DisposeAsync();

	bool StopSplitTunnelDriverService(bool waitForStop);

	bool UninstallSplitTunnelDriverService(bool waitForStop);

	void SetTrafficOptimizerConfig(TrafficOptimizerConfig trafficOptimizerConfig);

	void SetDnsMonitorConfig(DnsMonitoringConfig dnsMonitorConfig);

	string GetActiveVpnConnectionName();
}
