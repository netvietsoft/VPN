using System;
using System.Globalization;
using System.Windows.Data;
using VpnSDK.Interfaces;

namespace NextAiVPN.Converters;

internal class HeaderLoadConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			ILocation location = (ILocation)((CollectionViewGroup)value).Items[0];
			if (!location.Id.Equals("bestavailable"))
			{
				return ((IRegion)location).Load + "%";
			}
			return "";
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
