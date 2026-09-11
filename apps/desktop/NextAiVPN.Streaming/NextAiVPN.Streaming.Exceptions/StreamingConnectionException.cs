using System;

namespace NextAiVPN.Streaming.Exceptions;

public class StreamingConnectionException : Exception
{
	private const string DefaultMessage = "Can't connect to the streaming location.";

	public StreamingConnectionException()
		: base("Can't connect to the streaming location.")
	{
	}

	public StreamingConnectionException(string message)
		: base(message)
	{
	}

	public StreamingConnectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
