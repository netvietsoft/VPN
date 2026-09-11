using System.Globalization;
using NextAiVPN.Properties;

namespace NextAiVPN.Services.Persistence;

public static class IconHelper
{
	public static string GetIcon(string iconName)
	{
		if (!StyleModeDefiner.DefineAppStyle().Equals(Style.Light))
		{
			return string.Format(CultureInfo.InvariantCulture, Resources.IconFolderPathDarkMode, iconName);
		}
		return string.Format(CultureInfo.InvariantCulture, Resources.IconFolderPathLightMode, iconName);
	}
}
