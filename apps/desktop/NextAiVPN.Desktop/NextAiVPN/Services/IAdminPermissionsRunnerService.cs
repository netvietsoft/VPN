using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

public interface IAdminPermissionsRunnerService
{
	bool TryToRun(RunAsAdminOption option);

	bool TryToRunOnStartUp();
}
