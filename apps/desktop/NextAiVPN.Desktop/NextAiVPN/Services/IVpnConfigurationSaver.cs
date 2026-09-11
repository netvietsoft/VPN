namespace NextAiVPN.Services;

public interface IVpnConfigurationSaver
{
	void SaveKillSwitchConfiguration(bool status);

	void SaveBlockLANConfiguration(bool status);

	void SaveScrambleConfiguration(bool status);

	void SaveStartupConfiguration(bool status);

	void SaveAutoConnectConfiguration(bool status);

	void SaveProtocol(string protocol);

	void SaveOpenVpnType(string type);

	void SaveLogsEnabledConfiguration(bool status);
}
