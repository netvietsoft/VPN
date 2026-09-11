using System;

namespace NextAiVPN.Streaming.Exceptions;

public class LocationListException : Exception
{
	public LocationListException()
		: base("Cant get streaming locations from the server")
	{
	}

	public LocationListException(string message)
		: base(message)
	{
	}

	public LocationListException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
