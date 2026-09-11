using System.Security.Principal;

namespace NextAiVPN.Services.Persistence;

public interface IRoleChecker
{
	bool IsInRole(WindowsBuiltInRole inRole);
}
