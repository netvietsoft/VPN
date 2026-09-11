using System.Text.RegularExpressions;

namespace VpnSDK.Internal.Helpers;

internal static class StringHelper
{
	private const string AllowedNameCharactersPattern = "^[a-zA-Z0-9_-]{1,256}$";

	public static string RemoveSpecialCharactersFromString(string connectionName)
	{
		return Regex.Replace(connectionName, "[^0-9a-zA-Z]+", string.Empty);
	}

	public static bool IsCalloutDriverNameValid(string calloutDriverName)
	{
		return Regex.IsMatch(calloutDriverName, "^[a-zA-Z0-9_-]{1,256}$");
	}
}
