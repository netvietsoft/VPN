using System;
using System.Globalization;
using System.Windows.Data;
using VpnSDK.Interfaces;

namespace NextAiVPN.Converters;

internal class MinLoadConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			return LocationAggregationHelper.GetMinValueForCountry(value.ToString(), (ILocation l) => ((IRegion)l).Load).GetValueOrDefault() + "%";
		}
		catch (Exception)
		{
			return "error";
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
