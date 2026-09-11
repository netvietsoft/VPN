using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using Microsoft.Extensions.Logging;
using VpnSDK.Common.Dns;
using VpnSDK.Common.Routing;
using VpnSDK.Common.Settings;
using VpnSDK.Common.Utilities;
using VpnSDK.DnsResolver.Enum;
using VpnSDK.DnsResolver.EventArgs;
using VpnSDK.DnsResolver.Factory;
using VpnSDK.DnsResolver.Resolvers;

namespace VpnSDK.DnsResolver;

internal class DnsResolver
{
	private const int RouteMetric = 28000;

	private const string SubnetMask = "255.255.255.255";

	private readonly object _routeLock = new object();

	private CancellationTokenSource? _cancellationTokenSource;

	private CancellationToken _cancellationToken;

	private readonly ILogger<DnsResolver> _logger;

	private readonly ILogger<CustomDnsServer> _customDnslogger;

	private IDnsRequestResolver? _dnsRequestResolver;

	private DnsResolverSelector? _dnsResolverSelector;

	private IReadOnlyList<SplitTunnelDomain>? _domainsToBeExcludedFromVpn;

	private ConcurrentDictionary<string, List<IPAddress>> _domainIPMapping = new ConcurrentDictionary<string, List<IPAddress>>(StringComparer.InvariantCultureIgnoreCase);

	private CustomDnsServer? _server;

	private bool _raiseDnsResolverEvents;

	public event EventHandler<SplitTunnelDomainResolvedEventArgs>? SplitTunnelDomainResolved;

	public event EventHandler<SplitTunnelDomainDnsChangedEventArgs>? SplitTunnelDomainDnsChanged;

	public DnsResolver(ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<DnsResolver>();
		_customDnslogger = loggerFactory.CreateLogger<CustomDnsServer>();
	}

	internal void StartResolver(IDnsRequestResolver dnsRequestResolver, IReadOnlyList<SplitTunnelDomain> domainsToBeExcludedFromVpn, IReadOnlyList<IPAddress> localDnsAddresses, IReadOnlyList<IPAddress> vpnDnsAddresses, bool raiseDnsResolverEvents = false)
	{
		try
		{
			if (domainsToBeExcludedFromVpn == null || domainsToBeExcludedFromVpn.Count == 0)
			{
				throw new ArgumentException("The domainsToBeExcludedFromVpn parameter cannot be null or empty.");
			}
			if (localDnsAddresses == null || localDnsAddresses.Count == 0)
			{
				throw new ArgumentException("The localDnsAddresses parameter cannot be null or empty.");
			}
			if (vpnDnsAddresses == null || vpnDnsAddresses.Count == 0)
			{
				throw new ArgumentException("The vpnDnsAddresses parameter cannot be null or empty.");
			}
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;
			_dnsRequestResolver = dnsRequestResolver;
			_domainsToBeExcludedFromVpn = domainsToBeExcludedFromVpn;
			_raiseDnsResolverEvents = raiseDnsResolverEvents;
			CreateDnsServer();
			_cancellationToken.Register(PerformCleanUp);
			Func<DomainName, DnsResolverType> resolverSelector = delegate(DomainName domainName)
			{
				DnsResolverType result = DnsResolverType.Vpn;
				if (DomainUtility.IsDomainExcludedFromVpn(SanitizeDomainName(domainName.ToString()), _domainsToBeExcludedFromVpn))
				{
					result = DnsResolverType.Local;
				}
				return result;
			};
			_dnsResolverSelector = new DnsResolverSelector(new DnsResolverFactory(), localDnsAddresses, vpnDnsAddresses, resolverSelector);
			if (_dnsResolverSelector == null)
			{
				_logger.LogError("Failed to retrieve dns resolver selector");
			}
			else if (_server != null)
			{
				_server.QueryReceived += Server_QueryReceived;
				_server.ExceptionThrown += Server_ExceptionThrown;
				_server.Start();
				_logger.LogInformation("Dns resolver started.");
			}
		}
		catch (Exception value)
		{
			_logger.LogError($"Error while starting the DNS resolver: {value}");
		}
	}

	private void OnSplitTunnelDomainResolved(SplitTunnelDomainResolvedEventArgs args)
	{
		SplitTunnelDomainResolved?.Invoke(this, args);
	}

	private void OnSplitTunnelDomainDnsChanged(SplitTunnelDomainDnsChangedEventArgs args)
	{
		SplitTunnelDomainDnsChanged?.Invoke(this, args);
	}

	private void NotifySplitTunnelDomainResolved(string domainName, List<IPAddress> ipAddresses)
	{
		SplitTunnelDomainResolvedEventArgs args = new SplitTunnelDomainResolvedEventArgs(domainName, ipAddresses);
		OnSplitTunnelDomainResolved(args);
	}

	private void NotifySplitTunnelDomainDnsChanged(string domainName, List<IPAddress> oldIpAddresses, List<IPAddress> newIpAddresses)
	{
		SplitTunnelDomainDnsChangedEventArgs args = new SplitTunnelDomainDnsChangedEventArgs(domainName, oldIpAddresses, newIpAddresses);
		OnSplitTunnelDomainDnsChanged(args);
	}

