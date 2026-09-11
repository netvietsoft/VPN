using System;
using System.Globalization;
using System.Windows.Data;

namespace NextAiVPN.Converters;

internal class DateConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			DateTime dateTime = DateTime.Parse(value.ToString(), CultureInfo.InvariantCulture);
			if (dateTime.Date == DateTime.Now.Date)
			{
				return "TODAY";
			}
			if (dateTime.Date == DateTime.Now.Date.AddDays(-1.0))
			{
				return "YESTERDAY";
			}
			return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month).ToUpper() + " " + dateTime.Day;
		}
		catch (Exception)
		{
			return value;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
