using System;
using System.Collections.Generic;
using System.Threading;
using VpnSDK.DTO;
using VpnSDK.Enums;

namespace VpnSDK.Internal.Configuration;

internal class SDKConfiguration : ISDKConfiguration
{
	public string ApiKey { get; internal set; }

	public string BrandingKey { get; internal set; }

	public string[] ApiBaseUrls { get; internal set; } = new string[5] { "https://api.wlvpn.com/v3", "https://toolara.net/v3", "https://sportsballhub.com/v3", "https://ipfreely.io/v3", "https://amandahugnkiss.org/v3" };

	public string LoginApiUrl { get; internal set; }

	public string AuthorizationToken { get; internal set; }

	public string ApplicationName { get; internal set; }

	public TimeSpan? KeepServerCache { get; internal set; }

	public bool AutomaticRefreshTokenHandling { get; internal set; }

	public OpenVpnConfiguration OpenVpnConfiguration { get; internal set; }

	public WireguardConfiguration WireguardConfiguration { get; internal set; }

	public Dictionary<NetworkConnectionType, bool> AvailableVpnTypes { get; internal set; }

	public RasConfiguration RasConfiguration { get; internal set; }

	public SynchronizationContext SynchronizationContext { get; internal set; }

	public Type SDKType { get; internal set; }

	public bool RunInUserspace { get; internal set; }

	public bool UseTokenAuthentication { get; set; }

	public string CalloutDriverName { get; set; }

	public string ServerListCacheDirectory { get; internal set; }
}
