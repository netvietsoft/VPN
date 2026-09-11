using System.Threading.Tasks;

namespace NextAiVPN.Services;

public interface IPreferencesService
{
	void Set(string preferenceName, string preferenceValue);

	void Restore();

	void RestoreProtocol();

	Task RestoreAsync();
}
