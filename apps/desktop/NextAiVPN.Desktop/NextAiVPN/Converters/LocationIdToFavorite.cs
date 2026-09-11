using System;
using System.Globalization;
using System.Windows.Data;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Converters;

internal class LocationIdToFavorite : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			if (value == null)
			{
				return IconHelper.GetIcon("favNormal");
			}
			return FavoriteIconHelper.GetIconPath(value.ToString());
		}
		catch
		{
			return IconHelper.GetIcon("favNormal");
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
