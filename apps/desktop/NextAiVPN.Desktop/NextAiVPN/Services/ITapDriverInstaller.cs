using System;
using System.Threading.Tasks;
using NextAiVPN.Enums;

namespace NextAiVPN.Services;

public interface ITapDriverInstaller
{
	event Action<TapDriverInstallStatus> InstallStatusChanged;

	bool IsTapDriverInstalled();

	Task InstallTapDriver();
}
