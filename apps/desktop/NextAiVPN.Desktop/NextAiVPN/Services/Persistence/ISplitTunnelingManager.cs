namespace NextAiVPN.Services.Persistence;

public interface ISplitTunnelingManager
{
	bool IsSplitTunnelingEnabled();

	void SetApps();

	void SetHostnamesAndIps();
}
