using System;
using System.Globalization;
using System.Windows.Data;
using VpnSDK.Interfaces;

namespace NextAiVPN.Converters;

internal class LoadConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			if (!((ILocation)value).Id.Equals("bestavailable"))
			{
				return ((IRegion)value).Load + "%";
			}
			return "";
		}
		catch
		{
			return "296ms";
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
