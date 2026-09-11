using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VpnSDK.Interfaces;

namespace VpnSDK.Extensions;

public static class LocationExtensions
{
	public static Task Ping(this ReadOnlyObservableCollection<ILocation> list)
	{
		List<ILocation> list2 = list?.ToList();
		if (list2 == null)
		{
			return null;
		}
		return Task.WhenAll(list2.Select(delegate(ILocation x)
		{
			try
			{
				return x?.Ping();
			}
			catch
			{
				return (Task<ushort?>)null;
			}
		}));
	}
}