	private void CreateDnsServer()
	{
		int port = 53;
		List<IServerTransport> availableServerTransports = GetAvailableServerTransports(port);
		if (availableServerTransports.Count == 0)
		{
			Console.WriteLine("Both TCP and UDP ports are already in use.");
		}
		_server = new CustomDnsServer(_customDnslogger, availableServerTransports.ToArray());
	}

	internal void StopResolver()
	{
		_logger.LogInformation("Stopping dns resolver.");
		_cancellationTokenSource?.Cancel();
	}

	private async Task Server_QueryReceived(object sender, QueryReceivedEventArgs eventArgs)
	{
		_ = 1;
		try
		{
			if (_cancellationToken.IsCancellationRequested || _dnsResolverSelector == null)
			{
				return;
			}
			if (!(eventArgs.Query is DnsMessage dnsMessage) || !dnsMessage.Questions.Any())
			{
				_logger.LogInformation("The DNS message is either empty or does not include any queries.");
				return;
			}
			ReturnCode returnCode = ValidateDnsRequest(dnsMessage);
			DnsMessage responseToSendToClient = dnsMessage.CreateResponseInstance();
			if (!returnCode.Equals(ReturnCode.NoError))
			{
				responseToSendToClient.ReturnCode = returnCode;
				eventArgs.Response = responseToSendToClient;
				_logger.LogInformation($"The DNS message is invalid with error code: {returnCode}.");
				return;
			}
			DnsQuestion question = dnsMessage.Questions[0];
			string domainName = SanitizeDomainName(question.Name.ToString());
			if (_dnsRequestResolver != null)
			{
				DnsResolutionResult dnsResolutionResult = await _dnsRequestResolver.ResolveAsync(domainName).ConfigureAwait(continueOnCapturedContext: false);
				if (!dnsResolutionResult.IsDnsResolutionAllowedFurther)
				{
					if (dnsResolutionResult.IpAddresses != null && dnsResolutionResult.IpAddresses.Any())
					{
						foreach (IPAddress ipAddress in dnsResolutionResult.IpAddresses)
						{
							if (ipAddress != null)
							{
								responseToSendToClient.AnswerRecords.Add(new ARecord(DomainName.Parse(domainName), dnsResolutionResult.TimeToLive, ipAddress));
							}
						}
					}
					else
					{
						_logger.LogInformation("No IP addresses returned by external DNS resolver for domain : " + domainName);
					}
					eventArgs.Response = responseToSendToClient;
					responseToSendToClient.ReturnCode = ReturnCode.NoError;
					_logger.LogInformation("DNS resolution for domain: " + domainName + " was handled by external DNS resolver");
					return;
				}
			}
			if (question.RecordType == RecordType.A)
			{
				List<AddressRecordBase> list = await _dnsResolverSelector.ResolveAsync<AddressRecordBase>(question.Name, question.RecordType, question.RecordClass, _cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (list != null && list.Any())
				{
					responseToSendToClient.AnswerRecords.AddRange(list);
					eventArgs.Response = responseToSendToClient;
					responseToSendToClient.ReturnCode = ReturnCode.NoError;
					if (!_cancellationToken.IsCancellationRequested && DomainUtility.IsDomainExcludedFromVpn(domainName, _domainsToBeExcludedFromVpn))
					{
						lock (_routeLock)
						{
							BindDomainIpWithLocalAdapter(list, domainName);
						}
					}
				}
				else
				{
					eventArgs.Response = responseToSendToClient;
					_logger.LogDebug("No ARecords returned for: " + domainName);
				}
			}
			else
			{
				eventArgs.Response = responseToSendToClient;
				_logger.LogDebug($"No record ({question.RecordType}) returned for: {domainName}");
			}
		}
		catch (Exception value)
		{
			if (!_cancellationToken.IsCancellationRequested)
			{
				_logger.LogError($"Error in Server_QueryReceived: {value}");
			}
		}
	}

	private string SanitizeDomainName(string domainName)
	{
		if (string.IsNullOrEmpty(domainName))
		{
			throw new ArgumentNullException("domainName");
		}
		domainName = domainName.TrimEnd('.');
		domainName = DomainUtility.ExtractDomainName(domainName);
		return domainName;
	}

	private Task Server_ExceptionThrown(object sender, ExceptionEventArgs eventArgs)
	{
		_logger.LogError($"Error while resolving DNS: {eventArgs.Exception}");
		return Task.CompletedTask;
	}

	private ReturnCode ValidateDnsRequest(DnsMessage dnsMessage)
	{
		if (dnsMessage.Questions.Count != 1)
		{
			return ReturnCode.FormatError;
		}
		return ReturnCode.NoError;
	}

	private void PerformCleanUp()
	{
		_logger.LogInformation("Performing DNS resolver cleanup.");
		try
		{
			DeleteAllIpNicBindings();
			_domainIPMapping.Clear();
			_logger.LogInformation("Deleted all IP NIC bindings.");
			_dnsResolverSelector?.Dispose();
			_logger.LogInformation("Dns resolvers' transport and cache cleared.");
			if (_server != null)
			{
				_server.QueryReceived -= Server_QueryReceived;
				_server.ExceptionThrown -= Server_ExceptionThrown;
				_server.Stop();
				_server = null;
				_logger.LogInformation("Dns resolver stopped.");
			}
		}
		catch (Exception value)
		{
			_logger.LogError($"Error while performing DNS resolver cleanup :{value}");
		}
	}

	private void BindDomainIpWithLocalAdapter(List<AddressRecordBase> answerRecords, string domainName)
	{
		if (_cancellationToken.IsCancellationRequested)
		{
			return;
		}
		var (localInterfaceIndex, iPAddress) = NetworkInterfaceUtility.GetLocalInterfaceInfo();
		if (iPAddress == null)
		{
			_logger.LogError("Failed to locate the gateway address, preventing binding of " + domainName + " with the local adapter.");
			return;
		}
		List<IPAddress> list = answerRecords.Select((AddressRecordBase a) => a.Address).ToList();
		if (_domainIPMapping.TryGetValue(domainName, out List<IPAddress> value))
		{
			if (!ListUtils.AreListsEquivalent(value, list))
			{
				UpdateIpNicBinding(value, list, localInterfaceIndex, iPAddress.ToString());
				_domainIPMapping[domainName] = list;
				_logger.LogDebug("Updated IP addresses for domain " + domainName + ": " + string.Join(", ", list));
				if (_raiseDnsResolverEvents)
				{
					NotifySplitTunnelDomainDnsChanged(domainName, value, list);
					_logger.LogDebug("Updated whitelisted IP addresses for domain ST support for KS");
				}
			}
		}
		else
		{
			AddIpNicBinding(list, localInterfaceIndex, iPAddress.ToString());
			if (_domainIPMapping.TryAdd(domainName, list))
			{
				_logger.LogDebug("Added IP addresses for domain " + domainName + ": " + string.Join(", ", list));
			}
			if (_raiseDnsResolverEvents)
			{
				NotifySplitTunnelDomainResolved(domainName, list);
				_logger.LogDebug("Whitelisted IP addresses for domain ST support for KS");
			}
		}
	}

	private void AddIpNicBinding(List<IPAddress> ipAddresses, int localInterfaceIndex, string localInterfaceGatewayAddress)
	{
		foreach (IPAddress ipAddress in ipAddresses)
		{
			if (_cancellationToken.IsCancellationRequested)
			{
				break;
			}
			WindowsRoutingTableManager.CreateRouteIfNotExists(ipAddress.ToString(), "255.255.255.255", localInterfaceGatewayAddress, localInterfaceIndex, 28000);
		}
	}

	private void UpdateIpNicBinding(List<IPAddress> oldIpAddresses, List<IPAddress> newIpAddresses, int localInterfaceIndex, string localInterfaceGatewayAddress)
	{
		foreach (IPAddress oldIpAddress in oldIpAddresses)
		{
			DeleteIpNicBinding(localInterfaceIndex, oldIpAddress);
		}
		AddIpNicBinding(newIpAddresses, localInterfaceIndex, localInterfaceGatewayAddress);
	}

	private void DeleteIpNicBinding(int localInterfaceIndex, IPAddress ipAddress)
	{
		try
		{
			WindowsRoutingTableManager.DeleteRoute(ipAddress.ToString(), "255.255.255.255", localInterfaceIndex);
		}
		catch (Exception ex)
		{
			_logger.LogError("Error occurred while deleting IP NIC binding.Exception details: " + ex.ToString());
		}
	}

	private void DeleteLocalInterfaceRoutes(int localInterfaceIndex)
	{
		try
		{
			WindowsRoutingTableManager.DeleteRoute(28000, localInterfaceIndex);
		}
		catch (Exception ex)
		{
			_logger.LogError("Error occurred while deleting local interface route. Exception details: " + ex.ToString());
		}
	}

	private List<IServerTransport> GetAvailableServerTransports(int port)
	{
		IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, port);
		return new List<IServerTransport>
		{
			new UdpServerTransport(endpoint)
		};
	}

	private void DeleteAllIpNicBindings()
	{
		(int, IPAddress) localInterfaceInfo = NetworkInterfaceUtility.GetLocalInterfaceInfo();
		foreach (string key in _domainIPMapping.Keys)
		{
			_domainIPMapping.TryGetValue(key, out List<IPAddress> value);
			if (value == null)
			{
				continue;
			}
			foreach (IPAddress item in value)
			{
				try
				{
					DeleteIpNicBinding(localInterfaceInfo.Item1, item);
				}
				catch (Exception ex)
				{
					_logger.LogDebug($"An error occurred while deleting IP NIC binding for IP address {item}: {ex.ToString()}");
					_logger.LogError("An error occurred while deleting IP NIC binding for: " + ex.ToString());
				}
			}
		}
		if (WindowsRoutingTableManager.DoesCustomRouteExist(28000, localInterfaceInfo.Item1))
		{
			_logger.LogError("Identified custom routes and proceeding with the deletion process");
			DeleteLocalInterfaceRoutes(localInterfaceInfo.Item1);
		}
	}
}
