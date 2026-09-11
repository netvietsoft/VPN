namespace NextAiVPN.Services.Persistence;

public interface IDisconnectMessageBoxHelper
{
	bool ShowDisconnectMessageIfConnected(bool isStreaming = false);
}
