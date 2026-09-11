namespace VpnSDK.Private.WFP.Interop;

internal class WFPConstants
{
	public const uint FWP_ACTION_FLAG_TERMINATING = 4096u;

	public const uint FWP_ACTION_FLAG_CALLOUT = 16384u;

	public const uint FWP_ACTION_BLOCK = 4097u;

	public const uint FWP_ACTION_PERMIT = 4098u;

	public const uint FWP_ACTION_CALLOUT_TERMINATING = 20483u;

	public const int FWPM_SESSION_FLAG_DYNAMIC = 1;

	public const int FWPM_PROVIDER_FLAG_PERSISTENT = 1;

	public const int FWPM_SUBLAYER_FLAG_PERSISTENT = 1;

	public const uint FWPM_FILTER_FLAG_CLEAR_ACTION_RIGHT = 8u;

	public const int FWPM_CALLOUT_FLAG_PERSISTENT = 65536;

	public const int FWPM_CALLOUT_FLAG_USES_PROVIDER_CONTEXT = 131072;

	public const uint FWP_E_ALREADY_EXISTS = 2150760457u;

	public const int SUBLAYER_DEFAULT_WEIGHT = 2048;

	public const int PERMANENT_SUBLAYER_WEIGHT = 2047;

	public const int CALLOUT_SUBLAYER_WEIGHT = 2048;

	public const int FWP_FILTER_ENUM_FLAG_INCLUDE_DISABLED = 16;

	public const int FWP_FILTER_ENUM_FLAG_INCLUDE_BOOTTIME = 8;

	public const int FFP_MAX_CONDITIONS = 32;
}
