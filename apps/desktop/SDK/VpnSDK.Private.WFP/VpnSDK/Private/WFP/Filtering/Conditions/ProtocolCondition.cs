using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP.Filtering.Conditions;

public class ProtocolCondition : FilterCondition
{
	public ProtocolCondition(IPProtocol protocol)
	{
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_IP_PROTOCOL;
		Condition.conditionValue = protocol;
	}
}
