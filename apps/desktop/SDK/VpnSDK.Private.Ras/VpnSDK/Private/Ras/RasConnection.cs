using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DotRas;
using Microsoft.Extensions.Logging;
using VpnSDK.Common.Routing;
using VpnSDK.Common.Settings;
using VpnSDK.Common.Utilities;
using VpnSDK.Private.Ras.Extensions;
using VpnSDK.Private.Ras.Utilities;

namespace VpnSDK.Private.Ras;

internal class RasConnection : IDisposable
{
	private readonly ILogger _logger = LogProvider.GetLogger("VpnSDK::Private::RAS");

	private readonly CancellationTokenSource _disposeReconnectTokenSource = new CancellationTokenSource();

	private RasEntry _connectionEntry;

	private RasConnectionWatcher _connectionWatcher;

	private RasDialer _dialer;

	private readonly SplitTunnelClientSettings _splitTunnelClientSettings;

	private readonly List<RouteInfo> _routes;

	private readonly DnsSettings _dnsSettings;

	private const int INTERFACE_METRIC = 1;

	private const int ROUTE_METRIC = 1;

	public bool Connected { get; private set; }

	public RasConnectionType ConnectionType => (RasConnectionType)Enum.Parse(typeof(RasConnectionType), Regex.Match(_connectionEntry.Name, "\\(([^)]*)\\)").Groups[1].Value);

	public string Hostname => _connectionEntry?.PhoneNumber ?? null;

	public bool IsDisposed { get; private set; }

	public event Action UnexpectedDisconnect;

	internal RasConnection(RasDialer dialer, DotRas.RasConnection connection, NetworkCredential credentials, SplitTunnelClientSettings splitTunnelClientSettings, List<RouteInfo> routes, DnsSettings dnsSettings)
	{
		_dialer = dialer;
		_splitTunnelClientSettings = splitTunnelClientSettings;
		_routes = routes;
		_dnsSettings = dnsSettings;
		if (_dialer.Credentials == null)
		{
			_dialer.Credentials = credentials;
		}
		Connected = true;
		_connectionWatcher = new RasConnectionWatcher();
		_connectionWatcher.Disconnected += ConnectionWatcherOnDisconnected;
		_connectionWatcher.Handle = connection.Handle;
		_connectionWatcher.EnableRaisingEvents = true;
		using RasPhoneBook rasPhoneBook = new RasPhoneBook();
		rasPhoneBook.Open(_dialer.PhoneBookPath);
		rasPhoneBook.TryFindEntry(connection.EntryName, out _connectionEntry);
	}

	internal RasConnection(RasDialer dialer, IPAddress serverAddress, RasConnectionType connectionType, SplitTunnelClientSettings splitTunnelClientSettings, List<RouteInfo> routes, DnsSettings dnsSettings, string preSharedKey = null)
		: this(dialer, serverAddress.ToString(), connectionType, splitTunnelClientSettings, routes, dnsSettings, preSharedKey)
	{
	}

	internal RasConnection(RasDialer dialer, string hostName, RasConnectionType connectionType, SplitTunnelClientSettings splitTunnelClientSettings, List<RouteInfo> routes, DnsSettings dnsSettings, string preSharedKey = null)
	{
		_dialer = dialer;
		_splitTunnelClientSettings = splitTunnelClientSettings;
		_routes = routes;
		_dnsSettings = dnsSettings;
		_connectionWatcher = new RasConnectionWatcher();
		_connectionWatcher.Disconnected += ConnectionWatcherOnDisconnected;
		CreateOrUpdateEntry(_dialer.EntryName, hostName, connectionType, preSharedKey);
	}

	public async Task Connect(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException("The object was disposed.");
		}
		if (Connected)
		{
			throw new InvalidOperationException();
		}
		try
		{
			_logger?.LogInformation("Connection in progress");
			RasConnectionWatcher connectionWatcher = _connectionWatcher;
			connectionWatcher.Handle = await _dialer.DialAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			UpdateDnsIfNeeded();
			AddVpnRoutesIfSplitTunnelEnabled();
		}
		catch (InvalidOperationException ex)
		{
			_logger?.LogCritical(ex, $"{ex.Message}. Interface metric is {1} and route metric is {1}");
			throw;
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (RasException ex3)
		{
			_logger?.LogCritical(ex3, $"{ex3.Message}. Code={ex3.ErrorCode}");
			throw;
		}
		catch (Exception ex4)
		{
			_logger?.LogCritical(ex4, ex4.Message + ".");
			throw;
		}
		_connectionWatcher.EnableRaisingEvents = true;
		Connected = true;
	}

