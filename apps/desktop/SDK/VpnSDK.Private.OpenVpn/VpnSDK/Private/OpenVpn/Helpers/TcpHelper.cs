using System.Linq;
using System.Net.NetworkInformation;

namespace VpnSDK.Private.OpenVpn.Helpers;

internal class TcpHelper
{
	public static int FindPort()
	{
		TcpConnectionInformation[] activeTcpConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
		return Enumerable.Range(30500, 65535).Except(activeTcpConnections.Select((TcpConnectionInformation x) => x.LocalEndPoint.Port)).DefaultIfEmpty(20450)
			.FirstOrDefault();
	}
}
