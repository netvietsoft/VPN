using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NextAiVPN.Common;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.NextAiGlobal.SignIn;

public partial class NextAiGlobalSignInWindow : Window, IComponentConnector
{
	private readonly MainWindow _mainWindow;

	private readonly ISignInService _signInService;

	private readonly IAppLogger _logger;

	private readonly IAppSettingsHelper _appSettingsHelper;

	public bool IsSystemTrayClose { get; set; }

	public bool IsSignIn { get; set; }

	public NextAiGlobalSignInWindow(ISignInService signInService, MainWindow mainWindow, IAppLogger logger, IAppSettingsHelper appSettingsHelper)
	{
		_signInService = signInService;
		_mainWindow = mainWindow;
		_logger = logger;
		_appSettingsHelper = appSettingsHelper;
		InitializeComponent();
		WindowHeader.GetParent(this);
	}

	private void SetUpWebView2()
	{
		WebView2.CreationProperties = new CoreWebView2CreationProperties
		{
			UserDataFolder = VPNConstants.FilePath.WebView2UserDataFolderPath
		};
		string uriString = new OAuth2NextAiGlobalAuthenticator(_appSettingsHelper).Authenticate();
		WebView2.Source = new Uri(uriString, UriKind.Absolute);
		WebView2.NavigationStarting += WebView2Browser_NavigationStarting;
		WebView2.NavigationCompleted += WebView2Browser_NavigationCompleted;
		WebView2.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;
		WebView2.Visibility = Visibility.Visible;
	}

	private async void WebView2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
	{
		if (e.IsSuccess)
		{
			await WebView2.CoreWebView2.Profile.ClearBrowsingDataAsync(CoreWebView2BrowsingDataKinds.AllProfile);
		}
		else
		{
			_logger?.Error("WebView2 initialization failed", "WebView2_CoreWebView2InitializationCompleted", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\NextAiGlobal\\SignIn\\NextAiGlobalSignInWindow.xaml.cs", 70);
		}
	}

	private async void WebView2Browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
	{
		if (e.IsSuccess)
		{
			ProgressBar.Visibility = Visibility.Collapsed;
			WebView2.Visibility = Visibility.Visible;
		}
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
		_mainWindow.DisposeNextAiGlobalSignInWindow();
	}

	private void DisposeWebView2()
	{
		WebView2.NavigationStarting -= WebView2Browser_NavigationStarting;
		WebView2.NavigationCompleted -= WebView2Browser_NavigationCompleted;
		WebView2.CoreWebView2InitializationCompleted -= WebView2_CoreWebView2InitializationCompleted;
		try
		{
			WebView2.Stop();
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "DisposeWebView2", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\NextAiGlobal\\SignIn\\NextAiGlobalSignInWindow.xaml.cs", 133);
		}
		WebView2.Dispose();
	}

	private void SignInWindow_OnLoaded(object sender, RoutedEventArgs e)
	{
		SetUpWebView2();
	}
}
