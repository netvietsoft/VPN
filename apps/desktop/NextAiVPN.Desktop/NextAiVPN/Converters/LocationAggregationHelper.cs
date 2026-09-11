using System;
using System.Collections.Generic;
using System.Linq;
using VpnSDK.Interfaces;

namespace NextAiVPN.Converters;

internal static class LocationAggregationHelper
{
	public static ushort? GetMinValueForCountry(string countryCode, Func<ILocation, ushort?> selector)
	{
		IEnumerable<ILocation> enumerable = SDKInstance.GetInstance().Locations.Where((ILocation x) => x.CountryCode == countryCode);
		ushort num = 0;
		foreach (ILocation item in enumerable)
		{
			if (!item.Id.Equals("bestavailable"))
			{
				ushort? num2 = selector(item);
				if (!num2.HasValue)
				{
					return null;
				}
				if (num == 0 || num2 < num)
				{
					num = num2.Value;
				}
			}
		}
		return num;
	}
}
