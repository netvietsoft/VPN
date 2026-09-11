using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using DotRas;
using Microsoft.Extensions.Logging;
using VpnSDK.Common.Routing;
using VpnSDK.Common.Settings;
using VpnSDK.Common.Utilities;
using VpnSDK.DTO;
using VpnSDK.Enums;
using VpnSDK.Helpers;
using VpnSDK.Interfaces;
using VpnSDK.Internal.Extensions;
using VpnSDK.Internal.Helpers;
using VpnSDK.Private.API.DTO;
using VpnSDK.Private.Ras;
using VpnSDK.Private.Ras.Utilities;

namespace VpnSDK.Internal.Managers;

internal class RasManager : IProtocolManager, IDisposable
{
	private static readonly Dictionary<string, NetworkConnectionType> MiniportIds = new Dictionary<string, NetworkConnectionType> { 
	{
		"ms_agilevpnminiport",
		NetworkConnectionType.IKEv2
	} };

	private readonly ILogger _logger;

	private readonly ILogger _eventLogger;

	private readonly RasConfiguration _rasConfiguration;

	private readonly string _currentIdentity = string.Empty;

	private VpnSDK.Private.Ras.RasConnection _rasConnection;

	private bool _isDisposed;

	private DateTime _connectionStarted;

	private ApiManager _apiManager;

	public bool IsActive
	{
		get
		{
			if (_rasConnection != null)
			{
				return _rasConnection.Connected;
			}
			return false;
		}
	}

	public Action UnexpectedDisconnect { get; set; }

