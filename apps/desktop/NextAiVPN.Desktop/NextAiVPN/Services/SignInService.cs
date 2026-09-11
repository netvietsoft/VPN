using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

public class SignInService : ISignInService
{
	private readonly SDKMonitor _sdkMonitor;

	private readonly IWebBrowserDefiner _browserDefiner;

	private readonly IBugsnagService _bugsnagService;

	private readonly IApiClient _apiClient;

	private readonly IAnalyticsService _analyticsService;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly MainWindow _mainWindow;

	public SignInService(SDKMonitor sdkMonitor, IWebBrowserDefiner browserDefiner, IBugsnagService bugsnagService, IApiClient apiClient, IAnalyticsService analyticsService, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_sdkMonitor = sdkMonitor;
		_browserDefiner = browserDefiner;
		_bugsnagService = bugsnagService;
		_apiClient = apiClient;
		_analyticsService = analyticsService;
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
		_mainWindow = _sdkMonitor.VpnEntities.MainWindow;
	}

	public async void BrowserNavigating(object sender, EventArgs e)
	{
		NextAiTechnologyAuthResponse data;
		if (!(sender is WebView2 webView2))
		{
			if (!(sender is WebBrowser webBrowser))
			{
				return;
			}
			webBrowser.Visibility = Visibility.Collapsed;
			webBrowser.SuppressScriptErrors(hide: true);
			_sdkMonitor.VpnEntities.SignInWindow.ProgressBar.Visibility = Visibility.Visible;
			NavigatingCancelEventArgs eventArgs = (NavigatingCancelEventArgs)e;
			if (!eventArgs.Uri.OriginalString.Contains("http://localhost/sso?"))
			{
				return;
			}
			string code = SsoCallbackParser.ExtractCode(eventArgs.Uri.AbsoluteUri);
			webBrowser.Navigate("about:blank");
			data = await _apiClient.GetAuthData(code);
			if (data != null)
			{
				SaveCredentials(data);
				string text = await _apiClient.GetSubscriptionStatus();
				InitSdk(text);
				_analyticsService.RefreshData();
				_analyticsService.SendNotification("Successful Sign In");
				_sdkMonitor.VpnEntities.SignInWindow.ProgressBar.Visibility = Visibility.Collapsed;
				webBrowser.Visibility = Visibility.Visible;
				if (!string.IsNullOrEmpty(text) || string.IsNullOrEmpty(data.Message))
				{
					_sdkMonitor.AccountTypeHelper.SetAccountType(AccountType.NextAiTechnology);
					if (text == "Error")
					{
						new SubscriptionValidationError().ShowDialog();
						_sdkMonitor.TaskBarService.ContextMenuHide("quit", "sign out");
						_mainWindow.Show();
					}
					else if (text != "1")
					{
						if (!_appSettingsHelper.GetValue("subscription_type").Equals("0"))
						{
							switch (text)
							{
							case "500":
								MessageBox.Show("Internal Server Error, please try later.", SystemInfo.AppName);
								_apiClient.LogOut(_sdkMonitor);
								_mainWindow.Show();
								break;
							case "Relogin":
							case "400":
								MessageBox.Show("Please Re-login the application.", SystemInfo.AppName);
								_apiClient.LogOut(_sdkMonitor);
								_mainWindow.Show();
								break;
							case "NoSubscription":
							case "0":
								_sdkMonitor.VpnEntities.MainWindow.SubscriptionExpiredWindow.SetVisibleControl(_appSettingsHelper.GetValue("IsPurchaseStarted").Equals("1") ? "CompletePurchase" : "MainSubscription");
								_sdkMonitor.VpnEntities.MainWindow.SubscriptionExpiredWindow.Show();
								break;
							}
						}
						else if (_appSettingsHelper.GetValue("subscription_type").Equals("0"))
						{
							_sdkMonitor.VpnEntities.SignInWindow.Close();
							_mainWindow.ShowSubscriptionExpiredWindow();
							return;
						}
					}
					else
					{
						_sdkMonitor.VpnEntities.SignInWindow.IsSignIn = true;
						GoToMainPanel();
					}
				}
				else
				{
					_bugsnagService.Notify("[Sign in] - Unable to get subscription status");
					MessageBox.Show("Unable to get subscription status.");
					_sdkMonitor.TaskBarService.ContextMenuHide("quit", "sign out");
					_mainWindow.Show();
				}
				_sdkMonitor.TaskBarService.SignOut = false;
			}
			else
			{
				_mainWindow.Show();
				_sdkMonitor.TaskBarService.ContextMenuHide("quit");
			}
			eventArgs.Cancel = true;
			_sdkMonitor.VpnEntities.SignInWindow.Close();
			_mainWindow.Hide();
			return;
		}
		webView2.Visibility = Visibility.Collapsed;
		_sdkMonitor.VpnEntities.SignInWindow.ProgressBar.Visibility = Visibility.Visible;
		CoreWebView2NavigationStartingEventArgs eventArgs2 = (CoreWebView2NavigationStartingEventArgs)e;
		if (!eventArgs2.Uri.Contains("http://localhost/sso?"))
		{
			return;
		}
		string code2 = SsoCallbackParser.ExtractCode(eventArgs2.Uri);
		data = await _apiClient.GetAuthData(code2);
		if (data != null)
		{
			SaveCredentials(data);
			_sdkMonitor.AccountTypeHelper.SetAccountType(AccountType.NextAiTechnology);
			string text2 = await _apiClient.GetSubscriptionStatus();
			InitSdk(text2);
			_analyticsService.RefreshData();
			_analyticsService.SendNotification("Successful Sign In");
			_sdkMonitor.VpnEntities.SignInWindow.ProgressBar.Visibility = Visibility.Collapsed;
			webView2.Visibility = Visibility.Visible;
			if (!string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(data.Message))
			{
				if (text2 == "Error")
				{
					new SubscriptionValidationError().ShowDialog();
					_sdkMonitor.TaskBarService.ContextMenuHide("quit", "sign out");
					_mainWindow.Show();
				}
				else if (text2 != "1")
				{
					if (!_appSettingsHelper.GetValue("subscription_type").Equals("0"))
					{
						switch (text2)
						{
						case "500":
							MessageBox.Show("Internal Server Error, please try later.", SystemInfo.AppName);
							_apiClient.LogOut(_sdkMonitor);
							_mainWindow.Show();
							break;
						case "Relogin":
						case "400":
							MessageBox.Show("Please Re-login the application.", SystemInfo.AppName);
							_apiClient.LogOut(_sdkMonitor);
							_mainWindow.Show();
							break;
						case "NoSubscription":
						case "0":
							_sdkMonitor.VpnEntities.MainWindow.SubscriptionExpiredWindow.SetVisibleControl(_appSettingsHelper.GetValue("IsPurchaseStarted").Equals("1") ? "CompletePurchase" : "MainSubscription");
							_sdkMonitor.VpnEntities.MainWindow.SubscriptionExpiredWindow.Show();
							break;
						}
					}
					else if (_appSettingsHelper.GetValue("subscription_type").Equals("0"))
					{
						_sdkMonitor.VpnEntities.SignInWindow.Close();
						_mainWindow.ShowSubscriptionExpiredWindow();
						return;
					}
				}
				else
				{
					_sdkMonitor.VpnEntities.SignInWindow.IsSignIn = true;
					GoToMainPanel();
				}
			}
			else
			{
				_bugsnagService.Notify("[Sign in] - Unable to get subscription status");
				MessageBox.Show("Unable to get subscription status.");
				_sdkMonitor.TaskBarService.ContextMenuHide("quit", "sign out");
				_mainWindow.Show();
			}
			_sdkMonitor.TaskBarService.SignOut = false;
		}
		else
		{
			_mainWindow.Show();
			_sdkMonitor.TaskBarService.ContextMenuHide("quit");
		}
		eventArgs2.Cancel = true;
		_sdkMonitor.VpnEntities.SignInWindow.Close();
		_mainWindow.Hide();
	}

