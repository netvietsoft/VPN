using System;

namespace NextAiVPN.Services.Persistence;

public static class VpnModeChangeEvent
{
	public static Action<VpnType> OnVpnModeChanged;
}
