using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN;

public partial class SignInWindow : Window, IComponentConnector
{
	private readonly MainWindow _mainWindow;

	private readonly IWebBrowserDefiner _browserDefiner;

	private readonly ISignInService _signInService;

	public bool IsSystemTrayClose { get; set; }

	public bool IsHidden { get; set; }

	public bool IsSignIn { get; set; }

	public SignInWindow()
	{
		InitializeComponent();
	}

	public SignInWindow(ISignInService signInService, MainWindow mainWindow, IWebBrowserDefiner browserDefiner)
	{
		_signInService = signInService;
		_mainWindow = mainWindow;
		_browserDefiner = browserDefiner;
		InitializeComponent();
		WindowHeader.GetParent(this);
	}

	private void SetWebBrowser()
	{
		switch (_browserDefiner.DefineBrowser())
		{
		case WebBrowserResult.InternetExplorer:
			SetUpWebBrowser();
			break;
		case WebBrowserResult.WebView2:
			SetUpWebView2();
			break;
		case WebBrowserResult.NeedToInstallWebView2Runtime:
		case WebBrowserResult.NotFound:
			break;
		default:
			throw new ArgumentOutOfRangeException("SignInWindow");
		}
	}

	private void SetUpWebBrowser()
	{
		WebBrowser.Navigate(ApiEndpoints.LoginPage);
		WebBrowser.LoadCompleted += LoginBrowserLoadCompleted;
		WebBrowser.Navigating += LoginBrowserNavigating;
		WebBrowser.Visibility = Visibility.Visible;
	}

	private void SetUpWebView2()
	{
		WebView2.CreationProperties = new CoreWebView2CreationProperties
		{
			UserDataFolder = VPNConstants.FilePath.WebView2UserDataFolderPath
		};
		WebView2.Source = new Uri(ApiEndpoints.LoginPage, UriKind.Absolute);
		WebView2.NavigationStarting += WebView2Browser_NavigationStarting;
		WebView2.NavigationCompleted += WebView2Browser_NavigationCompleted;
		WebView2.Visibility = Visibility.Visible;
	}

	private async void WebView2Browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
	{
		if (e.IsSuccess)
		{
			ProgressBar.Visibility = Visibility.Collapsed;
			WebView2.Visibility = Visibility.Visible;
		}
	}

	private void LoginBrowserLoadCompleted(object sender, NavigationEventArgs e)
	{
		if (e.Uri != new Uri("about:blank"))
		{
			ProgressBar.Visibility = Visibility.Collapsed;
			WebBrowser.Visibility = Visibility.Visible;
		}
	}

	private async void LoginBrowserNavigating(object sender, NavigatingCancelEventArgs e)
	{
		_signInService.BrowserNavigating(sender, e);
	}

	private async void WebView2Browser_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
	{
		_signInService.BrowserNavigating(sender, e);
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (IsSystemTrayClose)
		{
			return;
		}
		if (!IsSignIn)
		{
			if (_mainWindow == null || _mainWindow.IsClosed)
			{
				return;
			}
			_mainWindow.ShowInTaskbar = true;
			_mainWindow.Show();
		}
		DisposeWebView2();
	}

	private void DisposeWebView2()
	{
		WebView2.NavigationStarting -= WebView2Browser_NavigationStarting;
		WebView2.NavigationCompleted -= WebView2Browser_NavigationCompleted;
		try
		{
			WebView2.Stop();
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "DisposeWebView2", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SignInWindow.xaml.cs", 155);
		}
		WebView2.Dispose();
	}

	private void SignInWindow_OnLoaded(object sender, RoutedEventArgs e)
	{
		SetWebBrowser();
	}
}