	private void SaveCredentials(NextAiTechnologyAuthResponse data)
	{
		_sdkMonitor?.CredentialStore?.SaveCredentials(new VpnCredentials(data.VpnUsername, data.VpnPassword));
	}

	private void InitSdk(string subscriptionStatus)
	{
		if (subscriptionStatus.Equals("1"))
		{
			InitSDkIfNeeded();
			_sdkMonitor.LoginToVpn();
			CheckIfUnderstandCommitted();
		}
	}

	public async void CheckPrelogged()
	{
		// [VI] Tự động Bypass Login để vào thẳng Dashboard NextAI VPN
		// [EN] Automatically Bypass Login to enter NextAI VPN Dashboard directly
		try
		{
			_mainWindow?.Hide();
			if (_mainWindow != null)
			{
				_mainWindow.Visibility = System.Windows.Visibility.Collapsed;
				_mainWindow.ShowInTaskbar = false;
			}
		}
		catch { }

		_appSettingsHelper.SetValue("id_token", "nextai_bypass_token");
		_appSettingsHelper.SetValue("access_token", "nextai_bypass_access_token");
		_appSettingsHelper.SetValue("nickname", "NextAiUser");
		_appSettingsHelper.SetValue("IUnderstand", "1");
		_appSettingsHelper.SetValue("IsLoggedIn", "1");
		_sdkMonitor.AccountTypeHelper.SetAccountType(AccountType.NextAiTechnology);

		InitSDkIfNeeded();

		if (_sdkMonitor.VpnExpandedWindow != null && !_sdkMonitor.VpnExpandedWindow.IsVisible)
		{
			_sdkMonitor.VpnExpandedWindow.Show();
			_sdkMonitor.VpnExpandedWindow.ExpandedSideMenu?.SetMenuOption(SideMenuOption.Location);
		}

		try
		{
			await _sdkMonitor.LoginToVpn();
		}
		catch (Exception ex)
		{
			_logger?.Warning("[CheckPrelogged] LoginToVpn error: " + ex.Message);
		}

		if (_appSettingsHelper.GetValue("OpenTab").ToLower().Equals("4"))
		{
			_sdkMonitor.VpnExpandedWindow?.ExpandedSideMenu?.OpenProtocolSettings();
			_appSettingsHelper.SetValue("OpenTab", "1");
		}
		else
		{
			_sdkMonitor.VpnExpandedWindow?.ExpandedSideMenu?.SetMenuOption(SideMenuOption.Location);
		}
		if (_sdkMonitor.VpnExpandedWindow != null && !_sdkMonitor.VpnExpandedWindow.IsVisible)
		{
			_sdkMonitor.VpnExpandedWindow.Show();
		}
		try
		{
			_mainWindow?.Hide();
		}
		catch { }
	}

