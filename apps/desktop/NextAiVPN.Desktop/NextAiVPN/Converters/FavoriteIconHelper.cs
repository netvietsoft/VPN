using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Converters;

internal static class FavoriteIconHelper
{
	public static string GetIconPath(string id)
	{
		if (!Utils.FavoritesService.ContainFavorite(id))
		{
			return IconHelper.GetIcon("favNormal");
		}
		return "/Assets/favSelected.png";
	}
}
