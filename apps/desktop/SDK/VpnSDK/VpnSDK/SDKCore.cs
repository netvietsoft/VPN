using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DotRas;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Polly;
using Polly.Retry;
using UACHelper;
using VpnSDK.Common.Dns;
using VpnSDK.Common.Enums;
using VpnSDK.Common.Helpers;
using VpnSDK.Common.Settings;
using VpnSDK.Core;
using VpnSDK.DTO;
using VpnSDK.DnsMonitor;
using VpnSDK.DnsMonitor.DTO;
using VpnSDK.DnsMonitor.Interfaces;
using VpnSDK.Enums;
using VpnSDK.Extensions;
using VpnSDK.Helpers;
using VpnSDK.Interfaces;
using VpnSDK.Internal;
using VpnSDK.Internal.Configuration;
using VpnSDK.Internal.Dtos;
using VpnSDK.Internal.Extensions;
using VpnSDK.Internal.Helpers;
using VpnSDK.Internal.Managers;
using VpnSDK.Private.API;
using VpnSDK.Private.API.DTO;
using VpnSDK.Private.OpenVpn.Extensions;
using VpnSDK.Private.Ras;
using VpnSDK.TrafficOptimizer;
using VpnSDK.TrafficOptimizer.DTO;
using VpnSDK.TrafficOptimizer.Interfaces;

namespace VpnSDK;

internal class SDKCore : BindableBase, ISDK, INotifyPropertyChanged, IDisposable, ISDKInternal
{
	protected NetworkGeolocation _lastKnownUserLocation;

	private const int NumberOfRetries = 3;

	private const int MaxNumberOfMetadata = 5;

	private const int MaxMetadataKeyLength = 128;

	private const int MaxMetadataValueLength = 1024;

	private const string FilteringString = "filtering";

	private const string PartnerString = "partner";

	private readonly ILogger _logger;

	private readonly ApiManager _zorgApiManager;

	private readonly INetworkFilteringManager _filteringManager;

	private readonly SynchronizationContext _syncContext;

	private readonly System.Timers.Timer _infoRefreshTimer;

	private readonly IDriverVersionProvider _driverVersionProvider;

	private Dictionary<VpnManagerType, IProtocolManager> _protocolManagers = new Dictionary<VpnManagerType, IProtocolManager>();

	private DateTime _lastAdapterChange = DateTime.MinValue;

	private bool _allowOnlyVPNConnectivity;

	private bool _allowLocalAdaptersWhenConnected;

	private bool _disableDNSLeakProtection;

	private bool _disableIPv6LeakProtection;

	private IConnectionInfo _connectionBeforeSuspend;

	private CancellationToken _cancellationToken;

	private bool _allowLANTraffic = true;

	private volatile CancellationTokenSource _ipgeoCancellationTokenSource;

	private volatile CancellationTokenSource _connectionCancellationTokenSource;

	private bool _isConnecting;

	private bool _isDisconnecting;

	private bool _isDisconnected = true;

	private bool _isConnectionCancelled;

	private IConnectionInfo _activeConnectionInformation;

	private IServer _currentServer;

	private TimeSpan _serverRefreshCooldown = TimeSpan.FromMinutes(5.0);

	private DateTime _lastServerListRefreshTime;

	private bool _isFilteringAvailable;

	private bool _isSplitTunnelEnabled;

	private SplitTunnelMode _splitTunnelMode;

	private List<SplitTunnelApp> _splitTunnelAllowedApps = new List<SplitTunnelApp>();

	private List<SplitTunnelDomain> _splitTunnelAllowedDomains = new List<SplitTunnelDomain>();

	private IDnsRequestResolver _dnsRequestResolver;

	private DnsFilteringMode _dnsFilterMode;

	private AsyncRetryPolicy _retryPolicy;

	private IDisposable _getVpnInterfaceDisposable;

	private IDisposable _bandwidthMonitorDisposable;

	private DataTransferEventArgs _lastDataTransferArgs;

	public NetworkGeolocation CurrentNetworkGeolocation { get; internal set; }

	public ReadOnlyObservableCollection<ILocation> Locations { get; internal set; }

	public bool IsDisposed { get; private set; }

	public bool IsElevated { get; internal set; }

	public IUser User
	{
		get
		{
			UserProxy user = _zorgApiManager.User;
			if (user == null || !user.IsValid)
			{
				return null;
			}
			return _zorgApiManager.User;
		}
	}

	public IBrandingInfo BrandingInfo => _zorgApiManager.BrandingInfo;

	public bool IsTapDriverInstalled => (_protocolManagers.Where((KeyValuePair<VpnManagerType, IProtocolManager> p) => p.Key == VpnManagerType.OpenVPN)?.Select((KeyValuePair<VpnManagerType, IProtocolManager> x) => x.Value)?.FirstOrDefault() as IDriverManager)?.IsDriverDetected ?? false;

	public string TapDriverDescription
	{
		get
		{
			ISDKConfiguration configuration = Configuration;
			if (configuration == null)
			{
				return null;
			}
			OpenVpnConfiguration openVpnConfiguration = configuration.OpenVpnConfiguration;
			if (openVpnConfiguration == null)
			{
				return null;
			}
			return openVpnConfiguration.InstalledTapDriver.GetDescription();
		}
	}

	public bool DisableDNSLeakProtection
	{
		get
		{
			if (_isFilteringAvailable)
			{
				return _disableDNSLeakProtection;
			}
			return true;
		}
		set
		{
			if (_isFilteringAvailable)
			{
				SetProperty(ref _disableDNSLeakProtection, value, "DisableDNSLeakProtection");
				_filteringManager.DisableDNSLeakProtection = value;
			}
		}
	}

	public bool DisableIPv6LeakProtection
	{
		get
		{
			if (_isFilteringAvailable)
			{
				return _disableIPv6LeakProtection;
			}
			return true;
		}
		set
		{
			if (_isFilteringAvailable)
			{
				SetProperty(ref _disableIPv6LeakProtection, value, "DisableIPv6LeakProtection");
				_filteringManager.DisableIPv6LeakProtection = value;
			}
		}
	}

	public bool AllowOnlyVPNConnectivity
	{
		get
		{
			if (_isFilteringAvailable)
			{
				return _allowOnlyVPNConnectivity;
			}
			return false;
		}
		set
		{
			if (_isFilteringAvailable)
			{
				SetProperty(ref _allowOnlyVPNConnectivity, value, "AllowOnlyVPNConnectivity");
				_filteringManager.KillSwitchEnabled = value;
			}
		}
	}

	public bool AllowLANTraffic
	{
		get
		{
			if (_isFilteringAvailable)
			{
				return _allowLANTraffic;
			}
			return true;
		}
		set
		{
			if (_isFilteringAvailable)
			{
				SetProperty(ref _allowLANTraffic, value, "AllowLANTraffic");
				_filteringManager.AllowLanConnectivity = value;
			}
		}
	}

	public bool AllowLocalAdaptersWhenConnected
	{
		get
		{
			if (_isFilteringAvailable)
			{
				return _allowLocalAdaptersWhenConnected;
			}
			return true;
		}
		set
		{
			if (_isFilteringAvailable)
			{
				SetProperty(ref _allowLocalAdaptersWhenConnected, value, "AllowLocalAdaptersWhenConnected");
				_filteringManager.AllowLocalAdaptersWhenConnected = value;
			}
		}
	}

	public bool IsConnected => _protocolManagers.Any((KeyValuePair<VpnManagerType, IProtocolManager> x) => x.Value.IsActive);

	public bool IsConnecting
	{
		get
		{
			return _isConnecting;
		}
		internal set
		{
			SetProperty(ref _isConnecting, value, "IsConnecting");
		}
	}

