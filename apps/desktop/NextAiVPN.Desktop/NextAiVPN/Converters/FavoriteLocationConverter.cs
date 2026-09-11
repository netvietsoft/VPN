using System;
using System.Globalization;
using System.Windows.Data;
using VpnSDK.Interfaces;

namespace NextAiVPN.Converters;

internal class FavoriteLocationConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			ILocation location = value as ILocation;
			return location.CountryCode + " - " + location.City;
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "Convert", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Converters\\FavoriteLocationConverter.cs", 30);
			throw;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
