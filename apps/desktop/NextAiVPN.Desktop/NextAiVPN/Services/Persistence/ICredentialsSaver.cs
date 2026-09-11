namespace NextAiVPN.Services.Persistence;

internal interface ICredentialsSaver
{
	void Restore();

	void Save();
}
