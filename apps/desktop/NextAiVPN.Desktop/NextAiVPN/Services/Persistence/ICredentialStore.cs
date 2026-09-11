using NextAiVPN.Entities;

namespace NextAiVPN.Services.Persistence;

public interface ICredentialStore
{
	void SaveCredentials(VpnCredentials credentials);

	VpnCredentials GetCredentials();

	void DeleteCredentials();
}
