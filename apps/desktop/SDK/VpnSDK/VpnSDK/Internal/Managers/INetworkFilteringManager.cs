using System;
using System.Collections.Generic;
using System.Net;
using VpnSDK.Common.Dns;
using VpnSDK.Common.Enums;
using VpnSDK.Common.Settings;
using VpnSDK.Enums;
using VpnSDK.NetFilter.CalloutDriver;

namespace VpnSDK.Internal.Managers;

internal interface INetworkFilteringManager : IDisposable
{
	bool AllowLanConnectivity { get; set; }

	bool AllowLocalAdaptersWhenConnected { get; set; }

	bool DisableDNSLeakProtection { get; set; }

	bool DisableIPv6LeakProtection { get; set; }

	bool KillSwitchEnabled { get; set; }

	bool IsSplitTunnelEnabled { get; set; }

	SplitTunnelMode SplitTunnelMode { get; set; }

	List<SplitTunnelApp> SplitTunnelAllowedApps { get; set; }

	List<SplitTunnelDomain> SplitTunnelAllowedDomains { get; set; }

	IDnsRequestResolver DnsRequestResolver { get; set; }

	DnsFilteringMode DnsFilterMode { get; set; }

	uint? VpnNetworkInterfaceMetricValue { get; set; }

	bool AllowDnsQueries { get; set; }

	void HandleConnectionStatus(ConnectionStatus status);

	void AddAddressToWhitelist(IPAddress ipaddress);

	bool StopSplitTunnelDriverService(bool waitForStop);

	bool UninstallSplitTunnelDriverService(bool waitForStop);

	void UpdateOpenVpnNetworkFilter();

	ICalloutDriverService GetCalloutDriverService();
}
