namespace NextAiVPN.Services.Persistence;

public interface IColorThemeDetector
{
	Style GetAppCurrentTheme();

	Style GetSystemCurrentTheme();
}
