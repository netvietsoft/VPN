using System.Windows.Media;

namespace NextAiVPN.Services.Persistence;

public static class ColorConverter
{
	public static SolidColorBrush ConvertColorCodeToBrush(string colorCode)
	{
		return (SolidColorBrush)new BrushConverter().ConvertFrom(colorCode);
	}
}
