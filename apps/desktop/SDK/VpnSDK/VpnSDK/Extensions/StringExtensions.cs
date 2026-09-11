using System.Globalization;

namespace VpnSDK.Extensions;

internal static class StringExtensions
{
	public static string ToTitleCase(this string s)
	{
		return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
	}
}
