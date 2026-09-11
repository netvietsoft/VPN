using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using DotRas;
using Microsoft.Extensions.Logging;
using VpnSDK.Common.Routing;
using VpnSDK.Common.Settings;
using VpnSDK.Private.Ras.Utilities;

namespace VpnSDK.Private.Ras;

internal class WindowsVpn : IDisposable
{
	private RasConnection _connection;

	private RasDialer _dialer;

	private bool _isDisposed;

	private readonly ILogger _logger;

	private readonly SplitTunnelClientSettings _splitTunnelClientSettings;

	private readonly List<RouteInfo> _routes;

	private readonly DnsSettings _dnsSettings;

	public WindowsVpn(string connectionName, NetworkCredential credentials, SplitTunnelClientSettings splitTunnelClientSettings, List<RouteInfo> routes, DnsSettings dnsSettings, string rasPhonebookPath = null, ILoggerFactory loggerFactory = null)
	{
		LogProvider.SetLogFactory(loggerFactory);
		_logger = LogProvider.GetLogger("VpnSDK::Private::RAS");
		_splitTunnelClientSettings = splitTunnelClientSettings;
		_routes = routes;
		_dnsSettings = dnsSettings;
		string phoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User);
		if (rasPhonebookPath != null)
		{
			phoneBookPath = rasPhonebookPath;
		}
		_dialer = new RasDialer
		{
			AutoUpdateCredentials = RasUpdateCredential.User,
			EntryName = connectionName,
			AllowUseStoredCredentials = true,
			PhoneBookPath = phoneBookPath,
			Credentials = credentials
		};
	}

	public RasConnection Connection(IPAddress serverAddress, RasConnectionType connectionType, SplitTunnelClientSettings splitTunnelClientSettings, bool allowLan, List<RouteInfo> routes, DnsSettings dnsSettings, string preSharedKey = null)
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("The object was disposed.");
		}
		return Connection(serverAddress.ToString(), connectionType, splitTunnelClientSettings, routes, dnsSettings, preSharedKey);
	}

	public RasConnection Connection(string hostName, RasConnectionType connectionType, SplitTunnelClientSettings splitTunnelClientSettings, List<RouteInfo> routes, DnsSettings dnsSettings, string preSharedKey = null)
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("The object was disposed.");
		}
		_connection = new RasConnection(_dialer, hostName, connectionType, splitTunnelClientSettings, routes, dnsSettings, preSharedKey);
		return _connection;
	}

	public RasConnection GetExistingConnection(string connectionName, NetworkCredential credentials = null)
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("The object was disposed.");
		}
		DotRas.RasConnection rasConnection = DotRas.RasConnection.GetActiveConnections().FirstOrDefault((DotRas.RasConnection x) => x.EntryName.StartsWith(connectionName));
		if (rasConnection == null)
		{
			return null;
		}
		return new RasConnection(_dialer, rasConnection, credentials, _splitTunnelClientSettings, _routes, _dnsSettings);
	}

	public static ReadOnlyCollection<DotRas.RasConnection> GetActiveConnections()
	{
		return DotRas.RasConnection.GetActiveConnections();
	}

	public void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}
		_isDisposed = true;
		try
		{
			if (_dialer != null)
			{
				_dialer.Dispose();
			}
			if (_connection != null)
			{
				_connection.Dispose();
			}
		}
		catch (Exception exception)
		{
			_logger?.LogWarning(exception, "An exception occured during RAS disposing.");
		}
		finally
		{
			_dialer = null;
			_connection = null;
		}
	}
}
