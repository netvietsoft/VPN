using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using VpnSDK.Core.Helpers;
using VpnSDK.Core.Interop;
using VpnSDK.NetFilter.CalloutDriver;
using VpnSDK.NetFilter.Enums;
using VpnSDK.NetFilter.Helpers;
using VpnSDK.NetFilter.Interop;

namespace VpnSDK.NetFilter;

internal class NetFilterManager : INetFilterManager
{
	private const string DefaultDriverName = "IPVCalloutDriver";

	private readonly ICalloutDriverService _calloutDriverService;

	private readonly ILogger _logger;

	private readonly NetFilterEventHandler _eventHandler;

	private readonly string _driverName;

	private List<NetFilterInterop.NF_RULE_EXTENDED> rulesCache = new List<NetFilterInterop.NF_RULE_EXTENDED>();

	public bool IsReady { get; private set; }

	public NetFilterManager(ILoggerFactory loggerFactory, string driverName)
	{
		_driverName = (string.IsNullOrEmpty(driverName) ? "IPVCalloutDriver" : driverName);
		_logger = loggerFactory.CreateLogger<NetFilterManager>();
		_eventHandler = new NetFilterEventHandler(_logger);
		_calloutDriverService = new CalloutDriverService(_driverName, _logger);
	}

	public bool StartNetFilterEngine()
	{
		if (!IsReady)
		{
			if (_calloutDriverService != null)
			{
				if (StartCalloutDriverService())
				{
					_logger?.LogInformation("Callout driver service started.");
				}
				else
				{
					_logger?.LogError("Couldn't start the callout driver service.");
				}
			}
			string processArchitecture = Utils.GetProcessArchitecture(mapAsX64: true);
			if (string.IsNullOrEmpty(processArchitecture))
			{
				throw new NotSupportedException("Unsupported CPU or OS architecture");
			}
			NativeMethods.SetDllDirectory(Path.Combine(PathHelper.GetApplicationDirectory(), "Drivers", processArchitecture));
			NetFilterInterop.NF_STATUS nF_STATUS = NetFilterInterop.NFAPI.nf_init(_driverName, _eventHandler);
			if (nF_STATUS == NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS)
			{
				_logger?.LogInformation("Net Filter Engine started.");
				IsReady = true;
			}
			else
			{
				_logger?.LogError($"Net Filter Initialization failed. <{nF_STATUS}>");
			}
		}
		else
		{
			_logger?.LogInformation("Net Filter Engine is already running.");
		}
		return IsReady;
	}

	public void StopNetFilterEngine()
	{
		if (IsReady)
		{
			NetFilterInterop.NFAPI.nf_deleteBindingRules();
			NetFilterInterop.NFAPI.nf_deleteRules();
			NetFilterInterop.NFAPI.nf_free();
			rulesCache.Clear();
			IsReady = false;
			_logger?.LogInformation("Closing net filter engine");
			NativeMethods.SetDllDirectory(null);
		}
	}

	public bool AddSplitTunneledApplication(string processName, IPAddress internetIp)
	{
		NetFilterInterop.NF_STATUS nF_STATUS = NetFilterInterop.NFAPI.nf_addBindingRule(new NetFilterInterop.NF_BINDING_RULE
		{
			ip_family = (ushort)internetIp.AddressFamily,
			newLocalIpAddress = internetIp.GetAddressBytes(),
			newLocalPort = 0,
			processName = processName,
			filteringFlag = 2uL
		}, 1);
		if (nF_STATUS != NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS)
		{
			_logger?.LogError($"Error <{nF_STATUS}> on adding {processName} to filter.");
		}
		return nF_STATUS == NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS;
	}

	public void ClearSplitTunneling()
	{
		NetFilterInterop.NFAPI.nf_deleteBindingRules();
	}

	public void AddNetFilterEventHandler(NetFilterEventHandlerBase handler)
	{
		_eventHandler.AddHandler(handler);
	}

	public ICalloutDriverService GetCalloutDriverService()
	{
		return _calloutDriverService;
	}

	public NetFilterInterop.NF_STATUS AddRule(NetFilterInterop.NF_RULE_EX rule, NetFilterFeature feature, NetFilterInterop.NF_APPEND append, bool applyRule = true)
	{
		NetFilterInterop.NF_RULE_EXTENDED item = new NetFilterInterop.NF_RULE_EXTENDED
		{
			rule = rule,
			feature = (uint)feature
		};
		if (append == NetFilterInterop.NF_APPEND.NF_HEAD)
		{
			rulesCache.Insert(0, item);
		}
		else
		{
			rulesCache.Add(item);
		}
		if (applyRule)
		{
			return ApplyRules();
		}
		return NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS;
	}

	public NetFilterInterop.NF_STATUS AddRules(List<NetFilterInterop.NF_RULE_EX> rules, NetFilterFeature feature, NetFilterInterop.NF_APPEND append)
	{
		foreach (NetFilterInterop.NF_RULE_EX rule in rules)
		{
			AddRule(rule, feature, append, applyRule: false);
		}
		return ApplyRules();
	}

	public NetFilterInterop.NF_STATUS ClearRules(NetFilterFeature features)
	{
		if (rulesCache.RemoveAll((NetFilterInterop.NF_RULE_EXTENDED item) => features == (NetFilterFeature)item.feature) > 0)
		{
			return ApplyRules();
		}
		return NetFilterInterop.NF_STATUS.NF_STATUS_SUCCESS;
	}

	public NetFilterInterop.NF_STATUS ApplyRules()
	{
		List<NetFilterInterop.NF_RULE_EX> list = new List<NetFilterInterop.NF_RULE_EX>();
		foreach (NetFilterInterop.NF_RULE_EXTENDED item in rulesCache)
		{
			list.Add(item.rule);
		}
		return NetFilterInterop.NFAPI.nf_setRulesEx(list.ToArray());
	}

	public bool StartCalloutDriverService()
	{
		if (_calloutDriverService == null)
		{
			return false;
		}
		return _calloutDriverService.Start();
	}

	public void StopCalloutDriverService(bool waitForStop = false)
	{
		if (_calloutDriverService == null)
		{
			return;
		}
		try
		{
			if (_calloutDriverService.IsServiceRunning())
			{
				_calloutDriverService.Stop(waitForStop);
			}
		}
		catch (Exception exception)
		{
			_logger?.LogWarning(exception, "Error while stopping the callout driver service");
		}
	}

	public bool UninstallSplitTunnelDriverService(bool waitForStop = false)
	{
		if (_calloutDriverService == null)
		{
			return false;
		}
		_calloutDriverService.Remove(waitForStop);
		return true;
	}
}
