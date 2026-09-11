using System;

namespace NextAiVPN.Services.Persistence;

public static class GlobalEvents
{
	public static event Action<Style, Style> StyleChanged;

	public static event Action LoggedOut;

	public static void RaiseStyleChanged(Style appStyle, Style systemStyle)
	{
		StyleChanged?.Invoke(appStyle, systemStyle);
	}

	public static void RaiseLoggedOut()
	{
		LoggedOut?.Invoke();
	}
}
