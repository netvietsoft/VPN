using System.Windows.Media;

namespace NextAiVPN.Services;

public interface ICountryFlagService
{
	ImageSource GetCountryFlagImageSource(string countryId);
}