	public async Task Disconnect()
	{
		ValidateState();
		DisableConnectionWatcher();
		DotRas.RasConnection activeConnection = GetActiveConnection();
		if (activeConnection != null)
		{
			RemoveRoutesIfSplitTunnelEnabled();
			if ((_splitTunnelClientSettings.IsEnabled && _splitTunnelClientSettings.IsDomainBasedSplitTunnelEnabled) || _dnsSettings.ShouldUpdateDns)
			{
				ResetDnsAddress(activeConnection);
			}
			await activeConnection.HangUpAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		Connected = false;
	}

	private void DisconnectSync()
	{
		ValidateState();
		DisableConnectionWatcher();
		DotRas.RasConnection activeConnection = GetActiveConnection();
		if (activeConnection != null)
		{
			RemoveRoutesIfSplitTunnelEnabled();
			if ((_splitTunnelClientSettings.IsEnabled && _splitTunnelClientSettings.IsDomainBasedSplitTunnelEnabled) || _dnsSettings.ShouldUpdateDns)
			{
				ResetDnsAddress(activeConnection);
			}
			activeConnection.HangUp();
		}
		Connected = false;
	}

	public void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}
		try
		{
			_disposeReconnectTokenSource.Cancel();
		}
		catch
		{
		}
		if (_connectionWatcher != null)
		{
			_connectionWatcher.Disconnected -= ConnectionWatcherOnDisconnected;
			_connectionWatcher.EnableRaisingEvents = false;
			_connectionWatcher.Dispose();
			_connectionWatcher = null;
		}
		if (Connected)
		{
			try
			{
				DisconnectSync();
			}
			catch
			{
			}
		}
		if (_dialer != null)
		{
			_dialer.Dispose();
			_dialer = null;
		}
		IsDisposed = true;
	}

	public async Task DisposeAsync()
	{
		if (IsDisposed)
		{
			return;
		}
		try
		{
			_disposeReconnectTokenSource.Cancel();
		}
		catch
		{
		}
		if (_connectionWatcher != null)
		{
			_connectionWatcher.Disconnected -= ConnectionWatcherOnDisconnected;
			_connectionWatcher.EnableRaisingEvents = false;
			_connectionWatcher.Dispose();
			_connectionWatcher = null;
		}
		if (Connected)
		{
			try
			{
				await Disconnect();
			}
			catch
			{
			}
		}
		if (_dialer != null)
		{
			_dialer.Dispose();
			_dialer = null;
		}
		IsDisposed = true;
	}

	private void ConnectionWatcherOnDisconnected(object sender, RasConnectionEventArgs e)
	{
		Connected = false;
		UnexpectedDisconnect?.Invoke();
	}

	private void CreateOrUpdateEntry(string entryName, string hostName, RasConnectionType connectionType, string presharedKey = null)
	{
		using RasPhoneBook rasPhoneBook = new RasPhoneBook();
		rasPhoneBook.Open(_dialer.PhoneBookPath);
		if (rasPhoneBook.TryFindEntry(entryName, out var entry))
		{
			try
			{
				UpdateEntry(entry, hostName, connectionType, presharedKey);
				return;
			}
			catch
			{
				rasPhoneBook.Entries.Remove(entry);
				AddEntry(rasPhoneBook, entryName, hostName, connectionType, presharedKey);
				return;
			}
		}
		AddEntry(rasPhoneBook, entryName, hostName, connectionType, presharedKey);
	}

	private void SetSplitTunnelSettings(RasEntry entry)
	{
		entry.Options.RemoteDefaultGateway = !_splitTunnelClientSettings.IsEnabled;
	}

	private void UpdateEntry(RasEntry entry, string hostName, RasConnectionType connectionType, string presharedKey = null)
	{
		entry.Device = RasDeviceFinder.Find(connectionType);
		entry.VpnStrategy = (RasVpnStrategy)Enum.Parse(typeof(RasVpnStrategy), connectionType.ToString() + "Only", ignoreCase: true);
		entry.PhoneNumber = hostName;
		if (connectionType == RasConnectionType.IKEv2)
		{
			entry.Options.RequireEap = true;
			entry.Options.RequireMSChap2 = false;
		}
		else
		{
			entry.Options.RequireEap = false;
			entry.Options.RequireMSChap2 = true;
		}
		entry.FramingProtocol = RasFramingProtocol.Ppp;
		entry.EntryType = RasEntryType.Vpn;
		entry.Options.ReconnectIfDropped = false;
		entry.IPv4InterfaceMetric = 1;
		entry.IPv6InterfaceMetric = 1;
		entry.DisableIKEv2Fragmentation = false;
		SetSplitTunnelSettings(entry);
		if (!string.IsNullOrEmpty(presharedKey))
		{
			entry.Options.UsePreSharedKey = true;
			entry.Update();
			entry.UpdateCredentials(RasPreSharedKey.Client, presharedKey);
		}
		else
		{
			entry.Options.UsePreSharedKey = false;
			entry.Update();
			entry.UpdateCredentials(_dialer.Credentials);
		}
		_connectionEntry = entry;
	}

	private void AddEntry(RasPhoneBook phoneBook, string entryName, string hostName, RasConnectionType connectionType, string presharedKey = null)
	{
		bool useRemoteDefaultGateway = !_splitTunnelClientSettings.IsEnabled;
		RasEntry rasEntry = RasEntry.CreateVpnEntry(entryName, hostName, (RasVpnStrategy)Enum.Parse(typeof(RasVpnStrategy), connectionType.ToString() + "Only", ignoreCase: true), RasDeviceFinder.Find(connectionType), useRemoteDefaultGateway);
		if (connectionType == RasConnectionType.IKEv2)
		{
			rasEntry.Options.RequireEap = true;
		}
		else
		{
			rasEntry.Options.RequireMSChap2 = true;
		}
		rasEntry.FramingProtocol = RasFramingProtocol.Ppp;
		rasEntry.EntryType = RasEntryType.Vpn;
		rasEntry.Options.ReconnectIfDropped = false;
		rasEntry.DisableIKEv2Fragmentation = false;
		rasEntry.IPv4InterfaceMetric = 1;
		rasEntry.IPv6InterfaceMetric = 1;
		SetSplitTunnelSettings(rasEntry);
		phoneBook.Entries.Add(rasEntry);
		if (!string.IsNullOrEmpty(presharedKey))
		{
			rasEntry.Options.UsePreSharedKey = true;
			rasEntry.Update();
			rasEntry.UpdateCredentials(RasPreSharedKey.Client, presharedKey);
		}
		else
		{
			rasEntry.Options.UsePreSharedKey = false;
			rasEntry.Update();
			rasEntry.UpdateCredentials(_dialer.Credentials);
		}
		_connectionEntry = rasEntry;
	}

	private void ResetDnsAddress(DotRas.RasConnection activeConnection)
	{
		try
		{
			using RasPhoneBook rasPhoneBook = new RasPhoneBook();
			rasPhoneBook.Open(_dialer.PhoneBookPath);
			rasPhoneBook.TryFindEntry(activeConnection.EntryName, out _connectionEntry);
			if (_connectionEntry != null)
			{
				_connectionEntry.DnsAddress = IPAddress.Any;
				_connectionEntry.Update();
				_logger?.LogInformation("Reset DNS address from Ras entry");
			}
		}
		catch (Exception arg)
		{
			_logger?.LogInformation($"Exception while removing the loopback dns address {arg}");
		}
	}

	private DotRas.RasConnection GetActiveConnection()
	{
		return DotRas.RasConnection.GetActiveConnections().FirstOrDefault((DotRas.RasConnection x) => x.EntryName == _connectionEntry?.Name) ?? DotRas.RasConnection.GetActiveConnections().FirstOrDefault();
	}

	private void DisableConnectionWatcher()
	{
		if (_connectionWatcher != null)
		{
			_connectionWatcher.EnableRaisingEvents = false;
			_connectionWatcher.Handle = null;
		}
	}

	private void ValidateState()
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException("The object was disposed.");
		}
		if (!Connected)
		{
			throw new InvalidOperationException();
		}
	}

	private void RemoveRoutesIfSplitTunnelEnabled()
	{
		if (!_splitTunnelClientSettings.IsEnabled)
		{
			return;
		}
		_logger?.LogInformation("Deleting VPN routes for IKEv2 connection");
		try
		{
			RouteOperationResult routeOperationResult = WindowsRoutingTableManager.DeleteRoutes(_connectionEntry.Name, 1, _routes);
			_logger?.LogInformation((routeOperationResult != RouteOperationResult.Success) ? $"Error occurred while deleting VPN routes for IKEv2 connection. Result: {routeOperationResult}" : "VPN routes for IKEv2 connection successfully deleted");
		}
		catch (Exception ex)
		{
			_logger?.LogError("Error occurred while deleting VPN routes for IKEv2 connection.Exception details: " + ex.ToString());
		}
	}

	private void UpdateDnsIfNeeded()
	{
		if (!_dnsSettings.ShouldUpdateDns && !_splitTunnelClientSettings.IsDomainBasedSplitTunnelEnabled)
		{
			return;
		}
		string[] array = _dnsSettings.DnsServers ?? Array.Empty<string>();
		if (_splitTunnelClientSettings.IsDomainBasedSplitTunnelEnabled)
		{
			array = array.Concat(new string[1] { IPAddress.Loopback.ToString() }).ToArray();
		}
		if (!array.Any())
		{
			return;
		}
		try
		{
			if (_dnsSettings.ShouldUpdateDns)
			{
				DnsConfigurationUtility.SetDnsServer(_connectionEntry.Name, array);
			}
			else
			{
				DnsConfigurationUtility.AddAdditionalDnsServers(_connectionEntry.Name, array);
			}
		}
		catch (Exception ex)
		{
			_logger?.LogError("Error occurred while modifying DNS servers for IKEv2 connection. Exception details: " + ex.ToString());
		}
	}

	private void AddVpnRoutesIfSplitTunnelEnabled()
	{
		if (_splitTunnelClientSettings.IsEnabled)
		{
			_logger?.LogInformation("Adding VPN routes for IKEv2 connection as its interface is no longer the default gateway on remote network.");
			RouteOperationResult routeOperationResult = WindowsRoutingTableManager.AddRoutes(_connectionEntry.Name, 1, _routes);
			_logger?.LogInformation((routeOperationResult != RouteOperationResult.Success) ? $"Error occurred while adding VPN routes for IKEv2 connection. Result: {routeOperationResult}" : "VPN routes successfully added for IKEv2 connection");
		}
	}
}
