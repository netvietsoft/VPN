using System;
using System.Globalization;
using System.Windows.Data;

namespace NextAiVPN.Converters;

internal class LocationsCityFavoriteSortConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			return new Random().Next(0, 2);
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "Convert", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Converters\\LocationsCityFavoriteSortConverter.cs", 18);
			return 0;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
