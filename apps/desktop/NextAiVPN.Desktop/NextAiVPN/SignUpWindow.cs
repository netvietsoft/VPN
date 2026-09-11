using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NextAiVPN.Entities;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN;

public partial class SignUpWindow : Window, IComponentConnector
{
	private readonly SDKMonitor _sdkMonitor;

	private readonly IWebBrowserDefiner _browserDefiner;

	private bool _flag;

	public bool IsSystemTrayClose { get; set; }

	public bool IsHidden { get; set; }

	public SignUpWindow(SDKMonitor sdkMonitor, IWebBrowserDefiner browserDefiner)
	{
		_sdkMonitor = sdkMonitor;
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
			throw new ArgumentOutOfRangeException("SignUpWindow");
		}
	}

	private void SetUpWebBrowser()
	{
		WebBrowser.Navigate(ApiEndpoints.CreateAccountPage);
		WebBrowser.LoadCompleted += WebBrowser_LoadCompleted;
		WebBrowser.Navigating += WebBrowser_Navigating;
		WebBrowser.Visibility = Visibility.Visible;
	}

	private void SetUpWebView2()
	{
		WebView2.CreationProperties = new CoreWebView2CreationProperties
		{
			UserDataFolder = VPNConstants.FilePath.WebView2UserDataFolderPath
		};
		WebView2.Source = new Uri(ApiEndpoints.CreateAccountPage, UriKind.Absolute);
		WebView2.NavigationStarting += WebView2Browser_NavigationStarting;
		WebView2.NavigationCompleted += WebView2Browser_NavigationCompleted;
		WebView2.Visibility = Visibility.Visible;
	}

	private async void WebView2Browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
	{
		if ((sender is WebView2 webView && webView.Source.ToString().Equals("about:blank")) || e.IsSuccess)
		{
			ProgressBar.Visibility = Visibility.Collapsed;
			WebView2.Visibility = Visibility.Visible;
		}
	}

	private async void WebView2Browser_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
	{
		WebView2.Visibility = Visibility.Collapsed;
		ProgressBar.Visibility = Visibility.Visible;
		if (e.Uri.Contains("http://localhost/sso?"))
		{
			string text = "code=";
			string value = "&scope=";
			int num = e.Uri.IndexOf(text);
			int num2 = e.Uri.IndexOf(value);
			string code = e.Uri.Substring(num + text.Length, num2 - num - text.Length);
			NextAiTechnologyAuthResponse obj = await Utils.Api.GetAuthData(code);
			Utils.MixpanelNotification.SendNotification("Successful Sign Up");
			if (obj != null)
			{
				_flag = true;
				_sdkMonitor.VpnEntities.MainWindow.ShowSubscriptionExpiredWindow();
				_sdkMonitor.VpnEntities.MainWindow.Hide();
			}
			e.Cancel = true;
			Close();
		}
		else if (e.Uri.Contains("?deny=1"))
		{
			e.Cancel = true;
			Close();
		}
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (IsSystemTrayClose || _sdkMonitor.VpnEntities.MainWindow.IsClosed || _sdkMonitor.VpnEntities.MainWindow == null)
		{
			return;
		}
		try
		{
			if (!_flag)
			{
				_sdkMonitor.VpnEntities.MainWindow?.Show();
			}
		}
		catch (Exception ex)
		{
			Utils.Logger.Error("SignUpWindow.Window_Closing" + ex.Message, "Window_Closing", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SignUpWindow.xaml.cs", 137);
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
			Utils.Logger?.Error(exception, "DisposeWebView2", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SignUpWindow.xaml.cs", 162);
		}
		WebView2.Dispose();
	}

	private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
	{
		if (e.Uri != new Uri("about:blank"))
		{
			WebBrowser.Visibility = Visibility.Visible;
			ProgressBar.Visibility = Visibility.Collapsed;
		}
	}

	private async void WebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
	{
		((WebBrowser)sender).SuppressScriptErrors(hide: true);
		WebBrowser.Visibility = Visibility.Collapsed;
		ProgressBar.Visibility = Visibility.Visible;
		if (e.Uri.OriginalString.Contains("http://localhost/sso?"))
		{
			string text = "code=";
			string value = "&scope=";
			int num = e.Uri.AbsoluteUri.IndexOf(text);
			int num2 = e.Uri.AbsoluteUri.IndexOf(value);
			string code = e.Uri.AbsoluteUri.Substring(num + text.Length, num2 - num - text.Length);
			WebBrowser.Navigate("about:blank");
			NextAiTechnologyAuthResponse obj = await Utils.Api.GetAuthData(code);
			Utils.MixpanelNotification.SendNotification("Successful Sign Up");
			if (obj != null)
			{
				_flag = true;
				_sdkMonitor.VpnEntities.MainWindow.ShowSubscriptionExpiredWindow();
				_sdkMonitor.VpnEntities.MainWindow.Hide();
			}
			e.Cancel = true;
			Close();
		}
		else if (e.Uri.OriginalString.Contains("?deny=1"))
		{
			e.Cancel = true;
			Close();
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		SetWebBrowser();
		Utils.MixpanelNotification.SendNotification("Sign Up");
	}
}
