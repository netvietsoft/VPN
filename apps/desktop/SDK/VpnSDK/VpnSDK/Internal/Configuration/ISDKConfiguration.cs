using System;
using System.Collections.Generic;
using System.Threading;
using VpnSDK.DTO;
using VpnSDK.Enums;

namespace VpnSDK.Internal.Configuration;

internal interface ISDKConfiguration
{
	string ApiKey { get; }

	string BrandingKey { get; }

	string[] ApiBaseUrls { get; }

	string LoginApiUrl { get; }

	string AuthorizationToken { get; }

	string ApplicationName { get; }

	TimeSpan? KeepServerCache { get; }

	[Obsolete("Automatic token refresh is enabled by default. Setting this property to false does not disable automatic refresh")]
	bool AutomaticRefreshTokenHandling { get; }

	OpenVpnConfiguration OpenVpnConfiguration { get; }

	WireguardConfiguration WireguardConfiguration { get; }

	Dictionary<NetworkConnectionType, bool> AvailableVpnTypes { get; }

	RasConfiguration RasConfiguration { get; }

	SynchronizationContext SynchronizationContext { get; }

	bool RunInUserspace { get; }

	bool UseTokenAuthentication { get; set; }

	string CalloutDriverName { get; set; }

	string ServerListCacheDirectory { get; }
}