	public bool IsDisconnecting
	{
		get
		{
			return _isDisconnecting;
		}
		internal set
		{
			SetProperty(ref _isDisconnecting, value, "IsDisconnecting");
		}
	}

	public bool IsDisconnected
	{
		get
		{
			return _isDisconnected;
		}
		internal set
		{
			SetProperty(ref _isDisconnected, value, "IsDisconnected");
		}
	}

	public bool IsConnectionCancelled
	{
		get
		{
			return _isConnectionCancelled;
		}
		internal set
		{
			SetProperty(ref _isConnectionCancelled, value, "IsConnectionCancelled");
		}
	}

	public Dictionary<NetworkConnectionType, bool> AvailableProtocols => Configuration.AvailableVpnTypes;

	public IConnectionInfo ActiveConnectionInformation
	{
		get
		{
			return _activeConnectionInformation;
		}
		internal set
		{
			SetProperty(ref _activeConnectionInformation, value, "ActiveConnectionInformation");
		}
	}

	public bool IsSplitTunnelEnabled
	{
		get
		{
			if (!_isFilteringAvailable)
			{
				return false;
			}
			return _isSplitTunnelEnabled;
		}
		set
		{
			if (_isFilteringAvailable)
			{
				SetProperty(ref _isSplitTunnelEnabled, value, "IsSplitTunnelEnabled");
				_filteringManager.IsSplitTunnelEnabled = value;
			}
		}
	}

	public SplitTunnelMode SplitTunnelMode
	{
		get
		{
			if (!_isFilteringAvailable)
			{
				return SplitTunnelMode.Disabled;
			}
			return _splitTunnelMode;
		}
		set
		{
			if (_isFilteringAvailable)
			{
				SetProperty(ref _splitTunnelMode, value, "SplitTunnelMode");
				_filteringManager.SplitTunnelMode = value;
			}
		}
	}

	public List<SplitTunnelApp> SplitTunnelAllowedApps
	{
		get
		{
			return _splitTunnelAllowedApps;
		}
		set
		{
			List<SplitTunnelApp> list = value?.GroupBy((SplitTunnelApp x) => new { x.Name, x.Path }).Select(g => g.First()).ToList();
			SetProperty(ref _splitTunnelAllowedApps, list, "SplitTunnelAllowedApps");
			if (_filteringManager != null)
			{
				_filteringManager.SplitTunnelAllowedApps = list;
			}
		}
	}

	public List<SplitTunnelDomain> SplitTunnelAllowedDomains
	{
		get
		{
			return _splitTunnelAllowedDomains;
		}
		set
		{
			SetProperty(ref _splitTunnelAllowedDomains, value, "SplitTunnelAllowedDomains");
			if (_filteringManager != null)
			{
				_filteringManager.SplitTunnelAllowedDomains = value;
			}
		}
	}

	public IDnsRequestResolver DnsRequestResolver
	{
		get
		{
			if (!_isFilteringAvailable)
			{
				return null;
			}
			return _dnsRequestResolver;
		}
		set
		{
			if (_isFilteringAvailable)
			{
				SetProperty(ref _dnsRequestResolver, value, "DnsRequestResolver");
				_filteringManager.DnsRequestResolver = value;
			}
		}
	}

	public DnsFilteringMode DnsFilterMode
	{
		get
		{
			if (!_isFilteringAvailable)
			{
				return DnsFilteringMode.Disabled;
			}
			return _dnsFilterMode;
		}
		set
		{
			if (_isFilteringAvailable)
			{
				SetProperty(ref _dnsFilterMode, value, "DnsFilterMode");
				_filteringManager.DnsFilterMode = value;
			}
		}
	}

	public uint? VpnNetworkInterfaceMetricValue
	{
		get
		{
			if (!_isFilteringAvailable)
			{
				return null;
			}
			return _filteringManager.VpnNetworkInterfaceMetricValue;
		}
		set
		{
			if (_isFilteringAvailable)
			{
				_filteringManager.VpnNetworkInterfaceMetricValue = value;
				OnPropertyChanged("VpnNetworkInterfaceMetricValue");
			}
		}
	}

	internal static ISDKConfiguration Configuration { get; set; }

	internal static SynchronizationContext SyncContext { get; set; } = SynchronizationContext.Current ?? new SynchronizationContext();

	protected ConnectionStatus CurrentConnectionStatus { get; set; }

	public event SDKChangeEventHandler<ConnectionStatus> VpnConnectionStatusChanged;

	public event SDKEventHandler AuthenticationCredentialsExpired;

	public event SDKEventHandler<ConnectionStatus> BeforeConnectionStateFiltersApplied;

	public event SDKOperationEventHandler<NetworkGeolocation> UserLocationStatusChanged;

	public event SDKEventHandler<AuthenticationStatus> AuthenticationStatusChanged;

	public event SDKOperationEventHandler TapDeviceInstallationStatusChanged;

	public event SDKEventHandler<RefreshLocationListStatus> LocationsRefreshStatusChanged;

	public event SDKEventHandler<ISDKError> ErrorRaised;

	public event EventHandler<IUser> TokensRefreshed;

	public event SDKEventHandler<DataTransferEventArgs> DataTransferUpdate;

	public event Action<TrafficOptimizerArgs> TrafficUpdate
	{
		add
		{
			ITrafficOptimizer feature = FeatureFactory.GetFeature<AppTrafficOptimizer>();
			if (feature != null)
			{
				feature.TrafficUpdate += value;
			}
		}
		remove
		{
			ITrafficOptimizer feature = FeatureFactory.GetFeature<AppTrafficOptimizer>();
			if (feature != null)
			{
				feature.TrafficUpdate -= value;
			}
		}
	}

	public event Action<DnsMonitoringArgs> DnsMonitoringUpdate
	{
		add
		{
			IDnsMonitoring feature = FeatureFactory.GetFeature<DnsMonitoring>();
			if (feature != null)
			{
				feature.DnsMonitoringUpdate += value;
			}
		}
		remove
		{
			IDnsMonitoring feature = FeatureFactory.GetFeature<DnsMonitoring>();
			if (feature != null)
			{
				feature.DnsMonitoringUpdate -= value;
			}
		}
	}

