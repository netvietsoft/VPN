using System;
using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP.Filtering.Conditions;

public class LoopbackCondition : FilterCondition
{
	public LoopbackCondition()
	{
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_FLAGS;
		base.MatchType = ConditionMatchType.FlagsAnySet;
		Condition.conditionValue = 1u;
		if (Environment.OSVersion.Version >= Version.Parse("6.2.0.0"))
		{
			Condition.conditionValue.Union.uint32 |= 4194304u;
			Condition.conditionValue.Union.uint32 |= 8388608u;
		}
	}
}
