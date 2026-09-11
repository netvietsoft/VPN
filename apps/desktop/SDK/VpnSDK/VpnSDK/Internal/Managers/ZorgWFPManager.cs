using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using VpnSDK.Common.Dns;
using VpnSDK.Common.Dto;
using VpnSDK.Common.Enums;
using VpnSDK.Common.Helpers;
using VpnSDK.Common.Settings;
using VpnSDK.Common.Utilities;
using VpnSDK.Core;
using VpnSDK.Core.Helpers;
using VpnSDK.Core.Interfaces;
using VpnSDK.DnsMonitor;
using VpnSDK.DnsMonitor.DTO;
using VpnSDK.DnsMonitor.Interfaces;
using VpnSDK.DnsResolver;
using VpnSDK.DnsResolver.EventArgs;
using VpnSDK.Enums;
using VpnSDK.Helpers;
using VpnSDK.Internal.Configuration;
using VpnSDK.Internal.Extensions;
using VpnSDK.Internal.Helpers;
using VpnSDK.NetFilter;
using VpnSDK.NetFilter.CalloutDriver;
using VpnSDK.NetFilter.Enums;
using VpnSDK.NetFilter.Helpers;
using VpnSDK.NetFilter.Interop;
using VpnSDK.Private.WFP;
using VpnSDK.Private.WFP.Filtering;
using VpnSDK.Private.WFP.Filtering.Conditions;
using VpnSDK.TrafficOptimizer;
using VpnSDK.TrafficOptimizer.Interfaces;

namespace VpnSDK.Internal.Managers;

internal class ZorgWFPManager : INetworkFilteringManager, IDisposable
{
	private const uint DefaultAdapterBestMetric = 1u;

	private readonly WFPManager _wfp;

	private readonly NetworkFilter _allowDns;

	private readonly NetworkFilter _allowIPv4Lan;

	private readonly NetworkFilter _allowIPv6Lan;

	private readonly NetworkFilter _allowVpnAdapterTraffic;

	private readonly NetworkFilter _preventNonVPNTraffic;

	private readonly NetworkFilter _preventDnsLeak;

	private readonly NetworkFilter _preventIpv6Leak;

	private readonly NetworkFilter _allowWhitelistIp;

	private readonly ILogger _logger;

	private readonly SDKConfiguration _sdkConfiguration;

	private readonly VpnSDK.DnsResolver.DnsResolver _dnsResolver;

	private readonly INetFilterManager _netFilterManager;

	private readonly ITrafficOptimizer _trafficOptimizer;

	private readonly IDnsMonitoring _dnsMonitoring;

	private readonly short _defaultDnsPort = 53;

	private readonly NetworkFilter _preventIpv6ApplicationLeak;

	private readonly NetFilterInterop.NF_RULE_EX _filterDns;

	private readonly NetFilterInterop.NF_RULE_EX _passVpnClientDns;

	private readonly NetFilterInterop.NF_RULE_EX _passloopbackTraffic;

	private readonly NetFilterInterop.NF_RULE_EX _filterNetworkTraffic;

	private NetworkFilter _allowWhitelistIpWhenKillSwitchIsOn;

	private NetworkFilter _ipLoopback;

	private NetworkFilter _loopback;

	private NetworkFilter _interfaceLoopback;

	private NetworkFilter _allowVpnClient;

	private NetworkFilter _allowOpenVpn;

	private NetworkFilter _preventVpnDnsLeak;

	private List<NetworkFilter> _allowSplitTunnelExcludedAppsToBypassVpn = new List<NetworkFilter>();

	private bool _isDisposed;

	private bool _isSplitTunnelEnabled;

	private List<SplitTunnelApp> _splitTunnelAllowedApps = new List<SplitTunnelApp>();

	private bool _allowLanOnKillswitch;

	private bool _killSwitchEnabled;

	private bool _allowDnsQueries;

	private ConnectionStatus _lastKnownConnectionStatus;

	public bool AllowLanConnectivity
	{
		get
		{
			if (!_isDisposed)
			{
				return _allowLanOnKillswitch;
			}
			return false;
		}
		set
		{
			if (_isDisposed)
			{
				return;
			}
			_allowLanOnKillswitch = value;
			if (_lastKnownConnectionStatus == ConnectionStatus.Connected)
			{
				return;
			}
			using (_wfp.StartTransaction())
			{
				if (value)
				{
					_wfp.Filters.DistinctAddRange(_allowIPv4Lan, _allowIPv6Lan);
				}
				else
				{
					_wfp.Filters.RemoveAll(_allowIPv4Lan, _allowIPv6Lan);
				}
			}
		}
	}

	public bool DisableDNSLeakProtection { get; set; }

	public bool DisableIPv6LeakProtection { get; set; }

	public bool AllowLocalAdaptersWhenConnected { get; set; }

	public bool KillSwitchEnabled
	{
		get
		{
			if (!_isDisposed)
			{
				return _killSwitchEnabled;
			}
			return false;
		}
		set
		{
			if (_isDisposed)
			{
				return;
			}
			_killSwitchEnabled = value;
			using (_wfp.StartTransaction())
			{
				if (value)
				{
					_wfp.Filters.DistinctAdd(_preventNonVPNTraffic);
				}
				else
				{
					_wfp.Filters.Remove(_preventNonVPNTraffic);
				}
			}
			if (!value)
			{
				AllowLanConnectivity = false;
			}
		}
	}

