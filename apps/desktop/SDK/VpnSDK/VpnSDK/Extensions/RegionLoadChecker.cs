using VpnSDK.Enums;

namespace VpnSDK.Extensions;

public static class RegionLoadChecker
{
	public static LoadLevel GetRegionLoadLevel(this ushort? value)
	{
		if (!value.HasValue)
		{
			return LoadLevel.Invalid;
		}
		if (value > 100)
		{
			return LoadLevel.Invalid;
		}
		if (value >= 85)
		{
			return LoadLevel.High;
		}
		if (value >= 50)
		{
			return LoadLevel.Medium;
		}
		return LoadLevel.Low;
	}
}
