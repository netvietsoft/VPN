namespace NextAiVPN.Services;

public interface IFavoritesService
{
	void SaveFavorites(string location);

	bool ContainFavorite(string location);

	bool HasFavorite(string location, SDKMonitor sdkObject);
}
