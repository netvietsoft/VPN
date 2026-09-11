using System;

namespace NextAiVPN.Services;

public static class TrustedEndpoints
{
	public const string VersionCheckUrl = "https://vpn.ncapi.io/appversion";

	public const string ExchangeCodeUrl = "https://vpn.ncapi.io/api/v1/auth/exchange_code";

	public const string CheckSubscriptionUrl = "https://vpn.ncapi.io/api/v3/auth/check";

	private static readonly string[] TrustedDownloadHosts = new string[2] { "ncapi.io", "microsoft.com" };

	public static bool IsTrustedDownloadUrl(string url)
	{
		if (string.IsNullOrWhiteSpace(url))
		{
			return false;
		}
		if (!Uri.TryCreate(url, UriKind.Absolute, out Uri result))
		{
			return false;
		}
		if (!string.Equals(result.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
		{
			return false;
		}
		string host = result.Host;
		string[] trustedDownloadHosts = TrustedDownloadHosts;
		foreach (string text in trustedDownloadHosts)
		{
			if (string.Equals(host, text, StringComparison.OrdinalIgnoreCase) || host.EndsWith("." + text, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsUpgrade(string currentVersion, string candidateVersion)
	{
		if (!Version.TryParse(currentVersion, out Version result) || !Version.TryParse(candidateVersion, out Version result2))
		{
			return false;
		}
		return result2 > result;
	}
}
