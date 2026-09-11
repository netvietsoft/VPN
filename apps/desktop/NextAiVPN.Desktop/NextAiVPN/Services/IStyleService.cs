using System.Windows.Controls;
using H.NotifyIcon;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

public interface IStyleService
{
	void SetStyle(Style style);

	void SetTaskbarIconStyle(Style style, TaskbarIcon taskBar, string connectionStatus);

	void SetContextMenuStyle(Style style, TaskbarIcon taskBar);

	void SetContextMenuStyle(Style style, ContextMenu contextMenu);

	void GetSdkObject(SDKMonitor sdkMonitor);

	bool HasSdkObject();
}