	internal SDKCore(ISDKConfiguration sdkConfiguration)
	{
		_logger = LogProvider.GetLogger("VpnSDK");
		VersionHelper.EnableTls12IfNeeded();
		IsElevated = global::UACHelper.UACHelper.IsElevated;
		if (sdkConfiguration.SynchronizationContext != null)
		{
			_syncContext = sdkConfiguration.SynchronizationContext;
		}
		else
		{
			_syncContext = SynchronizationContext.Current;
			if (_syncContext == null)
			{
				_syncContext = new SynchronizationContext();
				if (Debugger.IsAttached)
				{
					Console.WriteLine("WARNING! No synchronization context has been found or set. All events will be fired on new threads.");
					Console.WriteLine("If VpnSDK is being used in a UI-based application, the SDK must be instantiated on the UI thread.");
				}
			}
		}
		SyncContext = _syncContext;
		if (Debugger.IsAttached && _logger == null)
		{
			Console.WriteLine("WARNING! No log provider detected. Please setup a logging library in your application that is supported by LibLog.");
		}
		Configuration = sdkConfiguration;
		_logger?.LogInformation($"========== {DateTime.UtcNow} ==========");
		_logger?.LogInformation(".NET Framework {Version}", VersionHelper.GetNetFrameworkVersion());
		_logger?.LogInformation("SDK version {Version}", Assembly.GetExecutingAssembly().GetName().Version);
		_logger?.LogInformation("Referenced libraries:");
		Array.ForEach(typeof(SDKCore).Assembly.GetReferencedAssemblies(), delegate(AssemblyName x)
		{
			_logger?.LogInformation($"{x.Name} v{x.Version}");
		});
		_zorgApiManager = ApiManager.GetInstance(sdkConfiguration);
		OpenVpnManager openVpnManager = null;
		if (AvailableProtocols.ContainsKey(NetworkConnectionType.OpenVPN) && AvailableProtocols[NetworkConnectionType.OpenVPN])
		{
			openVpnManager = new OpenVpnManager(_zorgApiManager, sdkConfiguration.OpenVpnConfiguration)
			{
				UnexpectedDisconnect = OnVpnConnectionClosedUnexpectedly
			};
			_protocolManagers.Add(VpnManagerType.OpenVPN, openVpnManager);
			OnPropertyChanged("IsTapDriverInstalled");
		}
		if (AvailableProtocols.ContainsKey(NetworkConnectionType.WireGuard) && AvailableProtocols[NetworkConnectionType.WireGuard])
		{
			_protocolManagers.Add(VpnManagerType.WireGuard, new WireGuardManager(_zorgApiManager, sdkConfiguration.WireguardConfiguration)
			{
				UnexpectedDisconnect = OnVpnConnectionClosedUnexpectedly
			});
		}
		if (AvailableProtocols.Any((KeyValuePair<NetworkConnectionType, bool> x) => x.Key != NetworkConnectionType.OpenVPN && x.Key != NetworkConnectionType.WireGuard && x.Value))
		{
			_protocolManagers.Add(VpnManagerType.RAS, new RasManager(_zorgApiManager, sdkConfiguration.RasConfiguration)
			{
				UnexpectedDisconnect = OnVpnConnectionClosedUnexpectedly
			});
		}
		SystemEvents.PowerModeChanged += SystemEventsOnPowerModeChanged;
		Locations = new ReadOnlyObservableCollection<ILocation>(_zorgApiManager.Locations);
		if (sdkConfiguration.OpenVpnConfiguration == null)
		{
			_logger?.LogWarning("OpenVPN was not configured. The protocol will be disabled.");
		}
		if (sdkConfiguration.RasConfiguration == null)
		{
			_logger?.LogWarning("RAS was not configured. All RAS protocols will be disabled.");
		}
		try
		{
			if (!sdkConfiguration.RunInUserspace && IsElevated)
			{
				_filteringManager = new ZorgWFPManager(_zorgApiManager, sdkConfiguration as SDKConfiguration);
				_isFilteringAvailable = true;
			}
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Network filtering features could not be initialized.");
		}
		_driverVersionProvider = new DriverVersionProvider(_filteringManager?.GetCalloutDriverService(), openVpnManager);
		_infoRefreshTimer = new System.Timers.Timer
		{
			AutoReset = true,
			Enabled = false,
			Interval = TimeSpan.FromMinutes(15.0).TotalMilliseconds
		};
		_infoRefreshTimer.Elapsed += InfoRefreshTimerOnElapsed;
		NetworkChange.NetworkAddressChanged += NetworkChangeOnNetworkAddressChanged;
		_zorgApiManager.LocationsRefreshStatusChanged += OnLocationRefreshStatusChanged;
		_zorgApiManager.OnHttpError += OnHttpError;
		_zorgApiManager.OnProxyError += OnProxyError;
		_zorgApiManager.OnTokenRefresh += OnTokenRefresh;
		InitConnectionRetryPolicy();
	}

	public async Task<IBrandingInfo> GetBrandingInfo(string slug)
	{
		return new BrandingInfoProxy(await _zorgApiManager.GetBrandingInformation(slug, Configuration.BrandingKey).ConfigureAwait(continueOnCapturedContext: false));
	}

