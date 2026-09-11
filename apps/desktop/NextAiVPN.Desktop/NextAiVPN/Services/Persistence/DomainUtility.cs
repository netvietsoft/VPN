using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VpnSDK.Common.Settings;

namespace NextAiVPN.Services.Persistence;

internal class DomainUtility
{
	public static bool IsValidDomain(string input, out string extractedDomain)
	{
		extractedDomain = string.Empty;
		if (string.IsNullOrWhiteSpace(input))
		{
			return false;
		}
		if (Uri.TryCreate(input, UriKind.Absolute, out Uri result))
		{
			if (result.Scheme != Uri.UriSchemeHttp && result.Scheme != Uri.UriSchemeHttps)
			{
				return false;
			}
			extractedDomain = result.Host;
		}
		else
		{
			extractedDomain = input;
		}
		return new Regex("^([a-zA-Z0-9]([a-zA-Z0-9\\-]{0,61}[a-zA-Z0-9])?\\.)+[a-zA-Z]{2,}$").IsMatch(extractedDomain);
	}

	public static string ExtractDomainName(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return string.Empty;
		}
		string text = url.Trim();
		int num = text.IndexOf("://");
		if (num != -1)
		{
			text = text.Substring(num + 3);
		}
		if (text.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase))
		{
			text = text.Substring(4);
		}
		int num2 = text.IndexOfAny(new char[2] { '/', '?' });
		if (num2 != -1)
		{
			text = text.Substring(0, num2);
		}
		return text;
	}

	public static bool IsDomainExcludedFromVpn(string domainName, IReadOnlyList<SplitTunnelDomain> domainsToBeExcludedFromVpn)
	{
		return domainsToBeExcludedFromVpn?.Any(delegate(SplitTunnelDomain w)
		{
			if (domainName.IndexOf(w.DomainName, StringComparison.OrdinalIgnoreCase) >= 0 && w.IncludeAllSubdomains)
			{
				return true;
			}
			return domainName.Equals(w.DomainName, StringComparison.OrdinalIgnoreCase) && !w.IncludeAllSubdomains;
		}) ?? false;
	}
}
