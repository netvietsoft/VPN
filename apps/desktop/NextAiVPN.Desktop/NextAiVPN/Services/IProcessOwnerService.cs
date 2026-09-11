namespace NextAiVPN.Services;

internal interface IProcessOwnerService
{
	string GetProcessOwner(int processId);

	string GetProcessOwner(string processName);
}
