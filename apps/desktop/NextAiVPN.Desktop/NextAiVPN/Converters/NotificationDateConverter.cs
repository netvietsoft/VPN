using System;
using System.Globalization;
using System.Windows.Data;

namespace NextAiVPN.Converters;

internal class NotificationDateConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		DateTime value2 = (DateTime)value;
		if (value2.Day == DateTime.Today.Day && value2.Month == DateTime.Today.Month && value2.Year == DateTime.Today.Year)
		{
			return "TODAY, " + value2.ToShortTimeString();
		}
		if (value2.Day == DateTime.Today.AddDays(-1.0).Day && value2.Year == DateTime.Today.Year)
		{
			return "YESTERDAY, " + value2.ToShortTimeString();
		}
		return $"{value2:M}".ToUpper();
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