	public IDnsRequestResolver DnsRequestResolver { get; set; }

	public bool IsSplitTunnelEnabled
	{
		get
		{
			return _isSplitTunnelEnabled;
		}
		set
		{
			_isSplitTunnelEnabled = value;
		}
	}

	public SplitTunnelMode SplitTunnelMode { get; set; }

	public List<SplitTunnelDomain> SplitTunnelAllowedDomains { get; set; } = new List<SplitTunnelDomain>();

	public List<SplitTunnelApp> SplitTunnelAllowedApps
	{
		get
		{
			return _splitTunnelAllowedApps;
		}
		set
		{
			_splitTunnelAllowedApps = value;
		}
	}

	public bool AllowDnsQueries
	{
		get
		{
			if (_wfp != null)
			{
				if (_lastKnownConnectionStatus != ConnectionStatus.Connected && _wfp.Filters.Contains(_preventNonVPNTraffic))
				{
					if (_wfp.Filters.Contains(_preventNonVPNTraffic))
					{
						return _wfp.Filters.Contains(_allowDns);
					}
					return false;
				}
				return true;
			}
			return false;
		}
		set
		{
			if (_isDisposed)
			{
				return;
			}
			using (_wfp.StartTransaction())
			{
				if (value)
				{
					if (!_wfp.Filters.Contains(_allowDns))
					{
						_wfp.Filters.DistinctAdd(_allowDns);
					}
				}
				else if (_wfp.Filters.Contains(_allowDns))
				{
					_wfp.Filters.Remove(_allowDns);
				}
			}
		}
	}

	public DnsFilteringMode DnsFilterMode { get; set; }

	public uint? VpnNetworkInterfaceMetricValue { get; set; }

