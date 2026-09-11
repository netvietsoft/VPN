using ARSoft.Tools.Net.Dns;
using Microsoft.Extensions.Logging;

namespace VpnSDK.DnsResolver;

internal class CustomDnsServer : DnsServer
{
	private const int SioUdpConnectionReset = -1744830452;

	private ILogger<CustomDnsServer> _logger;

	public CustomDnsServer(ILogger<CustomDnsServer> logger, params IServerTransport[] transports)
		: base(transports)
	{
		_logger = logger;
	}

	public new void Start()
	{
		base.Start();
	}
}
