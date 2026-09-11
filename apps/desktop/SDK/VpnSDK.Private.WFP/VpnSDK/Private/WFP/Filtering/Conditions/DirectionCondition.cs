using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP.Filtering.Conditions;

public class DirectionCondition : FilterCondition
{
	public TrafficDirection Direction { get; private set; }

	public DirectionCondition(TrafficDirection direction)
	{
		Direction = direction;
		Condition.fieldKey = WFPGuids.FWPM_CONDITION_DIRECTION;
		Condition.conditionValue = Direction;
	}
}
