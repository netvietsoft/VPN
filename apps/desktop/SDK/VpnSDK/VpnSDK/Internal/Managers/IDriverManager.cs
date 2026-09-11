using System.Threading.Tasks;
using VpnSDK.Enums;

namespace VpnSDK.Internal.Managers;

internal interface IDriverManager
{
	bool IsDriverDetected { get; }

	Task<DriverInstallResult> InstallDriver();

	Task<DriverUninstallResult> RemoveDriver();
}
