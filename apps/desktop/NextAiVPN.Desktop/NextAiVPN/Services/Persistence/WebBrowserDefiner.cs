using NextAiVPN.Common;
using NextAiVPN.UI.NoBrowserFound;
using NextAiVPN.UI.WebView2;

namespace NextAiVPN.Services.Persistence;

internal class WebBrowserDefiner : IWebBrowserDefiner
{
	private readonly IWebView2Installer _webView2Installer;

	private readonly IBrowserVersionHelper _browserVersionHelper;

	private readonly IAppLogger _logger;

	private readonly IBrowserLinksOpener _browserLinksOpener;

	public WebBrowserDefiner(IWebView2Installer webView2Installer, IBrowserVersionHelper browserVersionHelper, IAppLogger logger, IBrowserLinksOpener browserLinksOpener)
	{
		_browserVersionHelper = browserVersionHelper;
		_webView2Installer = webView2Installer;
		_logger = logger;
		_browserLinksOpener = browserLinksOpener;
	}

	public WebBrowserResult DefineBrowser()
	{
		int edgeWebView2Version = _browserVersionHelper.GetEdgeWebView2Version();
		if (edgeWebView2Version == 0)
		{
			InstallWebView2RuntimeWindow installWebView2RuntimeWindow = new InstallWebView2RuntimeWindow(new InstallWebView2RuntimeWindowViewModel());
			installWebView2RuntimeWindow.ShowDialog();
			if (installWebView2RuntimeWindow.DialogResult)
			{
				_logger?.Information("Installing Microsoft WebView2 Runtime", "DefineBrowser", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\WebBrowserDefiner.cs", 45);
				_webView2Installer.Install();
				return WebBrowserResult.NeedToInstallWebView2Runtime;
			}
		}
		if (edgeWebView2Version >= 109)
		{
			_logger?.Information("WebView2", "DefineBrowser", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\WebBrowserDefiner.cs", 54);
			return WebBrowserResult.WebView2;
		}
		if (_browserVersionHelper.GetIeVersion() == 11001)
		{
			_logger?.Information("WebBrowser", "DefineBrowser", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\WebBrowserDefiner.cs", 63);
			return WebBrowserResult.InternetExplorer;
		}
		_logger?.Warning("Browsers not found", "DefineBrowser", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\WebBrowserDefiner.cs", 68);
		new NoBrowserFoundWindow(new NoBrowserFoundWindowViewModel(_browserLinksOpener)).ShowDialog();
		return WebBrowserResult.NotFound;
	}
}
