using System;
using System.Globalization;
using System.Windows.Data;

namespace NextAiVPN.Converters;

internal class AllLocationsPingConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			if (value == null)
			{
				return "---";
			}
			return value?.ToString() + "ms";
		}
		catch (Exception)
		{
			return "---";
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
