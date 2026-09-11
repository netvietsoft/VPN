using System.Collections.Generic;
using System.Linq;
using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP.Filtering;

internal static class LayerGuid
{
	public static readonly GUID[] IPv4 = new GUID[3]
	{
		WFPGuids.FWPM_LAYER_ALE_AUTH_CONNECT_V4,
		WFPGuids.FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4,
		WFPGuids.FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4
	};

	public static readonly GUID[] IPv6 = new GUID[3]
	{
		WFPGuids.FWPM_LAYER_ALE_AUTH_CONNECT_V6,
		WFPGuids.FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6,
		WFPGuids.FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6
	};

	public static IEnumerable<GUID> All => IPv4.Concat(IPv6);
}