	public async Task Login(string username, string password, string slug)
	{
		await UpdateBrandingInformation(slug);
		await Login(username, password).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task Login(string username, string password)
	{
		await PerformLogin(async delegate
		{
			await _zorgApiManager.Login(username, password).ConfigureAwait(continueOnCapturedContext: false);
		});
	}

	public async Task LoginWithAuthTokens(string accessToken, string refreshToken, string slug)
	{
		await UpdateBrandingInformation(slug);
		await LoginWithAuthTokens(accessToken, refreshToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task LoginWithAuthTokens(string accessToken, string refreshToken)
	{
		await PerformLogin(async delegate
		{
			await _zorgApiManager.LoginWithAuthTokens(accessToken, refreshToken).ConfigureAwait(continueOnCapturedContext: false);
		});
	}

	public async Task Logout()
	{
		OnAuthenticationStatusChanged(AuthenticationStatus.InProgress);
		await Disconnect().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await _zorgApiManager.Logout().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Error while logging out the user");
		}
		_zorgApiManager.ClearUser();
		_infoRefreshTimer.Stop();
		_zorgApiManager.Locations.Clear();
		_logger?.LogInformation("User has been logged out.");
		OnAuthenticationStatusChanged(AuthenticationStatus.NotAuthenticated);
	}

	public virtual Task Connect(ILocation location, IConnectionConfiguration connectionConfiguration, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Connect(new List<ILocation> { location }, new List<IConnectionConfiguration> { connectionConfiguration }, cancellationToken);
	}

	public virtual Task Connect(IEnumerable<ILocation> locations, IConnectionConfiguration connectionConfiguration, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Connect(locations, new List<IConnectionConfiguration> { connectionConfiguration }, cancellationToken);
	}

	public virtual Task Connect(ILocation location, IEnumerable<IConnectionConfiguration> connectionConfigurations, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Connect(new List<ILocation> { location }, connectionConfigurations, cancellationToken);
	}

	public virtual async Task Connect(IEnumerable<ILocation> selectedLocations, IEnumerable<IConnectionConfiguration> connectionConfigurations, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (CurrentConnectionStatus == ConnectionStatus.Connecting)
		{
			throw new InvalidOperationException("Connection already in progress.");
		}
		_connectionCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[1] { cancellationToken });
		try
		{
			foreach (IConnectionConfiguration connectionConfiguration in connectionConfigurations)
			{
				if (!AvailableProtocols.ContainsKey(connectionConfiguration.ConnectionType) || !AvailableProtocols[connectionConfiguration.ConnectionType])
				{
					throw new UnsupportedProtocolException($"{connectionConfiguration.ConnectionType} is not supported.");
				}
			}
			OnVpnConnectionStatusChanged(ConnectionStatus.Connecting);
			try
			{
				if (!(selectedLocations.FirstOrDefault() is Server) && !_lastKnownUserLocation.IsValid())
				{
					try
					{
						await UpdatePositionInfo(null, oneTime: true, _connectionCancellationTokenSource.Token);
					}
					catch (OperationCanceledException)
					{
						throw;
					}
					catch
					{
					}
				}
				IList<Server> serversToConnect = LocationsHelper.LocationToServers(Locations, selectedLocations, _lastKnownUserLocation);
				if (!serversToConnect.Any())
				{
					string text = ((connectionConfigurations.Count() <= 1) ? $"{connectionConfigurations.First().ConnectionType} protocol" : (string.Join(", ", connectionConfigurations.Select((IConnectionConfiguration x) => x.ConnectionType).ToList()) + " protocols"));
					if (selectedLocations.Any() && selectedLocations.Count() > 1)
					{
						_logger?.LogError("Country " + selectedLocations.First().Id + " does not support " + text + ".");
					}
					else
					{
						_logger?.LogError("Location " + selectedLocations.FirstOrDefault()?.Id + " does not support " + text + ".");
					}
					throw new NullLocationException("The location selected does not currently support the " + text + ", please try a different protocol or location.");
				}
				foreach (Server server in serversToConnect)
				{
					try
					{
						await Connect(server, connectionConfigurations, _connectionCancellationTokenSource.Token);
						OnVpnConnectionStatusChanged(ConnectionStatus.Connected);
					}
					catch (ConnectionException)
					{
						if (serversToConnect.IndexOf(server) == serversToConnect.Count - 1)
						{
							throw;
						}
						continue;
					}
					break;
				}
			}
			catch
			{
				OnVpnConnectionStatusChanged(ConnectionStatus.Disconnected);
				throw;
			}
		}
		finally
		{
			try
			{
				_connectionCancellationTokenSource.Dispose();
			}
			catch
			{
			}
			_connectionCancellationTokenSource = null;
		}
	}

	public virtual async Task<DriverInstallResult> InstallTapDriver()
	{
		if (!IsElevated)
		{
			throw new NotElevatedException("This feature only works when the running process is elevated");
		}
		if (Configuration.RunInUserspace)
		{
			throw new UnsupportedProtocolException("SDK is configured for running user space only. TAP Driver cannot be set up from user space.");
		}
		IProtocolManager protocolManager = _protocolManagers.Where((KeyValuePair<VpnManagerType, IProtocolManager> x) => x.Key == VpnManagerType.OpenVPN)?.Select((KeyValuePair<VpnManagerType, IProtocolManager> x) => x.Value).FirstOrDefault();
		OnTapDeviceInstallationStatusChanged(OperationStatus.InProgress);
		try
		{
			if (protocolManager is IDriverManager driverManager)
			{
				DriverInstallResult driverInstallResult = await driverManager.InstallDriver().ConfigureAwait(continueOnCapturedContext: false);
				OnPropertyChanged("IsTapDriverInstalled");
				OnTapDeviceInstallationStatusChanged((driverInstallResult != DriverInstallResult.Failed) ? OperationStatus.Completed : OperationStatus.Failed);
				if (driverInstallResult != DriverInstallResult.Failed)
				{
					_filteringManager.UpdateOpenVpnNetworkFilter();
				}
				return driverInstallResult;
			}
			throw new InvalidCastException("openvpnManager does not implement IDriverManager");
		}
		catch
		{
			OnPropertyChanged("IsTapDriverInstalled");
			throw;
		}
	}

	public virtual async Task<DriverUninstallResult> UninstallTapDriver()
	{
		if (!IsElevated)
		{
			throw new NotElevatedException("This feature only works when the running process is elevated");
		}
		if (Configuration.RunInUserspace)
		{
			throw new UnsupportedProtocolException("SDK is configured for running user space only. TAP Driver cannot be set up from user space.");
		}
		IProtocolManager protocolManager = _protocolManagers.Where((KeyValuePair<VpnManagerType, IProtocolManager> x) => x.Key == VpnManagerType.OpenVPN)?.Select((KeyValuePair<VpnManagerType, IProtocolManager> x) => x.Value).FirstOrDefault();
		try
		{
			if (protocolManager is IDriverManager driverManager)
			{
				DriverUninstallResult result = await driverManager.RemoveDriver().ConfigureAwait(continueOnCapturedContext: false);
				OnPropertyChanged("IsTapDriverInstalled");
				return result;
			}
			throw new InvalidCastException("openvpnManager does not implement IDriverManager");
		}
		catch
		{
			OnPropertyChanged("IsTapDriverInstalled");
			throw;
		}
	}

	public virtual async Task Disconnect()
	{
		_currentServer = null;
		IProtocolManager protocolManager = _protocolManagers.Where((KeyValuePair<VpnManagerType, IProtocolManager> x) => x.Value.IsActive)?.Select((KeyValuePair<VpnManagerType, IProtocolManager> x) => x.Value)?.FirstOrDefault();
		try
		{
			if (_connectionCancellationTokenSource != null && !_connectionCancellationTokenSource.IsCancellationRequested)
			{
				_connectionCancellationTokenSource.Cancel();
			}
		}
		catch
		{
		}
		if (protocolManager != null)
		{
			OnVpnConnectionStatusChanged(ConnectionStatus.Disconnecting);
			await protocolManager.Disconnect().ConfigureAwait(continueOnCapturedContext: false);
		}
		OnVpnConnectionStatusChanged(ConnectionStatus.Disconnected);
	}

	public async Task RefreshUserToken()
	{
		if (User == null)
		{
			throw new NotAuthorizedException("No user logged in to refresh.");
		}
		try
		{
			await _zorgApiManager.RefreshToken().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HTTPException ex) when (ex is OAuthException || ex is AuthenticationException)
		{
			_logger?.LogError("User account became invalidated.");
			OnAuthenticationCredentialsExpired();
			_zorgApiManager.ClearUser();
			throw;
		}
	}

	public void DebugDropServerInfo(int ignoreNumber = 0)
	{
		_zorgApiManager.DropServers(ignoreNumber);
	}

	public void DebugDropLocation(ILocation location)
	{
		_zorgApiManager.DropLocation(location);
	}

	public async Task RefreshServerInfoForced()
	{
		if (User == null)
		{
			throw new NotAuthorizedException("No user logged in to refresh server info.");
		}
		try
		{
			await _zorgApiManager.GetServers().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HTTPException ex) when (ex is OAuthException || ex is AuthenticationException)
		{
			_logger?.LogError("User account became invalidated.");
			OnAuthenticationCredentialsExpired();
			_zorgApiManager.ClearUser();
			throw;
		}
		finally
		{
			if (_infoRefreshTimer?.Enabled ?? false)
			{
				_infoRefreshTimer.Stop();
				_infoRefreshTimer.Start();
			}
		}
	}

	public async Task<bool> RefreshServerInfo()
	{
		if (_lastServerListRefreshTime.Add(_serverRefreshCooldown) > DateTime.Now)
		{
			return false;
		}
		_lastServerListRefreshTime = DateTime.Now;
		await RefreshServerInfoForced();
		return true;
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			IsDisposed = true;
			SystemEvents.PowerModeChanged -= SystemEventsOnPowerModeChanged;
			NetworkChange.NetworkAddressChanged -= NetworkChangeOnNetworkAddressChanged;
			if (_infoRefreshTimer != null)
			{
				_infoRefreshTimer.Stop();
				_infoRefreshTimer.Elapsed -= InfoRefreshTimerOnElapsed;
				_infoRefreshTimer.Dispose();
			}
			_filteringManager?.Dispose();
			_protocolManagers.All(delegate(KeyValuePair<VpnManagerType, IProtocolManager> x)
			{
				x.Value?.Dispose();
				return true;
			});
			_protocolManagers.Clear();
		}
	}

	public async Task DisposeAsync()
	{
		if (IsDisposed)
		{
			return;
		}
		IsDisposed = true;
		SystemEvents.PowerModeChanged -= SystemEventsOnPowerModeChanged;
		NetworkChange.NetworkAddressChanged -= NetworkChangeOnNetworkAddressChanged;
		if (_infoRefreshTimer != null)
		{
			_infoRefreshTimer.Stop();
			_infoRefreshTimer.Elapsed -= InfoRefreshTimerOnElapsed;
			_infoRefreshTimer.Dispose();
		}
		_filteringManager?.Dispose();
		foreach (IProtocolManager value in _protocolManagers.Values)
		{
			if (value != null && value.IsActive)
			{
				await value.DisposeAsync();
			}
		}
		_protocolManagers.Clear();
	}

	public List<string> GetActiveRasConnections()
	{
		return (from x in WindowsVpn.GetActiveConnections()
			select x.EntryName).ToList();
	}

	public async Task<Dictionary<string, string>> GetAccountMetadata()
	{
		try
		{
			return await _zorgApiManager.GetAccountMetadata().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HTTPException ex) when (ex is OAuthException || ex is AuthenticationException)
		{
			_logger?.LogError("User account became invalidated.");
			OnAuthenticationCredentialsExpired();
			_zorgApiManager.ClearUser();
			throw;
		}
	}

