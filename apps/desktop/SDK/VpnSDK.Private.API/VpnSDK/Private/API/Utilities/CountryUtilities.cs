using System.Linq;
using ISO3166;

namespace VpnSDK.Private.API.Utilities;

internal static class CountryUtilities
{
	internal static string CodeToCountryName(string countryCode)
	{
		if (countryCode == "UK")
		{
			countryCode = "GB";
		}
		try
		{
			return Country.List.First((Country x) => x.TwoLetterCode.Equals(countryCode)).Name;
		}
		catch
		{
			return "Unknown";
		}
	}
}
