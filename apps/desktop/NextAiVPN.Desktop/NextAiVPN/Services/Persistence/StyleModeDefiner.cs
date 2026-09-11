namespace NextAiVPN.Services.Persistence;

public static class StyleModeDefiner
{
	private static IColorThemeDetector ColorThemeDetector = new ColorThemeDetector();

	public static Style AppStyle = Style.Skip;

	public static Style DefineAppStyle()
	{
		string value = Utils.AppSettingsHelper.GetValue("ThemeAppearance");
		if (!string.IsNullOrEmpty(value))
		{
			string text = value.ToLower();
			if (text == "dark")
			{
				AppStyle = Style.Dark;
				return AppStyle;
			}
			if (text == "light")
			{
				AppStyle = Style.Light;
				return AppStyle;
			}
		}
		if (!AppStyle.Equals(Style.Skip))
		{
			return AppStyle;
		}
		// Default to Clean Emerald Light theme
		AppStyle = Style.Light;
		return Style.Light;
	}

	public static Style DefineSystemStyle()
	{
		return ColorThemeDetector.GetSystemCurrentTheme();
	}
}
