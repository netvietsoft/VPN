using System;
using System.Globalization;
using System.Windows.Data;
using VpnSDK.Interfaces;

namespace NextAiVPN.Converters;

internal class BestAvailableConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			if (((ILocation)value).Id.Equals("bestavailable"))
			{
				return "Best Available";
			}
			return ((IRegion)value).Country + ": " + ((IRegion)value).City;
		}
		catch
		{
			return "";
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
