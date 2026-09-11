using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NextAiVPN.Converters;

public class BoolToVisibilityConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		bool num = !(bool)values[0];
		bool flag = (bool)values[1];
		if (num | flag)
		{
			return Visibility.Collapsed;
		}
		return Visibility.Visible;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