	public async void SignInValidations()
	{
		if (!string.IsNullOrEmpty(_appSettingsHelper.GetValue("id_token")))
		{
			CheckIfUnderstandCommitted();
			if (_appSettingsHelper.GetValue("IsPurchaseStarted").Equals("1"))
			{
				switch (await _apiClient.GetSubscriptionStatus())
				{
				case "0":
				case "NoSubscription":
					_mainWindow.ShowSubscriptionExpiredWindow();
					break;
				case "Error":
					new SubscriptionValidationError().ShowDialog();
					break;
				case "1":
					_apiClient.SetIsPurchaseStarted("0");
					break;
				case "500":
					MessageBox.Show("Internal Server Error, please try later.", SystemInfo.AppName);
					break;
				case "400":
				case "Relogin":
					MessageBox.Show("Please Re-login the application.", SystemInfo.AppName);
					break;
				}
			}
			else
			{
				_mainWindow.ShowSubscriptionExpiredWindow();
				_logger?.Error("IsPurchaseStarted = 0", "SignInValidations", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\SignInService.cs", 376);
			}
			_mainWindow.Hide();
		}
		else
		{
			_sdkMonitor.VpnEntities.SignInWindow = new SignInWindow(this, _mainWindow, _browserDefiner);
			_sdkMonitor.VpnEntities.SignInWindow.Show();
			_mainWindow.Hide();
		}
	}

	private void InitSDkIfNeeded()
	{
		try
		{
			if (_sdkMonitor.NextAiVpnSdkManager.IsDisposed)
			{
				_sdkMonitor.InitSdk();
			}
		}
		catch (Exception ex)
		{
			string message = "SignInError - SignInService.InitSDkIfNeeded()" + ex.Message;
			_bugsnagService.Notify(message);
			_logger?.Error(message, "InitSDkIfNeeded", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\SignInService.cs", 403);
		}
	}

	private void GoToMainPanel()
	{
		_sdkMonitor.AccountTypeHelper.SetAccountType(AccountType.NextAiTechnology);
		if (!_appSettingsHelper.GetValue("IUnderstand").Equals("0"))
		{
			_sdkMonitor.TaskBarService.ContextMenuShow(string.Empty);
			if (!_sdkMonitor.VpnExpandedWindow.IsVisible)
			{
				_sdkMonitor.VpnExpandedWindow.Show();
			}
			_sdkMonitor.VpnExpandedWindow.Mainpanel.AnimateLoadingSpinner(animate: true);
			_sdkMonitor.VpnExpandedWindow.ExpandedSideMenu.SetMenuOption(SideMenuOption.Location);
		}
		if (_mainWindow.IsVisible)
		{
			_mainWindow.Hide();
		}
		_sdkMonitor.VpnEntities.SignInWindow.Close();
	}

	private void CheckIfUnderstandCommitted()
	{
		if (_appSettingsHelper.GetValue("IUnderstand").Equals("0"))
		{
			_sdkMonitor.VpnEntities.SignInWindow.Hide();
			_sdkMonitor.VpnExpandedWindow.BeforeConnectWindowWrapper.ShowDialog();
		}
	}
}
