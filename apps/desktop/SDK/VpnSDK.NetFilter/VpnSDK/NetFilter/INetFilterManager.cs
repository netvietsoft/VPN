using System.Collections.Generic;
using System.Net;
using VpnSDK.NetFilter.CalloutDriver;
using VpnSDK.NetFilter.Enums;
using VpnSDK.NetFilter.Helpers;
using VpnSDK.NetFilter.Interop;

namespace VpnSDK.NetFilter;

internal interface INetFilterManager
{
	bool IsReady { get; }

	bool StartNetFilterEngine();

	bool AddSplitTunneledApplication(string processName, IPAddress internetIp);

	void ClearSplitTunneling();

	void AddNetFilterEventHandler(NetFilterEventHandlerBase handler);

	void StopNetFilterEngine();

	void StopCalloutDriverService(bool waitForStop = false);

	bool UninstallSplitTunnelDriverService(bool waitForStop = false);

	ICalloutDriverService GetCalloutDriverService();

	NetFilterInterop.NF_STATUS AddRule(NetFilterInterop.NF_RULE_EX rule, NetFilterFeature feature, NetFilterInterop.NF_APPEND append, bool applyRule = true);

	NetFilterInterop.NF_STATUS AddRules(List<NetFilterInterop.NF_RULE_EX> rules, NetFilterFeature feature, NetFilterInterop.NF_APPEND append);

	NetFilterInterop.NF_STATUS ApplyRules();

	NetFilterInterop.NF_STATUS ClearRules(NetFilterFeature features);
}
