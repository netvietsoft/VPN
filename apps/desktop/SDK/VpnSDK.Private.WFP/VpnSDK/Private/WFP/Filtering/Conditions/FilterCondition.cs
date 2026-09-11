using System.ComponentModel;
using VpnSDK.Private.WFP.Interop;

namespace VpnSDK.Private.WFP.Filtering.Conditions;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class FilterCondition
{
	internal FWPM_FILTER_CONDITION0 Condition;

	public ConditionMatchType MatchType
	{
		get
		{
			return Condition.matchType;
		}
		set
		{
			Condition.matchType = value;
		}
	}
}
