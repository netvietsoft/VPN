using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Converters;

internal class LocationCountryToFavorite : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			if (values[1] == null)
			{
				return new BitmapImage(new Uri(IconHelper.GetIcon("favNormal"), UriKind.RelativeOrAbsolute));
			}
			return new BitmapImage(new Uri(FavoriteIconHelper.GetIconPath(values[1].ToString()), UriKind.RelativeOrAbsolute));
		}
		catch
		{
			return new BitmapImage(new Uri(IconHelper.GetIcon("favNormal"), UriKind.RelativeOrAbsolute));
		}
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
