namespace VpnSDK.Private.WFP.Interop;

internal struct FWPM_ACTION0
{
	public uint type;

	public GUID calloutKey;

	public static implicit operator FWPM_ACTION0(bool value)
	{
		return new FWPM_ACTION0
		{
			type = (value ? 4098u : 4097u)
		};
	}

	public static implicit operator bool(FWPM_ACTION0 value)
	{
		return value.type == 4098;
	}
}
