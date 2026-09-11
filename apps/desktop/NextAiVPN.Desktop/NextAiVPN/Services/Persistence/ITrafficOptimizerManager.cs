namespace NextAiVPN.Services.Persistence;

public interface ITrafficOptimizerManager
{
	bool IsTrafficOptimizerEnabled();

	void SetApps();
}
