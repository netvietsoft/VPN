using System;
using System.Globalization;
using System.Windows.Data;

namespace NextAiVPN.Converters;

internal class LocationsLoadConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value != null && !string.IsNullOrEmpty(value.ToString()))
		{
			return value?.ToString() + "%";
		}
		return string.Empty;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
