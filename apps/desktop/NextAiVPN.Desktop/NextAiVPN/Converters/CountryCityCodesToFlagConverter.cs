using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VpnSDK.Interfaces;

namespace NextAiVPN.Converters;

internal class CountryCityCodesToFlagConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			ISDK instance = SDKInstance.GetInstance();
			ImageSource imageSource = FlagConverterHelper.GetFlagByCountryCode(values[0].ToString());
			if (instance.IsConnected && !Utils.AppSettingsHelper.GetValue("IsBestAvailable").Equals("1") && instance.ActiveConnectionInformation.Location.CountryCode.Equals(values[0].ToString()) && instance.ActiveConnectionInformation.Location.CityCode.Equals(values[1].ToString()))
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

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