	internal RasManager(ApiManager apiManager, RasConfiguration configuration)
	{
		_logger = VpnSDK.Helpers.LogProvider.GetLogger("VpnSDK::RAS");
		_eventLogger = VpnSDK.Helpers.LogProvider.GetLogger("VpnSDK::RAS::EventLog");
		_apiManager = apiManager;
		_rasConfiguration = configuration;
		RasExistingConnection.Disconnect(configuration.RasDeviceDescription);
		_logger?.LogInformation("Initialization completed.");
		_currentIdentity = WindowsIdentity.GetCurrent()?.Name ?? string.Empty;
		NetworkComponentFinder.Update();
		foreach (KeyValuePair<string, NetworkConnectionType> item in MiniportIds.Where((KeyValuePair<string, NetworkConnectionType> x) => NetworkComponentFinder.GetComponentById(x.Key) == null))
		{
			_logger?.LogError("Missing device {miniport} required for {ConnectionType}", item.Key, item.Value);
		}
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			UnexpectedDisconnect = null;
			DisposeConnection();
			_isDisposed = true;
		}
	}

	public async Task DisposeAsync()
	{
		if (!_isDisposed)
		{
			UnexpectedDisconnect = null;
			if (_rasConnection != null)
			{
				_rasConnection.UnexpectedDisconnect -= ConnectionOnUnexpectedDisconnect;
				await _rasConnection.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
				_rasConnection = null;
			}
			_isDisposed = true;
		}
	}

	public async Task Connect(Server server, IConnectionConfiguration connectionConfiguration, IUser user, SplitTunnelClientSettings splitTunnelClientSettings, DnsSettings dnsSettings, CancellationToken token = default(CancellationToken))
	{
		List<RouteInfo> routes = null;
		if (!(connectionConfiguration is IRasConnectionConfiguration rasConnectionConfiguration))
		{
			throw new InvalidCastException("connectionConfiguration should be an instance of IRasConnectionConfiguration");
		}
		_connectionStarted = DateTime.Now;
		if (_rasConnection != null)
		{
			_rasConnection.Dispose();
			_rasConnection = null;
		}
		if (splitTunnelClientSettings.IsEnabled)
		{
			List<string> list = await _apiManager.GetIkev2ProtocolConfig(token).ConfigureAwait(continueOnCapturedContext: false);
			if (list == null || !list.Any())
			{
				throw new Exception("Failed to fetch the allowed IPs from the server.");
			}
			routes = GetRouteInfos(list);
		}
		NetworkCredential vpnCredentials = user?.VpnCredential;
		if (_apiManager.IsUsingTokenAuthentication())
		{
			vpnCredentials = await _apiManager.GetIKEv2VpnTokenCredentials(server.Hostname, token).ConfigureAwait(continueOnCapturedContext: false);
		}
		for (int i = 0; i <= 5; i++)
		{
			try
			{
				_rasConnection = CreateConnection(server, rasConnectionConfiguration, vpnCredentials, splitTunnelClientSettings, routes, dnsSettings);
			}
			catch
			{
				if (i == 5)
				{
					throw;
				}
				await Task.Delay(TimeSpan.FromSeconds(1.0));
				continue;
			}
			break;
		}
		if (_rasConnection == null)
		{
			throw new InvalidOperationException("Unable to create phonebook entry.");
		}
		_rasConnection.UnexpectedDisconnect += ConnectionOnUnexpectedDisconnect;
		try
		{
			_logger?.LogInformation("Connecting to {Server}, Protocol={Protocol}", server.Hostname.Split(new char[1] { '.' })[0], connectionConfiguration.ConnectionType);
			await _rasConnection.Connect(token).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (RasDialException ex)
		{
			_logger?.LogError("RAS connection failed. Error: {ErrorCode} Extended: {ExtendedErrorCode}", ex.ErrorCode, ex.ExtendedErrorCode);
			LogLatestRasError();
			DisposeConnection();
			string text = string.Empty;
			if (ex.ErrorCode == 691 || ex.ErrorCode == 739 || ex.ErrorCode == 826 || ex.ErrorCode == 919 || ex.ErrorCode == 955 || ex.ExtendedErrorCode == -2143158251 || ex.ExtendedErrorCode == -2143157998 || ex.ExtendedErrorCode == -2143157999 || ex.Message.Contains("The authenticator rejected"))
			{
				throw new VPNAuthenticationException("Your VPN credentials are invalid. Try connecting again.", ex);
			}
			if (string.IsNullOrEmpty(ex.Message))
			{
				try
				{
					if (ex.ErrorCode != 0)
					{
						text += new Win32Exception(ex.ErrorCode).Message;
					}
					if (ex.ExtendedErrorCode != 0 && ex.ExtendedErrorCode != ex.ErrorCode)
					{
						text = text + " " + new Win32Exception(ex.ExtendedErrorCode).Message;
					}
				}
				catch
				{
				}
			}
			string text2 = ((ex.ErrorCode > 0) ? ex.ErrorCode.ToString() : $"0x{ex.ErrorCode:X8}");
			if (ex.ExtendedErrorCode != 0 && ex.ExtendedErrorCode != ex.ErrorCode)
			{
				text2 += $" / {ex.ExtendedErrorCode:X8}";
			}
			throw new VpnException("Unable to connect to the VPN server. " + text + " (" + text2 + ")", ex);
		}
		catch
		{
			DisposeConnection();
			throw;
		}
	}

	public async Task Disconnect()
	{
		try
		{
			await (_rasConnection?.Disconnect());
		}
		catch
		{
		}
		finally
		{
			DisposeConnection();
		}
	}

	private void LogLatestRasError()
	{
		try
		{
			using EventLog eventLog = EventLog.GetEventLogs().FirstOrDefault((EventLog el) => el.Log.Equals("Application", StringComparison.OrdinalIgnoreCase));
			if (eventLog == null || eventLog.Entries.Count <= 0)
			{
				return;
			}
			foreach (EventLogEntry entry in eventLog.Entries)
			{
				if (entry.TimeGenerated >= _connectionStarted && entry.Source == "RasClient")
				{
					ProcessRasErrorEntry(entry);
				}
			}
		}
		catch (Exception ex)
		{
			_logger?.LogWarning("Failed to get RAS error entry from Event Log. Exception message: " + ex.Message);
		}
	}

	private void ProcessRasErrorEntry(EventLogEntry e)
	{
		string text = e.Message;
		if (text.Contains("CoId={") || text.Contains("CoID={"))
		{
			text = text.Substring(text.IndexOf(":", StringComparison.Ordinal) + 1);
		}
		text = ((!text.Contains("The user")) ? StringExtensions.Replace(StringExtensions.Replace(text, Environment.UserDomainName + "\\" + Environment.UserName, string.Empty, StringComparison.OrdinalIgnoreCase), _currentIdentity, string.Empty, StringComparison.OrdinalIgnoreCase).Replace("by user", "by the current user").Trim() : StringExtensions.Replace(StringExtensions.Replace(text.Replace("The user", "The current user"), Environment.UserDomainName + "\\" + Environment.UserName + " ", string.Empty, StringComparison.OrdinalIgnoreCase), _currentIdentity, string.Empty, StringComparison.OrdinalIgnoreCase).Trim());
		if (text.Contains("Dial-in User ="))
		{
			text = string.Join(Environment.NewLine, from x in text.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
				where (!x.StartsWith("Ip") || !x.EndsWith("By Server")) && !x.StartsWith("Dial-in User")
				select x);
		}
		text = text.Trim();
		switch (e.EntryType)
		{
		case EventLogEntryType.Error:
			_eventLogger?.LogError(text);
			break;
		case EventLogEntryType.Warning:
			_eventLogger?.LogWarning(text);
			break;
		default:
			_eventLogger?.LogInformation(text);
			break;
		}
	}

	private void DisposeConnection()
	{
		if (_rasConnection != null)
		{
			_rasConnection.UnexpectedDisconnect -= ConnectionOnUnexpectedDisconnect;
			_rasConnection.Dispose();
			_rasConnection = null;
		}
	}

	private void ConnectionOnUnexpectedDisconnect()
	{
		LogLatestRasError();
		DisposeConnection();
		UnexpectedDisconnect?.Invoke();
	}

	private VpnSDK.Private.Ras.RasConnection CreateConnection(Server server, IRasConnectionConfiguration connectionConfiguration, NetworkCredential vpnCredentials, SplitTunnelClientSettings splitTunnelClientSettings, List<RouteInfo> routes, DnsSettings dnsSettings)
	{
		return new WindowsVpn(_rasConfiguration.RasDeviceDescription + (_rasConfiguration.UseConnectionTypeInName ? $" ({connectionConfiguration.ConnectionType})" : string.Empty), vpnCredentials, splitTunnelClientSettings, routes, dnsSettings, null, VpnSDK.Helpers.LogProvider.LoggerFactoryInstance).Connection(server.Hostname, RasConnectionType.IKEv2, splitTunnelClientSettings, routes, dnsSettings);
	}

	private List<RouteInfo> GetRouteInfos(List<string> allowedIps)
	{
		List<RouteInfo> list = new List<RouteInfo>();
		foreach (string allowedIp in allowedIps)
		{
			(IPAddress IPAddress, IPAddress SubnetMask) tuple = IpAddressUtility.ConvertCidr(allowedIp);
			IPAddress item = tuple.IPAddress;
			IPAddress item2 = tuple.SubnetMask;
			RouteInfo item3 = new RouteInfo(item, item2);
			list.Add(item3);
		}
		return list;
	}
}
