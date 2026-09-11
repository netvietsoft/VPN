using System;
using System.Globalization;
using System.Windows.Data;
using NextAiVPN.Entities.Streaming;

namespace NextAiVPN.Converters;

internal class FavoriteStreamingLocationConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is IStreamingLocation streamingLocation)
		{
			return streamingLocation.CountryCode + " - " + streamingLocation.Country;
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
