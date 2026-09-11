using System.Windows;
using System.Windows.Media;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

internal class CountryFlagService : ICountryFlagService
{
	public ImageSource GetCountryFlagImageSource(string countryId)
	{
		try
		{
			string text = countryId.ToUpper();
			if (string.IsNullOrEmpty(text))
			{
				text = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? "BLANK" : "BLANK_DM");
			}
			return ((ImageSource)Application.Current.Resources["CountryFlag_" + text]) ?? ((ImageSource)Application.Current.Resources["BRANDING_UNKNOWN_COUNTRY_LOGO"]);
		}
		catch
		{
			return (ImageSource)Application.Current.Resources["BRANDING_UNKNOWN_COUNTRY_LOGO"];
		}
	}
}
