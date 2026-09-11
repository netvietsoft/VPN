using System;
using System.Net;
using Microsoft.Extensions.Logging;
using VpnSDK.Private.OpenVpn.Helpers;

namespace VpnSDK.Private.OpenVpn;

internal class OpenVpn : IDisposable
{
	private readonly OpenVpnEnvironment _environment;

	private readonly NetworkCredential _networkCredential;

	private readonly string _privateKeyPassword;

	private OpenVpnConnection _connection;

	public OpenVpn()
	{
		OpenVpnEnvironment openVpnEnvironment = new OpenVpnEnvironment();
		if (!openVpnEnvironment.Validate())
		{
			throw new InvalidOperationException("OpenVPN environment is not configured correctly.");
		}
		_environment = openVpnEnvironment;
	}

	public OpenVpn(OpenVpnEnvironment environment, NetworkCredential networkCredential, string privateKeyPassword = null, ILoggerFactory loggerFactory = null)
	{
		LogProvider.SetLogFactory(loggerFactory);
		if (!environment.Validate())
		{
			throw new InvalidOperationException("OpenVPN environment is not configured correctly.");
		}
		_environment = environment;
		_networkCredential = networkCredential;
		_privateKeyPassword = privateKeyPassword;
	}

	public OpenVpn(OpenVpnEnvironment environment, string privateKeyPassword, ILoggerFactory loggerFactory = null)
	{
		LogProvider.SetLogFactory(loggerFactory);
		if (!environment.Validate())
		{
			throw new InvalidOperationException("OpenVPN environment is not configured correctly.");
		}
		_environment = environment;
		_privateKeyPassword = privateKeyPassword;
	}

	public OpenVpnConnection Connection()
	{
		if (_connection == null || _connection.IsDisposed)
		{
			_connection = new OpenVpnConnection(_environment, _networkCredential, _privateKeyPassword);
		}
		return _connection;
	}

	public void Dispose()
	{
		if (_connection != null && _connection.IsDisposed)
		{
			_connection.Dispose();
		}
		_connection = null;
	}
}
