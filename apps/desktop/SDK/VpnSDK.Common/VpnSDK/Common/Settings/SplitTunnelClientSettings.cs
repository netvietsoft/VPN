using System.Collections.Generic;
using System.Linq;
using VpnSDK.Common.Enums;

namespace VpnSDK.Common.Settings;

public class SplitTunnelClientSettings
{
	public bool IsEnabled { get; set; }

	public SplitTunnelMode Mode { get; set; }

	public List<SplitTunnelApp>? AllowedApps { get; set; }

	public List<SplitTunnelDomain>? AllowedDomains { get; set; }

	public bool IsDomainBasedSplitTunnelEnabled
	{
		get
		{
			if (IsEnabled && AllowedDomains != null)
			{
				return AllowedDomains.Any();
			}
			return false;
		}
	}
}
