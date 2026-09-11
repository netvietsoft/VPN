using System;
using System.Globalization;
using System.Windows.Data;
using VpnSDK.Interfaces;

namespace NextAiVPN.Converters;

internal class MinPingConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			if (value == null)
			{
				return "---";
			}
			ushort? minValueForCountry = LocationAggregationHelper.GetMinValueForCountry(value.ToString(), (ILocation l) => l.PingMs);
			object result;
			if (minValueForCountry.HasValue)
			{
				ushort? num = minValueForCountry;
				result = num + "ms";
			}
			else
			{
				result = "---";
			}
			return result;
		}
		catch (Exception)
		{
			return "---";
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
