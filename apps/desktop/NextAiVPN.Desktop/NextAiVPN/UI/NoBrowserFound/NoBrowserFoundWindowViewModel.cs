using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.NoBrowserFound;

public class NoBrowserFoundWindowViewModel
{
	private readonly IBrowserLinksOpener _browserLinksOpener;

	private NoBrowserFoundWindow _parentWindow;

	public bool DialogResult { get; set; }

	public string TitleText { get; set; } = "Supported browser not detected";

	public string BodyText { get; set; } = "You will need to install a supported browser to sign in. Please follow the instructions in the below guide.";

	public string WebView2InstallGuideLink { get; } = VPNConstants.Links.WebView2InstallGuideLink;

	public ICommand CloseXButtonClickCommand { get; set; }

	public ICommand AgreeButtonClickCommand { get; set; }

	public NoBrowserFoundWindowViewModel(IBrowserLinksOpener browserVersionHelper)
	{
		_browserLinksOpener = browserVersionHelper;
		CloseXButtonClickCommand = new ActionCommand(CloseXButtonClickCommandExecute);
		AgreeButtonClickCommand = new ActionCommand(AgreeButtonClickCommandExecute);
	}

	public void SetParentWindow(NoBrowserFoundWindow parentWindow)
	{
		_parentWindow = parentWindow;
	}

	private void CloseXButtonClickCommandExecute()
	{
		Process.GetCurrentProcess().Kill();
	}

	private void AgreeButtonClickCommandExecute()
	{
		_browserLinksOpener.OpenBrowserLink(WebView2InstallGuideLink);
		Process.GetCurrentProcess().Kill();
	}

	private void Close(bool result)
	{
		DialogResult = result;
		_parentWindow?.Close();
	}
}
