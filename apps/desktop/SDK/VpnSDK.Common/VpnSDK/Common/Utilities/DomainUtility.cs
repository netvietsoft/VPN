using System;
using System.Collections.Generic;
using System.Linq;
using VpnSDK.Common.Settings;

namespace VpnSDK.Common.Utilities;

public class DomainUtility
{
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

	public static bool IsDomainExcludedFromVpn(string domainName, IReadOnlyList<SplitTunnelDomain>? domainsToBeExcludedFromVpn)
	{
		return domainsToBeExcludedFromVpn?.Any((SplitTunnelDomain w) => (domainName.IndexOf(w.DomainName, StringComparison.OrdinalIgnoreCase) >= 0 && w.IncludeAllSubdomains) || (domainName.Equals(w.DomainName, StringComparison.OrdinalIgnoreCase) && !w.IncludeAllSubdomains)) ?? false;
	}
}
