using System.Globalization;

namespace NextAiVPN.Common;

public static class RequestContext
{
	public static string BuildAcceptLanguage()
	{
		try
		{
			CultureInfo installedUICulture = CultureInfo.InstalledUICulture;
			string obj = (string.IsNullOrWhiteSpace(installedUICulture?.TwoLetterISOLanguageName) ? "en" : installedUICulture.TwoLetterISOLanguageName.ToLowerInvariant());
			string text = (string.IsNullOrWhiteSpace(installedUICulture?.Name) ? "en-US" : installedUICulture.Name);
			return obj + ", " + text;
		}
		catch
		{
			return "en, en-US";
		}
	}
}
