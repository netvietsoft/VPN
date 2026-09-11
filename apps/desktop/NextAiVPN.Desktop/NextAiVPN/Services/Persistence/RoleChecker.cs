using System.Security.Principal;

namespace NextAiVPN.Services.Persistence;

internal class RoleChecker : IRoleChecker
{
	public bool IsInRole(WindowsBuiltInRole inRole)
	{
		using WindowsIdentity ntIdentity = WindowsIdentity.GetCurrent();
		return new WindowsPrincipal(ntIdentity).IsInRole(inRole);
	}
}