	public async Task<bool> SaveAccountMetadata(Dictionary<string, string> metadata)
	{
		try
		{
			ValidateMetadata(metadata);
			return await _zorgApiManager.SaveAccountMetadata(metadata).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HTTPException ex) when (ex is OAuthException || ex is AuthenticationException)
		{
			_logger?.LogError("User account became invalidated.");
			OnAuthenticationCredentialsExpired();
			_zorgApiManager.ClearUser();
			throw;
		}
	}

	public bool StopSplitTunnelDriverService(bool waitForStop)
	{
		return _filteringManager?.StopSplitTunnelDriverService(waitForStop) ?? false;
	}

	public bool UninstallSplitTunnelDriverService(bool waitForStop)
	{
		return _filteringManager?.UninstallSplitTunnelDriverService(waitForStop) ?? false;
	}

	public async Task InvalidateUser()
	{
		try
		{
			await _zorgApiManager.Logout();
		}
		catch (Exception exception)
		{
			_logger?.LogError(exception, "Unable to invalidate user");
		}
	}

	public void SetTokenExpire(DateTime expireDateTime)
	{
		_zorgApiManager.SetTokenExpire(expireDateTime);
	}

	public string GetActiveVpnConnectionName()
	{
		return NetworkingHelper.GetActiveVpnConnectionName();
	}

	public void SetApiTimeout(int timeoutInSeconds)
	{
		_zorgApiManager.SetApiTimeout(timeoutInSeconds);
	}

	public void SetUserLocation(double latitude, double longitude, string countryCode)
	{
		_lastKnownUserLocation = new NetworkGeolocation
		{
			CountryCode = countryCode,
			Latitude = (decimal)latitude,
			Longitude = (decimal)longitude
		};
	}

	public IList<Server> GetOptimalServers()
	{
		return LocationsHelper.LocationToServers(Locations, new List<ILocation> { Locations.First() }, _lastKnownUserLocation);
	}

	public Version GetDriverVersion(Driver driver)
	{
		return _driverVersionProvider.GetVersion(driver);
	}

	protected void OnTapDeviceInstallationStatusChanged(OperationStatus status)
	{
		if (!IsDisposed)
		{
			_syncContext.Post(delegate(object state)
			{
				TapDeviceInstallationStatusChanged?.Invoke(state as ISDK, status);
			}, this);
		}
	}

	protected void OnVpnConnectionStatusChanged(ConnectionStatus status)
	{
		if (IsDisposed)
		{
			return;
		}
		ConnectionStatus connectionStatusBackup = CurrentConnectionStatus;
		CurrentConnectionStatus = status;
		_logger?.LogTrace("VpnConnectionStatus={VpnConnectionStatus}", status);
		BeforeConnectionStateFiltersApplied?.Invoke(this, status);
		_filteringManager?.HandleConnectionStatus(status);
		_logger?.LogTrace("Raised Handle ConnectionStatus with Filter Manager");
		CalculateDataTransfer();
		OnPropertyChanged("IsConnected");
		_syncContext.Post(delegate(object state)
		{
			if (status == ConnectionStatus.Disconnected)
			{
				ActiveConnectionInformation = null;
			}
			IsConnecting = status == ConnectionStatus.Connecting;
			IsDisconnecting = status == ConnectionStatus.Disconnecting;
			IsDisconnected = status == ConnectionStatus.Disconnected;
			VpnConnectionStatusChanged?.Invoke(state as ISDK, connectionStatusBackup, status);
		}, this);
		switch (status)
		{
		case ConnectionStatus.Connected:
			UpdatePositionInfo(_currentServer).Forget();
			break;
		case ConnectionStatus.Disconnected:
			UpdatePositionInfo().Forget();
			break;
		}
	}

	private async Task UpdateBrandingInformation(string slug)
	{
		BrandingInfo brandingInfo = await _zorgApiManager.GetBrandingInformation(slug, Configuration.BrandingKey).ConfigureAwait(continueOnCapturedContext: false);
		if (brandingInfo != null && brandingInfo.ApiKey != null && brandingInfo != null && brandingInfo.Slug != null)
		{
			_zorgApiManager.UpdateApiKey(brandingInfo.ApiKey);
			_zorgApiManager.UpdateAuthSuffix(brandingInfo.Slug);
		}
	}

	private async Task PerformLogin(Func<Task> loginAction)
	{
		bool isReauth = _zorgApiManager.User.IsValid;
		if (isReauth)
		{
			_logger?.LogInformation("Re-authenticating with new tokens/credentials.");
		}
		else
		{
			OnAuthenticationStatusChanged(AuthenticationStatus.InProgress);
			try
			{
				using CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30.0));
				await NetworkingHelper.WaitForNetworkAvailable(tokenSource.Token).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch
			{
			}
		}
		try
		{
			await loginAction().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (EndpointsUnreachableException)
		{
			OnAuthenticationStatusChanged(AuthenticationStatus.NotAuthenticated);
			throw;
		}
		catch (HTTPException ex2) when (!isReauth)
		{
			if (!ex2.GetAllExceptions().OfType<VpnApiException>().Any((VpnApiException x) => x.Error == ApiError.Unknown || x.Error == ApiError.InternalServerError))
			{
				OnAuthenticationStatusChanged(AuthenticationStatus.NotAuthenticated);
				throw;
			}
			try
			{
				await Task.Delay(TimeSpan.FromSeconds(5.0)).ConfigureAwait(continueOnCapturedContext: false);
				await loginAction().ConfigureAwait(continueOnCapturedContext: false);
			}
			catch
			{
				OnAuthenticationStatusChanged(AuthenticationStatus.NotAuthenticated);
				throw;
			}
		}
		catch (Exception)
		{
			OnAuthenticationStatusChanged(AuthenticationStatus.NotAuthenticated);
			throw;
		}
		if (isReauth)
		{
			OnAuthenticationStatusChanged(AuthenticationStatus.Authenticated);
			if (!_infoRefreshTimer.Enabled)
			{
				_infoRefreshTimer.Start();
			}
			return;
		}
		try
		{
			if (await _zorgApiManager.GetServers().ConfigureAwait(continueOnCapturedContext: false) == ServersLoadedFrom.Cache)
			{
				RefreshServerInfoForced().Forget();
			}
		}
		catch
		{
			try
			{
				await Task.Delay(TimeSpan.FromSeconds(5.0)).ConfigureAwait(continueOnCapturedContext: false);
				await _zorgApiManager.GetServers().ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception inner)
			{
				_zorgApiManager.ClearUser();
				OnAuthenticationStatusChanged(AuthenticationStatus.NotAuthenticated);
				throw new ServerListException("Failed to retrieve server list.", inner);
			}
		}
		UpdatePositionInfo().Forget();
		OnAuthenticationStatusChanged(AuthenticationStatus.Authenticated);
		_infoRefreshTimer.Start();
	}

	private bool ShouldSkipConnectionConfiguration(IConnectionConfiguration connectionConfiguration, bool isDoubleHopEnabled)
	{
		if (!isDoubleHopEnabled)
		{
			return false;
		}
		if (connectionConfiguration.ConnectionType == NetworkConnectionType.IKEv2)
		{
			_logger?.LogInformation("Skipping connection with IKEv2 as it cannot be used when double hop VPN is enabled.");
			return true;
		}
		if (connectionConfiguration is OpenVpnConnectionConfiguration { Scramble: not false })
		{
			_logger?.LogInformation("Skipping connection with OpenVPN Scramble as it cannot be used when double hop VPN is enabled.");
			return true;
		}
		return false;
	}

