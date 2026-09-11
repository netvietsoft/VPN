using NextAiVPN.Enums;

namespace NextAiVPN.Services;

public static class ConnectionStateMapper
{
	public static string ToStatusString(ConnectionState state)
	{
		return state switch
		{
			ConnectionState.Disconnected => "disconnected", 
			ConnectionState.Connecting => "connecting", 
			ConnectionState.Connected => "connected", 
			ConnectionState.Error => "error", 
			_ => null, 
		};
	}
}