	public ZorgWFPManager(ApiManager apiManager, SDKConfiguration sdkConfiguration)
	{
		ILoggerFactory loggerFactoryInstance = LogProvider.LoggerFactoryInstance;
		_logger = LogProvider.GetLogger("VpnSDK::Filtering");
		WFPEngine engine = new WFPEngine();
		_wfp = new WFPManager(engine, loggerFactoryInstance);
		_dnsResolver = new VpnSDK.DnsResolver.DnsResolver(loggerFactoryInstance);
		_netFilterManager = new NetFilterManager(loggerFactoryInstance, sdkConfiguration.CalloutDriverName);
		_sdkConfiguration = sdkConfiguration;
		_trafficOptimizer = new AppTrafficOptimizer(loggerFactoryInstance)
		{
			SynchronizationContext = sdkConfiguration.SynchronizationContext
		};
		_dnsMonitoring = new DnsMonitoring(loggerFactoryInstance)
		{
			SynchronizationContext = sdkConfiguration.SynchronizationContext
		};
		_dnsMonitoring.AddException(_sdkConfiguration.ApiBaseUrls);
		FeatureFactory.Register(_trafficOptimizer);
		FeatureFactory.Register(_dnsMonitoring);
		_netFilterManager.AddNetFilterEventHandler(_trafficOptimizer as NetFilterEventHandlerBase);
		_netFilterManager.AddNetFilterEventHandler(_dnsMonitoring as NetFilterEventHandlerBase);
		SubscribeDnsResolverEvents();
		string engineName = GetEngineName();
		_wfp.StartGeneralEngine(engineName);
		_allowDns = new NetworkFilter("Allow DNS Globally", new PortCondition(52, 53))
		{
			CanOverride = true,
			Weight = FilterWeight.Priority
		};
		_allowWhitelistIp = new NetworkFilter("Allow VPN Server Traffic")
		{
			Weight = FilterWeight.Priority,
			CanOverride = true
		};
		_allowWhitelistIpWhenKillSwitchIsOn = new NetworkFilter("Allow Split Tunnel Domain Traffic When KS Is ON")
		{
			Weight = FilterWeight.Priority,
			CanOverride = true
		};
		_allowVpnAdapterTraffic = new NetworkFilter("Allow VPN Traffic")
		{
			Weight = FilterWeight.High,
			CanOverride = false
		};
		FilterCondition[] privateIPv4Range = ConditionTemplates.PrivateIPv4Range;
		_allowIPv4Lan = new NetworkFilter("Allow IPv4 LAN Traffic", privateIPv4Range)
		{
			Weight = FilterWeight.Low
		};
		privateIPv4Range = ConditionTemplates.PrivateIPv6Range;
		_allowIPv6Lan = new NetworkFilter("Allow IPv6 LAN Traffic", privateIPv4Range)
		{
			Weight = FilterWeight.Low
		};
		_preventIpv6ApplicationLeak = new NetworkFilter("Deny VPN Client IPv6 Traffic", new IPAddressCondition(IPAddress.IPv6Any, 0), new ApplicationCondition(VpnSDK.Core.Helpers.PathHelper.GetApplicationFilePath()))
		{
			Allow = false,
			Weight = FilterWeight.Priority,
			AddressFamily = IPAddressFamily.IPv6,
			CanOverride = true
		};
		_preventDnsLeak = new NetworkFilter("Deny Non-VPN DNS Traffic", new PortCondition(52, 53), new ProtocolCondition(IPProtocol.UDP))
		{
			Allow = false,
			Weight = FilterWeight.High,
			CanOverride = true
		};
		_preventVpnDnsLeak = new NetworkFilter("Deny VPN DNS Traffic", new PortCondition(52, 53), new ProtocolCondition(IPProtocol.UDP))
		{
			Allow = false,
			Weight = FilterWeight.High,
			CanOverride = true
		};
		_preventIpv6Leak = new NetworkFilter("Deny Non-VPN IPv6 Traffic", new IPAddressCondition(IPAddress.IPv6Any, 0))
		{
			Allow = false,
			Weight = FilterWeight.Lowest,
			AddressFamily = IPAddressFamily.IPv6
		};
		_preventNonVPNTraffic = new NetworkFilter("Deny Non-VPN Traffic")
		{
			Weight = FilterWeight.Lowest,
			Allow = false,
			CanOverride = true,
			AddressFamily = IPAddressFamily.All
		};
		using (_wfp.StartTransaction())
		{
			_loopback = new NetworkFilter("Allow Loopback", new LoopbackCondition())
			{
				AddressFamily = IPAddressFamily.All,
				Weight = FilterWeight.Priority,
				CanOverride = true
			};
			_interfaceLoopback = new NetworkFilter("Allow Interface Loopback", new InterfaceCondition(InterfaceType.Loopback))
			{
				AddressFamily = IPAddressFamily.All,
				Weight = FilterWeight.Priority,
				CanOverride = true
			};
			_ipLoopback = new NetworkFilter("Allow Loopback via 127.0.0.0/8", new IPAddressCondition(IPAddress.Parse("127.0.0.0"), 8))
			{
				AddressFamily = IPAddressFamily.IPv4,
				CanOverride = true,
				Weight = FilterWeight.Priority
			};
			_allowVpnClient = new NetworkFilter("Allow VPN Client", new ApplicationCondition(VpnSDK.Core.Helpers.PathHelper.GetApplicationFilePath()))
			{
				CanOverride = true,
				Weight = FilterWeight.Priority
			};
			ApplyOpenVpnNetworkFilter();
			string text = Path.Combine(VpnSDK.Core.Helpers.PathHelper.GetApplicationDirectory(), "WireGuard", "VpnHostService.exe");
			if (File.Exists(text))
			{
				_wfp.Filters.Add(new NetworkFilter("Allow WireGuard", new ApplicationCondition(text))
				{
					Allow = true,
					Weight = FilterWeight.Priority,
					CanOverride = true
				});
			}
			_wfp.Filters.AddRange(_loopback, _allowIPv4Lan, _allowIPv6Lan, _allowVpnClient, _interfaceLoopback, _ipLoopback);
		}
		_filterDns = new NetFilterInterop.NF_RULE_EX
		{
			direction = 2,
			remotePort = (ushort)IPAddress.HostToNetworkOrder(_defaultDnsPort),
			filteringFlag = 2u
		};
		_passVpnClientDns = new NetFilterInterop.NF_RULE_EX
		{
			processName = VpnSDK.Core.Helpers.PathHelper.GetApplicationFilePath(),
			direction = 2,
			remotePort = (ushort)IPAddress.HostToNetworkOrder((short)53),
			filteringFlag = 0u
		};
		_passloopbackTraffic = new NetFilterInterop.NF_RULE_EX
		{
			filteringFlag = 0u,
			ip_family = 2,
			remoteIpAddress = IPAddress.Loopback.GetAddressBytes(),
			remoteIpAddressMask = IPAddress.Parse("255.0.0.0").GetAddressBytes()
		};
		_filterNetworkTraffic = new NetFilterInterop.NF_RULE_EX
		{
			filteringFlag = 512u
		};
		SubscribeToDnsMonitoringEvent();
		if (_netFilterManager != null)
		{
			IConfig config = _dnsMonitoring.Config;
			if (config != null && config.IsEnabled)
			{
				_dnsMonitoring.Start();
			}
		}
	}

	private void ApplyOpenVpnNetworkFilter()
	{
		if (_sdkConfiguration.OpenVpnConfiguration != null)
		{
			string text = Path.Combine(_sdkConfiguration.OpenVpnConfiguration.OpenVpnDirectory, _sdkConfiguration.OpenVpnConfiguration.OpenVpnExecutableFileName);
			_logger?.LogInformation("OpenVPN assembly path is " + text);
			if (File.Exists(text))
			{
				_allowOpenVpn = new NetworkFilter("Allow OpenVPN", new ApplicationCondition(text))
				{
					Allow = true,
					Weight = FilterWeight.Priority,
					CanOverride = true
				};
				_wfp.Filters.Add(_allowOpenVpn);
				_logger?.LogInformation("Filter added for OpenVPN");
			}
			else
			{
				_logger?.LogWarning("OpenVPN executable can't be found. No OpenVPN filter added.");
			}
		}
	}

