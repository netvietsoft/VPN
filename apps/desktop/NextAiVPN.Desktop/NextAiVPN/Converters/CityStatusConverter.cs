using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VpnSDK.Interfaces;

namespace NextAiVPN.Converters;

internal class CityStatusConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			ISDK instance = SDKInstance.GetInstance();
			ImageSource result = new BitmapImage(new Uri("/Assets/noflag.png", UriKind.Relative));
			if (instance.IsConnected && !Utils.AppSettingsHelper.GetValue("IsBestAvailable").Equals("1") && instance.ActiveConnectionInformation.Location.CityCode == value.ToString())
			{
				result = new BitmapImage(new Uri("/Assets/check.png", UriKind.Relative));
			}
			return result;
		}
		catch
		{
			return new BitmapImage(new Uri("/Assets/noflag.png", UriKind.Relative));
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
