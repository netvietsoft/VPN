using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NextAiVPN.Converters;

internal class StreamingCountryCodeToFlagConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			ImageSource imageSource = FlagConverterHelper.GetFlagByCountryCode(value.ToString());
			if (value.ToString().ToLower().Equals("connected"))
			{
				imageSource = FlagConverterHelper.ConnectedCheckIcon;
			}
			return imageSource ?? FlagConverterHelper.UnknownCountryLogo;
		}
		catch
		{
			return FlagConverterHelper.UnknownCountryLogo;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value;
	}
}
