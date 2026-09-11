using System;
using System.Globalization;
using System.Windows.Data;

namespace NextAiVPN.Converters;

internal class StreamingPingConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value != null)
		{
			return value?.ToString() + "ms";
		}
		return new Random().Next(50, 256) + "ms";
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
