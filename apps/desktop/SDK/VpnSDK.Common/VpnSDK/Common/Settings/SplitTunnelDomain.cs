namespace VpnSDK.Common.Settings;

public class SplitTunnelDomain
{
	public string DomainName { get; private set; }

	public bool IncludeAllSubdomains { get; set; }

	public SplitTunnelDomain(string domainName, bool includeAllSubdomains = false)
	{
		DomainName = domainName;
		IncludeAllSubdomains = includeAllSubdomains;
	}
}