	public void AddAddressToWhitelist(IPAddress address)
	{
		if (!_isDisposed && !_allowWhitelistIp.Conditions.OfType<IPAddressCondition>().Any((IPAddressCondition x) => x.Address.Equals(address)))
		{
			using (_wfp.StartTransaction())
			{
				_allowWhitelistIp.Conditions.Add(new IPAddressCondition(address));
				_wfp.Filters.DistinctAdd(_allowWhitelistIp);
			}
		}
	}

	public void UpdateOpenVpnNetworkFilter()
	{
		using (_wfp.StartTransaction())
		{
			if (_wfp.Filters.Contains(_allowOpenVpn))
			{
				_wfp.Filters.Remove(_allowOpenVpn);
			}
			ApplyOpenVpnNetworkFilter();
		}
	}

	public ICalloutDriverService GetCalloutDriverService()
	{
		return _netFilterManager.GetCalloutDriverService();
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			IConfig config = _dnsMonitoring.Config;
			if (config != null && config.IsEnabled)
			{
				_dnsMonitoring.Stop();
			}
			if (_netFilterManager != null && _netFilterManager.IsReady)
			{
				_netFilterManager.StopNetFilterEngine();
			}
			UnSubscribeDnsResolverEvents();
			UnSubscribeToDnsMonitoringEvent();
			DisposeFilters();
			try
			{
				_wfp?.Dispose();
			}
			catch (Exception exception)
			{
				_logger?.LogWarning(exception, "Error while disposing the WFP manager");
			}
			_isDisposed = true;
		}
	}

	private void SubscribeDnsResolverEvents()
	{
		if (_dnsResolver != null)
		{
			_dnsResolver.SplitTunnelDomainResolved += DnsResolver_SplitTunnelDomainResolved;
			_dnsResolver.SplitTunnelDomainDnsChanged += DnsResolver_SplitTunnelDomainDnsChanged;
		}
	}

	private void UnSubscribeDnsResolverEvents()
	{
		if (_dnsResolver != null)
		{
			_dnsResolver.SplitTunnelDomainResolved -= DnsResolver_SplitTunnelDomainResolved;
			_dnsResolver.SplitTunnelDomainDnsChanged -= DnsResolver_SplitTunnelDomainDnsChanged;
		}
	}

	private void SubscribeToDnsMonitoringEvent()
	{
		if (_dnsMonitoring != null)
		{
			_dnsMonitoring.DnsMonitoringStarted += DnsMonitoring_Started;
			_dnsMonitoring.DnsMonitoringStopped += DnsMonitoring_Stopped;
		}
	}

	private void UnSubscribeToDnsMonitoringEvent()
	{
		if (_dnsMonitoring != null)
		{
			_dnsMonitoring.DnsMonitoringStarted -= DnsMonitoring_Started;
			_dnsMonitoring.DnsMonitoringStopped -= DnsMonitoring_Stopped;
		}
	}

	private void DnsResolver_SplitTunnelDomainResolved(object sender, SplitTunnelDomainResolvedEventArgs e)
	{
		if (_isDisposed)
		{
			return;
		}
		try
		{
			using (_wfp.StartTransaction())
			{
				AddAddressesToWhitelistIpWhenKillSwitchIsOn(e.IpAddresses);
				_wfp.Filters.DistinctAdd(_allowWhitelistIpWhenKillSwitchIsOn);
			}
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Error adding IP addresses to whitelist when kill switch is on.");
		}
	}

	private void DnsResolver_SplitTunnelDomainDnsChanged(object sender, SplitTunnelDomainDnsChangedEventArgs e)
	{
		if (_isDisposed)
		{
			return;
		}
		try
		{
			using (_wfp.StartTransaction())
			{
				RemoveAddressesFromWhitelistIpWhenKillSwitchIsOn(e.OldIpAddresses);
				AddAddressesToWhitelistIpWhenKillSwitchIsOn(e.NewIpAddresses);
				_wfp.Filters.DistinctAdd(_allowWhitelistIpWhenKillSwitchIsOn);
			}
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Error updating IP address whitelist when kill switch is on.");
		}
	}

	private void DnsMonitoring_Started(DnsMonitoringConfig args)
	{
		if (_dnsMonitoring == null || _netFilterManager == null)
		{
			return;
		}
		try
		{
			if (_netFilterManager.StartNetFilterEngine() && _netFilterManager.AddRules(new List<NetFilterInterop.NF_RULE_EX> { _filterDns, _passVpnClientDns }, NetFilterFeature.DnsMonitoring, NetFilterInterop.NF_APPEND.NF_HEAD) == NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS)
			{
				_logger?.LogInformation("DNS monitoring rules successfully applied.");
			}
			else
			{
				_logger?.LogError("Failed to apply DNS monitoring rules");
			}
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Error while starting the DNS monitoring.");
		}
	}

	private void DnsMonitoring_Stopped(DnsMonitoringConfig args)
	{
		if (_dnsMonitoring == null || _netFilterManager == null)
		{
			return;
		}
		try
		{
			_netFilterManager.ClearRules(NetFilterFeature.DnsMonitoring);
			_logger?.LogInformation("DNS monitoring rules deleted");
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Error while stopping the DNS monitoring.");
		}
	}

	private void AddAddressesToWhitelistIpWhenKillSwitchIsOn(IPAddress[] ipAddresses)
	{
		List<IPAddress> list = ipAddresses.Except(from c in _allowWhitelistIpWhenKillSwitchIsOn.Conditions.OfType<IPAddressCondition>()
			select c.Address).ToList();
		if (list.Count > 0)
		{
			ConditionCollection conditions = _allowWhitelistIpWhenKillSwitchIsOn.Conditions;
			FilterCondition[] items = list.Select((IPAddress ip) => new IPAddressCondition(ip)).ToArray();
			conditions.AddRange(items);
		}
	}

	private void RemoveAddressesFromWhitelistIpWhenKillSwitchIsOn(IPAddress[] ipAddresses)
	{
		List<IPAddress> list = ipAddresses.Intersect(from c in _allowWhitelistIpWhenKillSwitchIsOn.Conditions.OfType<IPAddressCondition>()
			select c.Address).ToList();
		if (list.Count <= 0)
		{
			return;
		}
		foreach (IPAddress address in list)
		{
			IPAddressCondition iPAddressCondition = _allowWhitelistIpWhenKillSwitchIsOn.Conditions.OfType<IPAddressCondition>().FirstOrDefault((IPAddressCondition c) => c.Address.Equals(address));
			if (iPAddressCondition != null)
			{
				_allowWhitelistIpWhenKillSwitchIsOn.Conditions.Remove(iPAddressCondition);
			}
		}
	}

	private void DisposeFilters()
	{
		List<NetworkFilter> list = new List<NetworkFilter>
		{
			_allowDns, _allowIPv4Lan, _allowIPv6Lan, _allowVpnAdapterTraffic, _allowWhitelistIp, _allowWhitelistIpWhenKillSwitchIsOn, _preventDnsLeak, _preventVpnDnsLeak, _preventIpv6Leak, _preventIpv6ApplicationLeak,
			_preventNonVPNTraffic, _ipLoopback, _loopback, _allowVpnClient, _interfaceLoopback, _allowOpenVpn
		};
		if (_allowSplitTunnelExcludedAppsToBypassVpn != null && _allowSplitTunnelExcludedAppsToBypassVpn.Any())
		{
			list.AddRange(_allowSplitTunnelExcludedAppsToBypassVpn);
		}
		using (_wfp.StartTransaction())
		{
			_wfp.Filters.Clear();
			foreach (NetworkFilter item in list)
			{
				try
				{
					item?.Dispose();
				}
				catch (Exception exception)
				{
					_logger?.LogWarning(exception, "Error while disposing a network filter with name " + item?.GetType().Name);
				}
			}
		}
	}

	public void HandleConnectionStatus(ConnectionStatus status)
	{
		if (_isDisposed || _wfp == null || _lastKnownConnectionStatus == status)
		{
			return;
		}
		_lastKnownConnectionStatus = status;
		try
		{
			switch (status)
			{
			case ConnectionStatus.Connecting:
				OnConnecting();
				break;
			case ConnectionStatus.Connected:
				OnConnected();
				break;
			case ConnectionStatus.Disconnecting:
				OnDisconnecting();
				break;
			case ConnectionStatus.Disconnected:
				OnDisconnected();
				break;
			}
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Unable to handle updating Network Filtering.");
		}
	}

	public void EnableSplitTunneling()
	{
		try
		{
			if (SplitTunnelAllowedDomains.Any())
			{
				StartDnsResolver();
			}
			if (SplitTunnelAllowedApps.Any())
			{
				IPAddress localIpAddress = NetworkInterfaceUtility.GetLocalIpAddress();
				if (localIpAddress == null)
				{
					_logger?.LogInformation("Failed to retrieve local ip address.");
					return;
				}
				_logger?.LogDebug($"The local IP address obtained is {localIpAddress}");
				_netFilterManager.StartNetFilterEngine();
				AddAppBasedSplitTunnelFilters(localIpAddress);
				_logger?.LogInformation("App based split tunnel engine started.");
			}
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Error while enabling split tunneling.");
		}
	}

	public void DisableSplitTunneling()
	{
		if (SplitTunnelAllowedDomains.Any())
		{
			_dnsResolver.StopResolver();
		}
		if (!SplitTunnelAllowedApps.Any() || !_netFilterManager.IsReady)
		{
			return;
		}
		if (KillSwitchEnabled)
		{
			using (_wfp.StartTransaction())
			{
				ClearExcludedApps();
			}
		}
		_netFilterManager.ClearSplitTunneling();
		_logger?.LogInformation("Split tunnel app drivers has been stopped.");
	}

	public bool StopSplitTunnelDriverService(bool waitForStop)
	{
		if (_netFilterManager == null)
		{
			return false;
		}
		_netFilterManager.StopCalloutDriverService(waitForStop);
		return true;
	}

	public bool UninstallSplitTunnelDriverService(bool waitForStop)
	{
		if (_netFilterManager == null)
		{
			return false;
		}
		return _netFilterManager.UninstallSplitTunnelDriverService(waitForStop);
	}

	private void ClearExcludedApps()
	{
		if (_allowSplitTunnelExcludedAppsToBypassVpn == null || !_allowSplitTunnelExcludedAppsToBypassVpn.Any())
		{
			return;
		}
		foreach (NetworkFilter item in _allowSplitTunnelExcludedAppsToBypassVpn)
		{
			_wfp.Filters.Remove(item);
		}
		_allowSplitTunnelExcludedAppsToBypassVpn.Clear();
	}

	private void AddAllowedAppsToFiltersWhenKSIsEnabled()
	{
		if (SplitTunnelAllowedApps == null || !SplitTunnelAllowedApps.Any())
		{
			return;
		}
		foreach (SplitTunnelApp splitTunnelAllowedApp in SplitTunnelAllowedApps)
		{
			try
			{
				if (splitTunnelAllowedApp.Path.IndexOf("*", StringComparison.Ordinal) > -1)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(splitTunnelAllowedApp.Path));
					if (directoryInfo.Exists)
					{
						string fileName = Path.GetFileName(splitTunnelAllowedApp.Path);
						FileInfo[] files = directoryInfo.GetFiles(fileName);
						foreach (FileInfo fileInfo in files)
						{
							AddAllowedAppToFilter(fileInfo.Name, fileInfo.FullName);
						}
					}
				}
				else
				{
					AddAllowedAppToFilter(splitTunnelAllowedApp.Name, splitTunnelAllowedApp.Path);
				}
			}
			catch (Exception arg)
			{
				_logger?.LogError($"Failed to add app allowed filter for {splitTunnelAllowedApp.Name} with path {splitTunnelAllowedApp.Path} when KS is active: {arg}");
			}
		}
	}

	private void AddAllowedAppToFilter(string applicationName, string applicationPath)
	{
		NetworkFilter item = new NetworkFilter("Allow " + applicationName + " to bypass VPN", new ApplicationCondition(applicationPath))
		{
			CanOverride = true,
			Weight = FilterWeight.Priority
		};
		_allowSplitTunnelExcludedAppsToBypassVpn.Add(item);
		_wfp.Filters.Add(item);
	}

	private void StartDnsResolver()
	{
		List<IPAddress> list = new List<IPAddress>();
		List<IPAddress> list2 = new List<IPAddress>();
		NetworkInterface connectionNetworkInterface = GetConnectionNetworkInterface();
		if (connectionNetworkInterface == null)
		{
			_logger?.LogWarning("The current network interface cannot be retrieved. DNS Resolver failed to start.");
			return;
		}
		foreach (IPAddress dnsAddress in connectionNetworkInterface.GetIPProperties().DnsAddresses)
		{
			if (!IPAddress.IsLoopback(dnsAddress))
			{
				list2.Add(dnsAddress);
			}
		}
		NetworkInterface localInterface = NetworkInterfaceUtility.GetLocalInterface();
		if (localInterface == null)
		{
			_logger?.LogInformation("The local network interface cannot be retrieved. DNS Resolver failed to start.");
			return;
		}
		foreach (IPAddress dnsAddress2 in localInterface.GetIPProperties().DnsAddresses)
		{
			if (dnsAddress2.AddressFamily == AddressFamily.InterNetworkV6)
			{
				_logger?.LogInformation("Excluded InterNetworkV6 DNS address from local DNS address list sent to DNS resolver.");
			}
			else
			{
				list.Add(dnsAddress2);
			}
		}
		_logger?.LogInformation($"DNS servers sent to resolver: Local [{list.Count}], VPN [{list2.Count}]");
		_dnsResolver.StartResolver(DnsRequestResolver, SplitTunnelAllowedDomains, list, list2, KillSwitchEnabled);
	}

	private void AddAppBasedSplitTunnelFilters(IPAddress ipAddress)
	{
		foreach (SplitTunnelApp splitTunnelAllowedApp in SplitTunnelAllowedApps)
		{
			try
			{
				string text = (_netFilterManager.AddSplitTunneledApplication(splitTunnelAllowedApp.Path, ipAddress) ? "Success" : "failed");
				_logger?.LogInformation("Added app based exclude filters for " + splitTunnelAllowedApp.Name + " with path " + splitTunnelAllowedApp.Path + ". Status: " + text);
			}
			catch (Exception ex)
			{
				_logger?.LogError("Failed to create app exclude filter for " + splitTunnelAllowedApp.Name + " with path " + splitTunnelAllowedApp.Path + ": " + ex.ToString());
			}
		}
		if (KillSwitchEnabled)
		{
			using (_wfp.StartTransaction())
			{
				ClearExcludedApps();
				AddAllowedAppsToFiltersWhenKSIsEnabled();
			}
		}
	}

	private NetworkInterface GetConnectionNetworkInterface()
	{
		NetworkInterface result = null;
		NetworkInterface[] allNetworkInterfacesSafely = NetworkInterfaceHelper.GetAllNetworkInterfacesSafely();
		if (allNetworkInterfacesSafely.Length == 0)
		{
			_logger?.LogWarning("No network interfaces found. Ensure the system has active network adapters.");
			return null;
		}
		NetworkAdapterInfo[] array = null;
		if (_sdkConfiguration.OpenVpnConfiguration != null)
		{
			array = NetworkInterfaceHelper.GetAllAdaptersFromRegistry();
		}
		foreach (NetworkInterface networkInterface in allNetworkInterfacesSafely.Where((NetworkInterface x) => x.OperationalStatus == OperationalStatus.Up))
		{
			if (_sdkConfiguration.RasConfiguration != null && networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ppp && networkInterface.Description.StartsWith(_sdkConfiguration.RasConfiguration.RasDeviceDescription))
			{
				result = networkInterface;
				break;
			}
			if (_sdkConfiguration.OpenVpnConfiguration != null)
			{
				IEnumerable<NetworkAdapterInfo> enumerable = array?.Where((NetworkAdapterInfo adapter) => adapter.ComponentId == _sdkConfiguration.OpenVpnConfiguration.InstalledTapDriver.ToString());
				if (enumerable != null && enumerable.Any((NetworkAdapterInfo adapter) => networkInterface.Id.Equals(adapter.InstanceId, StringComparison.OrdinalIgnoreCase)))
				{
					result = networkInterface;
					break;
				}
			}
			if (!string.IsNullOrEmpty(_sdkConfiguration.WireguardConfiguration?.TunDeviceDescription) && !string.IsNullOrEmpty(_sdkConfiguration.WireguardConfiguration?.ConnectionName) && (networkInterface.Description.StartsWith(_sdkConfiguration.WireguardConfiguration.TunDeviceDescription, StringComparison.OrdinalIgnoreCase) || networkInterface.Description.StartsWith("Wintun")) && networkInterface.Name.StartsWith(_sdkConfiguration.WireguardConfiguration.ConnectionName))
			{
				result = networkInterface;
				break;
			}
		}
		return result;
	}

	private void OnConnecting()
	{
		if (KillSwitchEnabled)
		{
			using (_wfp.StartTransaction())
			{
				_wfp.Filters.DistinctAdd(_allowDns);
			}
		}
	}

	private bool CanApplySplitTunnelingSettings()
	{
		if (IsSplitTunnelEnabled)
		{
			if (!SplitTunnelAllowedDomains.Any())
			{
				return SplitTunnelAllowedApps.Any();
			}
			return true;
		}
		return false;
	}

	private void OnConnected()
	{
		IDnsMonitoring dnsMonitoring = _dnsMonitoring;
		if (dnsMonitoring != null && dnsMonitoring.Config?.IsEnabled == true)
		{
			_dnsMonitoring.Stop();
		}
		if (_netFilterManager != null && CanApplySplitTunnelingSettings())
		{
			if (AllowLocalAdaptersWhenConnected)
			{
				EnableSplitTunneling();
			}
			else
			{
				_logger?.LogInformation("Unable to split tunnel traffic as allow local adapters are blocked.");
			}
		}
		if (_netFilterManager != null)
		{
			IConfig config = _trafficOptimizer.Config;
			if (config != null && config.IsEnabled)
			{
				_netFilterManager.StartNetFilterEngine();
				if (_netFilterManager.AddRules(new List<NetFilterInterop.NF_RULE_EX> { _passloopbackTraffic, _filterNetworkTraffic }, NetFilterFeature.AppTrafficOptimizer, NetFilterInterop.NF_APPEND.NF_TAIL) == NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS)
				{
					_trafficOptimizer.Start();
					_logger?.LogInformation("Application Traffic Shaping started.");
				}
				else
				{
					_netFilterManager.ClearRules(NetFilterFeature.AppTrafficOptimizer);
					_logger?.LogError("Failed to apply rules for Traffic Shaping.");
				}
			}
		}
		if (KillSwitchEnabled && _wfp.Filters.Contains(_allowDns))
		{
			using (_wfp.StartTransaction())
			{
				_wfp.Filters.Remove(_allowDns);
			}
		}
		NetworkInterface connectionNetworkInterface = GetConnectionNetworkInterface();
		if (connectionNetworkInterface == null)
		{
			_logger?.LogWarning("The current network interface cannot be retrieved. WFP filters could not be applied.");
		}
		else if (!_wfp.Filters.Contains(_allowVpnAdapterTraffic))
		{
			_logger?.LogInformation("VPN adapter found: {InterfaceName}", connectionNetworkInterface.Name);
			NetworkInterop.FlushDnsCache();
			using (_wfp.StartTransaction())
			{
				if (!AllowLocalAdaptersWhenConnected)
				{
					_wfp.Filters.DistinctAdd(_preventNonVPNTraffic);
					_wfp.Filters.RemoveAll(_allowIPv4Lan, _allowIPv6Lan);
				}
				else
				{
					_wfp.Filters.DistinctAddRange(_allowIPv4Lan, _allowIPv6Lan);
				}
				_allowVpnAdapterTraffic.Conditions.RemoveAllOfType(typeof(InterfaceCondition));
				_allowVpnAdapterTraffic.Conditions.Add(new InterfaceCondition(connectionNetworkInterface));
				_wfp.Filters.DistinctAdd(_allowVpnAdapterTraffic);
				if (IsSplitTunnelEnabled && SplitTunnelAllowedDomains.Any())
				{
					_logger?.LogInformation("Applying filter to forward all DNS request to local DNS resolver");
					_preventDnsLeak.Conditions.RemoveAllOfType(typeof(InterfaceCondition));
					_preventDnsLeak.Conditions.Add(new InterfaceCondition(connectionNetworkInterface)
					{
						MatchType = ConditionMatchType.NotEqual
					});
					_preventVpnDnsLeak.Conditions.RemoveAllOfType(typeof(InterfaceCondition));
					_preventVpnDnsLeak.Conditions.Add(new InterfaceCondition(connectionNetworkInterface)
					{
						MatchType = ConditionMatchType.Equal
					});
					_wfp.Filters.DistinctAdd(_preventVpnDnsLeak);
					_wfp.Filters.DistinctAdd(_preventDnsLeak);
				}
				else if (!DisableDNSLeakProtection)
				{
					_logger?.LogTrace("Applying DNS leak prevention. VPNAdapter={VpnAdapter}", connectionNetworkInterface.Description);
					_preventDnsLeak.Conditions.RemoveAllOfType(typeof(InterfaceCondition));
					_preventDnsLeak.Conditions.Add(new InterfaceCondition(connectionNetworkInterface)
					{
						MatchType = ConditionMatchType.NotEqual
					});
					_wfp.Filters.DistinctAdd(_preventDnsLeak);
				}
				if (!DisableIPv6LeakProtection)
				{
					_logger?.LogTrace("Applying IPv6 leak prevention. VPNAdapter={VpnAdapter}", connectionNetworkInterface.Description);
					_preventIpv6Leak.Conditions.RemoveAllOfType(typeof(InterfaceCondition));
					_preventIpv6Leak.Conditions.Add(new InterfaceCondition(connectionNetworkInterface)
					{
						MatchType = ConditionMatchType.NotEqual
					});
					_wfp.Filters.DistinctAddRange(_preventIpv6Leak, _preventIpv6ApplicationLeak);
				}
			}
			SetBestMetricForNetworkInterface(connectionNetworkInterface);
		}
		else
		{
			_logger?.LogWarning("WFP filters not applied as AllowVpnAdapterTraffic filter is already present.");
		}
	}

	private void SetBestMetricForNetworkInterface(NetworkInterface currentNetworkInterface)
	{
		uint num = VpnNetworkInterfaceMetricValue ?? 1;
		_logger?.LogInformation($"Setting interface metric to {num} for faster DNS resolution.");
		for (int i = 1; i <= 3; i++)
		{
			try
			{
				currentNetworkInterface.SetMetric(num);
				return;
			}
			catch (Exception exception)
			{
				_logger?.LogWarning(exception, $"Failed to set lower metric for network interface. Attempt {i} of {3} failed.", i, 3);
				Thread.Sleep(10);
			}
		}
		_logger?.LogWarning($"Unable to set metric to {num} after multiple attempts.");
	}

	private void OnDisconnecting()
	{
	}

	private void OnDisconnected()
	{
		if (AllowLocalAdaptersWhenConnected && CanApplySplitTunnelingSettings())
		{
			DisableSplitTunneling();
		}
		_trafficOptimizer.Stop();
		_netFilterManager.ClearRules(NetFilterFeature.AppTrafficOptimizer);
		using (_wfp.StartTransaction(ignoreFilterProcessException: true))
		{
			_logger?.LogTrace("Removing network filters.");
			if (KillSwitchEnabled && IsSplitTunnelEnabled && SplitTunnelAllowedDomains.Any())
			{
				_wfp.Filters.Remove(_allowWhitelistIpWhenKillSwitchIsOn);
			}
			if (!KillSwitchEnabled && _wfp.Filters.Contains(_preventNonVPNTraffic))
			{
				_wfp.Filters.Remove(_preventNonVPNTraffic);
			}
			if (KillSwitchEnabled && _wfp.Filters.Contains(_allowDns))
			{
				_wfp.Filters.Remove(_allowDns);
			}
			if (KillSwitchEnabled && !AllowLanConnectivity)
			{
				_wfp.Filters.RemoveAll(_allowIPv4Lan, _allowIPv6Lan);
			}
			else
			{
				_wfp.Filters.DistinctAddRange(_allowIPv4Lan, _allowIPv6Lan);
			}
			_wfp.Filters.RemoveAll(_allowVpnAdapterTraffic, _preventIpv6Leak, _preventDnsLeak, _preventIpv6ApplicationLeak, _preventVpnDnsLeak);
		}
		_logger?.LogTrace("Network filters removed.");
		IDnsMonitoring dnsMonitoring = _dnsMonitoring;
		if (dnsMonitoring != null && dnsMonitoring.Config?.IsEnabled == true)
		{
			_dnsMonitoring.Start();
		}
	}

	private string GetEngineName()
	{
		return $"{_sdkConfiguration.ApplicationName}.{DateTime.Now.Ticks}";
	}
}
