using System.Threading.Tasks;

namespace NextAiVPN.Services.Persistence;

public interface IPreferencesRepository
{
	Task InitializePreferencesDbIfNeeded();

	Task EnsurePreferencesTableExistsAsync();

	Task EnsureLogDbIsExist();

	Task RestoreCredentials();

	Task<bool> UserExists();

	Task<string> HasActiveUser();

	Task LoadUserPreferencesToConfig(string nickname, bool isSignIn);

	Task SaveSignInPrimaryInfo();

	Task UpdateSignInPrimaryInfo();

	void SaveSinglePreference(string preference, string value);

	Task SetUserActiveStatus(int status);

	void SaveUserPreferences(string username);

	bool SearchUserPreferences(string username);
}
