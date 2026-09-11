using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NextAiVPN.Converters;

internal class BoolToVisibilityValueConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return false;
		}
		bool? flag = value as bool?;
		if (!flag.HasValue)
		{
			return false;
		}
		return (!flag.Value) ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
