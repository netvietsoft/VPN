namespace VpnSDK.Private.WFP.Filtering.Conditions;

public enum ConditionMatchType
{
	Equal,
	GreaterThan,
	LessThan,
	GreaterThanOrEqual,
	LessThanOrEqual,
	Range,
	FlagsAllSet,
	FlagsAnySet,
	FlagsNoneSet,
	EqualCaseInsensitive,
	NotEqual
}
