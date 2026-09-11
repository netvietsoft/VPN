using System;

namespace NextAiVPN.Services;

internal class NullBugsnagService : IBugsnagService
{
	public void StartSession()
	{
	}

	public void Notify(string message)
	{
	}

	public void Notify(Exception exception)
	{
	}
}
