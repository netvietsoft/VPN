using System;

namespace NextAiVPN.Services.Persistence;

public class MessageEventArgs : EventArgs
{
	public int Message { get; }

	public DateTime Timestamp { get; }

	public MessageEventArgs(int message)
	{
		Message = message;
		Timestamp = DateTime.UtcNow;
	}
}
