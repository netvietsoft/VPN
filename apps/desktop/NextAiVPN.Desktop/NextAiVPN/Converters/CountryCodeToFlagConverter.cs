using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VpnSDK.Interfaces;

namespace NextAiVPN.Converters;

internal class CountryCodeToFlagConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		try
		{
			if (value == null)
			{
				return FlagConverterHelper.UnknownCountryLogo;
			}
			ISDK instance = SDKInstance.GetInstance();
			ImageSource imageSource = FlagConverterHelper.GetFlagByCountryCode(value.ToString());
			if (instance.IsConnected && !Utils.AppSettingsHelper.GetValue("IsBestAvailable").Equals("1") && instance.ActiveConnectionInformation.Location.CountryCode == value.ToString())
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
		throw new NotImplementedException();
	}
}
