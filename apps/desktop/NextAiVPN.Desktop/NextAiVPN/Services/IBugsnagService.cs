using System;

namespace NextAiVPN.Services;

public interface IBugsnagService
{
	void StartSession();

	void Notify(string message);

	void Notify(Exception exception);
}
