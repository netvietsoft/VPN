using System.Linq;
using System.Threading.Tasks;
using VpnSDK.Interfaces;

namespace VpnSDK.Helpers;

public static class BatchPingUtility
{
	public static Task Ping(params ILocation[] locations)
	{
		return Task.WhenAll(locations.Select(delegate(ILocation x)
		{
			try
			{
				return x.Ping();
			}
			catch
			{
				return (Task<ushort?>)null;
			}
		}));
	}
}
