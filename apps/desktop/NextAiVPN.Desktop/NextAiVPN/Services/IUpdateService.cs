using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NextAiVPN.Services;

public interface IUpdateService
{
	event Action<string> DownloadFailed;

	void Update();

	Task CheckForUpdate(Border updateControl);

	Task<bool> IsUpdateAvailable();
}