	private async Task Connect(Server serverToConnect, IEnumerable<IConnectionConfiguration> connectionConfigurations, CancellationToken cancellationToken)
	{
		int protocolsTried = 0;
		_cancellationToken = cancellationToken;
		IsConnectionCancelled = false;
		List<IConnectionConfiguration> list = connectionConfigurations.Where((IConnectionConfiguration x) => serverToConnect.Configuration.GetNetworkConnectionTypes().Contains(x.ConnectionType)).ToList();
		Validate(list);
		bool isDoubleHopEnabled = list.Any((IConnectionConfiguration c) => c is IDoubleHopConfiguration { DoubleHopSettings: not null } doubleHopConfiguration && doubleHopConfiguration.DoubleHopSettings.IsDoubleHopEnabled);
		InvalidAccountException ex5 = default(InvalidAccountException);
		foreach (IConnectionConfiguration connectionConfiguration in list)
		{
			try
			{
				if (connectionConfiguration is IOpenVpnConnectionConfiguration && !IsTapDriverInstalled)
				{
					throw new TapAdapterException("No TAP device is currently installed. Please install a TAP device to use OpenVPN.");
				}
				if (ShouldSkipConnectionConfiguration(connectionConfiguration, isDoubleHopEnabled))
				{
					protocolsTried++;
					if (protocolsTried >= connectionConfigurations.Count())
					{
						throw new ConnectionException("Failed to establish a connection to the server.");
					}
					continue;
				}
				_currentServer = new ServerProxy(serverToConnect);
				ConnectionInfo connInfo = new ConnectionInfo
				{
					Configuration = connectionConfiguration,
					Location = _currentServer,
					Protocol = connectionConfiguration.ConnectionType,
					ServerIp = serverToConnect.IP,
					IsKillKwitchOn = AllowOnlyVPNConnectivity,
					IsLanTrafficAllowed = AllowLANTraffic,
					IsDnsProtected = !DisableDNSLeakProtection,
					IsIPv6Protected = !DisableIPv6LeakProtection,
					IsAutomaticProtocolUsed = (connectionConfigurations.Count() > 0)
				};
				SplitTunnelClientSettings splitTunnelClientSettings = new SplitTunnelClientSettings
				{
					IsEnabled = IsSplitTunnelEnabled,
					Mode = SplitTunnelMode,
					AllowedApps = SplitTunnelAllowedApps,
					AllowedDomains = SplitTunnelAllowedDomains
				};
				_filteringManager?.AddAddressToWhitelist(connInfo.ServerIp);
				try
				{
					await _zorgApiManager.EnsureValidAccount(_cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					IProtocolManager protocolManager = (from x in _protocolManagers
						where x.Key == connectionConfiguration.ManagerType
						select x.Value).FirstOrDefault();
					if (protocolManager == null)
					{
						throw new InvalidOperationException("Unknown connection configuration provided.");
					}
					if (_cancellationToken.IsCancellationRequested)
					{
						throw new OperationCanceledException();
					}
					DnsSettings dnsSettings = await GetDnsSettingsForThreatProtection().ConfigureAwait(continueOnCapturedContext: false);
					if (!Configuration.UseTokenAuthentication || (connectionConfiguration.ManagerType != VpnManagerType.OpenVPN && connectionConfiguration.ManagerType != VpnManagerType.RAS))
					{
						await protocolManager.Connect(serverToConnect, connectionConfiguration, User, splitTunnelClientSettings, dnsSettings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					else
					{
						await ConnectWithTokenRefreshRetryPolicy(protocolManager, serverToConnect, connectionConfiguration, User, splitTunnelClientSettings, dnsSettings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					ActiveConnectionInformation = connInfo;
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (VPNAuthenticationException exception)
				{
					if (Configuration.UseTokenAuthentication && (connectionConfiguration.ManagerType == VpnManagerType.OpenVPN || connectionConfiguration.ManagerType == VpnManagerType.RAS))
					{
						_logger?.LogError(exception, "Authentication failed: Invalid tokens.");
					}
					else
					{
						_logger?.LogError(exception, "Authentication failed: Invalid credentials.");
					}
					throw new VPNAuthenticationException("VPN authorization failed. Please try connecting to a different location.");
				}
				await Task.Delay(1000);
				NetworkInterop.FlushDnsCache();
				UpdatePositionInfo(_currentServer).Forget();
			}
			catch (OperationCanceledException)
			{
				IsConnectionCancelled = true;
				OnPropertyChanged("IsConnectionCancelled");
				_logger?.LogInformation("Connection process was canceled.");
				if (!_cancellationToken.IsCancellationRequested)
				{
					throw new ApiTimeoutException("The HTTP request has timed out.");
				}
				throw;
			}
			catch (HTTPException ex3) when (ex3 is OAuthException || ex3 is AuthenticationException)
			{
				_logger?.LogError("User account became invalidated.");
				OnAuthenticationCredentialsExpired();
				_zorgApiManager.ClearUser();
				throw;
			}
			catch (HTTPException ex4) when (((Func<bool>)delegate
			{
				// Could not convert BlockContainer to single expression
				ex5 = ex4 as InvalidAccountException;
				return ex5 != null;
			}).Invoke())
			{
				_logger?.LogError("User account is invalid.");
				if (ex5.AccountStatus.HasValue)
				{
					_logger?.LogDebug($"The account status is {ex5.AccountStatus}.");
				}
				throw;
			}
			catch (HTTPException ex6) when (ex6 is InvalidServerException)
			{
				_logger?.LogError("Invalid server.");
				throw;
			}
			catch (HTTPException ex7) when (ex7 is InvalidDoubleHopConfigurationException)
			{
				_logger?.LogError("Invalid double hop configuration.");
				throw;
			}
			catch (DnsConfigurationException exception2)
			{
				_logger?.LogError(exception2, "DNS servers configuration error.");
				throw;
			}
			catch (VpnHostServiceFileNotFoundException exception3)
			{
				_logger?.LogError(exception3, "VPNHostService file not found before connection.");
				throw;
			}
			catch (DoubleHopNotAvailableException exception4)
			{
				_logger?.LogError(exception4, "Doublehop is not available.");
				throw;
			}
			catch (EndpointsUnreachableException)
			{
				throw;
			}
			catch (TapAdapterException exception5)
			{
				_logger?.LogError(exception5, "Tap adapter is not installed.");
				throw;
			}
			catch (Exception ex9)
			{
				protocolsTried++;
				string id = serverToConnect.GetParent<Location>().Id;
				string id2 = serverToConnect.GetParent<Location>().GetParent<Location>().Id;
				ErrorRaised?.Invoke(this, new ConnectionError(serverToConnect.Hostname, serverToConnect.IP.ToString(), id, id2, connectionConfiguration, ex9));
				_logger?.LogError(ex9, "An exception occurred during connection process.");
				if (protocolsTried >= connectionConfigurations.Count())
				{
					if (connectionConfigurations.Count() == 1)
					{
						throw new ConnectionException($"Failed to establish connection to the server using {connectionConfigurations.First().ConnectionType} protocol.", ex9);
					}
					throw new ConnectionException("Failed to establish connection to the server.", ex9);
				}
				continue;
			}
			break;
		}
	}

	private void Validate(List<IConnectionConfiguration> configurationsToTry)
	{
		if (User == null)
		{
			throw new VPNAuthenticationException("User not logged in.");
		}
		if (!AllowLocalAdaptersWhenConnected && IsSplitTunnelEnabled)
		{
			throw new InvalidOperationException("Split tunneling cannot function as the local area network (LAN) is blocked.");
		}
		if (configurationsToTry != null && configurationsToTry.Count == 1 && configurationsToTry?.FirstOrDefault() is OpenVpnConnectionConfiguration { DoubleHopSettings: not null } openVpnConnectionConfiguration && openVpnConnectionConfiguration.DoubleHopSettings.IsDoubleHopEnabled && openVpnConnectionConfiguration.Scramble)
		{
			throw new InvalidOperationException("Double hop VPN is not supported with OpenVPN Scramble configuration.");
		}
	}

	private void InitConnectionRetryPolicy()
	{
		_retryPolicy = Policy.Handle<VPNAuthenticationException>().WaitAndRetryAsync(1, (int retryCount) => TimeSpan.FromMilliseconds(100.0), async delegate(Exception response, TimeSpan retryCount, Context context)
		{
			_logger?.LogWarning($"VPN connection failed. Refreshing the token before retry. Attempt: {retryCount}, Response: {response.Message}");
			SetTokenExpire(DateTime.UtcNow);
			await _zorgApiManager.RefreshToken().ConfigureAwait(continueOnCapturedContext: false);
			_logger?.LogInformation($"Token refresh completed. Attempt: {retryCount}, Response: {response.Message}");
		});
	}

	private async Task ConnectWithTokenRefreshRetryPolicy(IProtocolManager protocolManager, Server serverToConnect, IConnectionConfiguration connectionConfiguration, IUser user, SplitTunnelClientSettings splitTunnelClientSettings, DnsSettings dnsSettings, CancellationToken cancellationToken)
	{
		await _retryPolicy.ExecuteAsync(async delegate
		{
			await protocolManager.Connect(serverToConnect, connectionConfiguration, user, splitTunnelClientSettings, dnsSettings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<DnsSettings> GetDnsSettingsForThreatProtection()
	{
		DnsSettings dnsSettings = new DnsSettings();
		if (DnsFilterMode != DnsFilteringMode.Disabled)
		{
			List<Dns> list = await _zorgApiManager.GetDnsConfigForThreatProtection(_cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (list == null || !list.Any())
			{
				throw new DnsConfigurationException("No DNS servers returned for threat protection. Threat protection requires at least one valid DNS server.");
			}
			string[] array = FilterDnsServers(list, DnsFilterMode);
			if (array.Length == 0)
			{
				throw new DnsConfigurationException("No custom DNS servers configured for threat protection. Threat protection requires at least one valid custom DNS server.");
			}
			dnsSettings.ShouldUpdateDns = true;
			dnsSettings.DnsServers = array;
		}
		return dnsSettings;
	}

	private string[] FilterDnsServers(List<Dns> dnsServers, DnsFilteringMode dnsFilterMode)
	{
		string filter;
		switch (dnsFilterMode)
		{
		case DnsFilteringMode.WithWLVPNDns:
			filter = "filtering";
			break;
		case DnsFilteringMode.WithPartnerDns:
			filter = "partner";
			break;
		default:
			throw new ArgumentException("Unknown filtering mode.");
		}
		return dnsServers?.Where((Dns dns) => string.Equals(dns.DnsName, filter, StringComparison.OrdinalIgnoreCase)).SelectMany((Dns dns) => dns.DnsValue).ToArray() ?? Array.Empty<string>();
	}

	private async Task RestoreConnection(IConnectionInfo connInfo)
	{
		if (_connectionBeforeSuspend == null && IsDisposed && CurrentConnectionStatus != ConnectionStatus.Disconnected)
		{
			return;
		}
		OnVpnConnectionStatusChanged(ConnectionStatus.Connecting);
		try
		{
			await Connect(((ServerProxy)connInfo.Location).Node, new List<IConnectionConfiguration> { connInfo.Configuration }, _cancellationToken);
			OnVpnConnectionStatusChanged(ConnectionStatus.Connected);
		}
		catch
		{
			OnVpnConnectionStatusChanged(ConnectionStatus.Disconnected);
			throw;
		}
	}

	private async void SystemEventsOnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
	{
		if (IsDisposed)
		{
			return;
		}
		if (e.Mode == PowerModes.Suspend)
		{
			try
			{
				_logger?.LogTrace("PowerSuspend");
				_connectionBeforeSuspend = ActiveConnectionInformation;
				await Disconnect().ConfigureAwait(continueOnCapturedContext: false);
				return;
			}
			catch (Exception exception)
			{
				_logger?.LogError(exception, "Exception during PowerSuspend event");
				return;
			}
		}
		if (e.Mode == PowerModes.Resume && _connectionBeforeSuspend != null)
		{
			try
			{
				await Task.Delay(1000);
				await RestoreConnection(_connectionBeforeSuspend);
			}
			catch (Exception exception2)
			{
				_logger?.LogWarning(exception2, "Failed to reconnect on power event.", new ReconnectOnPowerEventException("Failed to reconnect on power event."));
			}
			_connectionBeforeSuspend = null;
		}
	}

	private void OnVpnConnectionClosedUnexpectedly()
	{
		if (!IsDisposed)
		{
			_logger?.LogTrace("VpnConnectionClosed");
			_lastAdapterChange = DateTime.Now;
			OnVpnConnectionStatusChanged(ConnectionStatus.Disconnected);
			_filteringManager?.HandleConnectionStatus(ConnectionStatus.Disconnected);
			UpdatePositionInfo().Forget();
		}
	}

	private void OnAuthenticationStatusChanged(AuthenticationStatus status)
	{
		if (IsDisposed)
		{
			return;
		}
		_logger?.LogTrace("AuthenticationStatus={AuthenticationStatus}", status);
		_syncContext.Post(delegate(object state)
		{
			AuthenticationStatusChanged?.Invoke(state as ISDK, status);
			if (status != AuthenticationStatus.InProgress)
			{
				OnPropertyChanged("User");
			}
		}, this);
	}

	private void OnAuthenticationCredentialsExpired()
	{
		if (!IsDisposed)
		{
			_logger?.LogTrace("AuthenticationCredentialsExpired");
			_syncContext.Post(delegate(object state)
			{
				AuthenticationCredentialsExpired?.Invoke(state as ISDK);
			}, this);
			OnAuthenticationStatusChanged(AuthenticationStatus.NotAuthenticated);
		}
	}

	private void OnUserLocationStatusChanged(OperationStatus status, NetworkGeolocation location = null)
	{
		if (!IsDisposed)
		{
			if (status == OperationStatus.InProgress)
			{
				_logger?.LogTrace("LocationChanging");
			}
			else if (status == OperationStatus.Completed)
			{
				_logger?.LogTrace("LocationChanged={location}", location);
			}
			_syncContext.Post(delegate(object state)
			{
				CurrentNetworkGeolocation = location;
				OnPropertyChanged("CurrentNetworkGeolocation");
				UserLocationStatusChanged?.Invoke(state as ISDK, status, location);
			}, this);
		}
	}

	private void OnLocationRefreshStatusChanged(object sender, RefreshLocationListStatus status)
	{
		if (!IsDisposed)
		{
			_syncContext.Post(delegate(object state)
			{
				LocationsRefreshStatusChanged?.Invoke(state as ISDK, status);
			}, this);
		}
	}

	private void OnTokenRefresh(object sender, IUser e)
	{
		if (!IsDisposed)
		{
			_syncContext.Post(delegate(object state)
			{
				OnPropertyChanged("User");
				TokensRefreshed?.Invoke(state as ISDK, e);
			}, this);
		}
	}

	private void OnProxyError(object sender, ProxyError e)
	{
		ErrorRaised?.Invoke(this, ApiProxyError.Create(e));
	}

	private void OnHttpError(object sender, HttpError e)
	{
		ErrorRaised?.Invoke(this, ApiHttpError.Create(e));
	}

	private void InfoRefreshTimerOnElapsed(object sender, ElapsedEventArgs e)
	{
		_syncContext.Post(async delegate
		{
			await InfoRefreshExecute().ConfigureAwait(continueOnCapturedContext: false);
		}, this);
	}

	private async Task InfoRefreshExecute()
	{
		if (User == null || IsDisposed)
		{
			return;
		}
		try
		{
			await _zorgApiManager.GetServers().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (HTTPException ex) when (ex is OAuthException || ex is AuthenticationException)
		{
			_logger?.LogError("User account became invalidated.");
			OnAuthenticationCredentialsExpired();
			_zorgApiManager.ClearUser();
		}
		catch (Exception exception)
		{
			_logger?.LogWarning(exception, "Auto-refreshing server list failed.");
		}
	}

	private async Task UpdatePositionInfo(IServer serverLocation = null, bool oneTime = false, CancellationToken? cancellationToken = null)
	{
		if (User == null || IsDisposed)
		{
			return;
		}
		OnUserLocationStatusChanged(OperationStatus.InProgress);
		_ipgeoCancellationTokenSource?.Cancel();
		_ipgeoCancellationTokenSource = new CancellationTokenSource();
		NetworkGeolocation networkGeolocation = null;
		CancellationToken token = ((!cancellationToken.HasValue) ? _ipgeoCancellationTokenSource.Token : cancellationToken.Value);
		try
		{
			networkGeolocation = await _zorgApiManager.GetCurrentPosition(token, oneTime).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			return;
		}
		finally
		{
			_ipgeoCancellationTokenSource?.Dispose();
			_ipgeoCancellationTokenSource = null;
		}
		if (networkGeolocation != null)
		{
			if (serverLocation != null)
			{
				networkGeolocation.City = serverLocation.City?.ToTitleCase();
				networkGeolocation.Country = serverLocation.Country?.ToTitleCase();
				networkGeolocation.CountryCode = serverLocation.CountryCode?.ToUpper();
			}
			else
			{
				networkGeolocation.City = networkGeolocation.City?.ToTitleCase();
				networkGeolocation.Country = networkGeolocation.Country?.ToTitleCase();
				networkGeolocation.CountryCode = networkGeolocation.CountryCode?.ToUpper();
				_lastKnownUserLocation = networkGeolocation;
			}
		}
		if (networkGeolocation == null)
		{
			OnUserLocationStatusChanged(OperationStatus.Failed);
		}
		else
		{
			OnUserLocationStatusChanged(OperationStatus.Completed, networkGeolocation);
		}
	}

	private void NetworkChangeOnNetworkAddressChanged(object sender, EventArgs e)
	{
		if (!IsDisposed && CurrentConnectionStatus == ConnectionStatus.Disconnected && _ipgeoCancellationTokenSource == null && (DateTime.Now - _lastAdapterChange).TotalSeconds > 5.0)
		{
			_lastAdapterChange = DateTime.Now;
			_logger?.LogTrace("UserNetworkChangedWhileDisconnected");
			UpdatePositionInfo().Forget();
		}
	}

	private void ValidateMetadata(Dictionary<string, string> metadata)
	{
		if (metadata == null)
		{
			throw new AccountMetadataException("Metadata cannot be null.");
		}
		if (metadata.Count > 5)
		{
			throw new AccountMetadataException("Metadata cannot have more than 5 key-value pairs.");
		}
		if (metadata.Any((KeyValuePair<string, string> data) => string.IsNullOrWhiteSpace(data.Key)))
		{
			throw new AccountMetadataException("The key must not be left blank or contain only white spaces.");
		}
		if (metadata.Any((KeyValuePair<string, string> data) => data.Key.Length > 128))
		{
			throw new AccountMetadataException("Key must have a maximum length of 128 characters.");
		}
		if (metadata.Any((KeyValuePair<string, string> data) => data.Value.Length > 1024))
		{
			throw new AccountMetadataException("Value must have a maximum length of 1024 characters.");
		}
		if ((from data in metadata
			group data by data.Key).Any((IGrouping<string, KeyValuePair<string, string>> group) => group.Count() > 1))
		{
			throw new AccountMetadataException("Duplicate keys are not permitted.");
		}
	}

	private void CalculateDataTransfer()
	{
		CleanUp();
		if (DataTransferUpdate == null)
		{
			return;
		}
		RaiseDataTransferUpdateEvent(0L, 0L);
		if (CurrentConnectionStatus != ConnectionStatus.Connected)
		{
			return;
		}
		string[] networkInterfaceDescriptions = new string[3]
		{
			Configuration.OpenVpnConfiguration.InstalledTapDriver.GetDescription(),
			"WinTUN",
			"Wireguard"
		};
		NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault((NetworkInterface networkInterface2) => networkInterface2.OperationalStatus == OperationalStatus.Up && (networkInterface2.Name.Contains(Configuration.ApplicationName) || networkInterfaceDescriptions.Any((string x) => networkInterface2.Description.StartsWith(x, StringComparison.OrdinalIgnoreCase))));
		if (networkInterface == null && Configuration.OpenVpnConfiguration != null)
		{
			networkInterface = NetworkInterfaceHelper.GetNetworkInterface(Configuration.OpenVpnConfiguration.InstalledTapDriver.ToString());
		}
		_getVpnInterfaceDisposable = (from item in Observable.Interval(TimeSpan.FromMilliseconds(100.0)).TakeWhile((long item) => CurrentConnectionStatus == ConnectionStatus.Connected)
			select networkInterface into item
			where item != null
			select item).Take(1).Subscribe(delegate(NetworkInterface networkInterface2)
		{
			_bandwidthMonitorDisposable = (from item in networkInterface2.ObserveDataUsageFromStart(TimeSpan.FromSeconds(1.0), Scheduler.Default)
				where item != null
				select item).Buffer(2, 1).SubscribeOn(_syncContext).Subscribe(delegate(IList<TotalDataTransfer> item)
			{
				RaiseDataTransferUpdateEvent(item[1].Received, item[1].Sent);
			});
		});
	}

	private void CleanUp()
	{
		if (_getVpnInterfaceDisposable != null)
		{
			_getVpnInterfaceDisposable.Dispose();
			_getVpnInterfaceDisposable = null;
		}
		if (_bandwidthMonitorDisposable != null)
		{
			_bandwidthMonitorDisposable.Dispose();
			_bandwidthMonitorDisposable = null;
		}
	}

	private void RaiseDataTransferUpdateEvent(long downloadedBytes, long uploadedBytes)
	{
		DataTransferEventArgs dataTransferArgs = new DataTransferEventArgs(downloadedBytes, uploadedBytes);
		if (_lastDataTransferArgs == dataTransferArgs)
		{
			return;
		}
		_lastDataTransferArgs = dataTransferArgs;
		_syncContext.Post(delegate
		{
			try
			{
				DataTransferUpdate?.Invoke(this, dataTransferArgs);
			}
			catch
			{
			}
		}, this);
	}

	public void SetTrafficOptimizerConfig(TrafficOptimizerConfig trafficOptimizerConfig)
	{
		if (!IsConnected)
		{
			AppTrafficOptimizer feature = FeatureFactory.GetFeature<AppTrafficOptimizer>();
			if (feature != null)
			{
				try
				{
					feature.Config = trafficOptimizerConfig;
					return;
				}
				catch (Exception ex)
				{
					throw new InvalidConfigurationException(ex.Message, ex);
				}
			}
			return;
		}
		throw new InvalidOperationException("Cannot change the configuration after connecting to the VPN server");
	}

	public void SetDnsMonitorConfig(DnsMonitoringConfig dnsMonitorConfig)
	{
		if (!IsConnected)
		{
			DnsMonitoring feature = FeatureFactory.GetFeature<DnsMonitoring>();
			if (feature != null)
			{
				try
				{
					feature.Config = dnsMonitorConfig;
					return;
				}
				catch (Exception ex)
				{
					throw new InvalidConfigurationException(ex.Message, ex);
				}
			}
			return;
		}
		throw new InvalidOperationException("Cannot change the configuration after connecting to the VPN server");
	}
}
