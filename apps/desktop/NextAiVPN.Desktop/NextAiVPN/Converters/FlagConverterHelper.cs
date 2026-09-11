using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Converters;

internal static class FlagConverterHelper
{
	public static ImageSource UnknownCountryLogo => (ImageSource)Application.Current.Resources["BRANDING_UNKNOWN_COUNTRY_LOGO"];

	public static ImageSource ConnectedCheckIcon => new BitmapImage(new Uri(IconHelper.GetIcon("check_whitebg"), UriKind.Relative));

	public static ImageSource GetFlagByCountryCode(string countryCode)
	{
		string text = countryCode?.ToUpper() ?? string.Empty;
		if (string.IsNullOrEmpty(text))
		{
			text = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? "BLANK" : "BLANK_DM");
		}
		return (ImageSource)Application.Current.Resources["CountryFlag_" + text];
	}
}
