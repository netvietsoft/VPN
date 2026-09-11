using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;
using NextAiVPN.UI.NextAiGlobal.SignIn;

namespace NextAiVPN.Services;

internal class NextAiGlobalSignInService : ISignInService
{
	private readonly SDKMonitor _sdkMonitor;

	private readonly AccessTokenGenerator _accessTokenGenerator;

	private readonly MainWindow _mainWindow;

	private readonly IBugsnagService _bugsnagService;

	private readonly IApiClient _apiClient;

	private readonly IAnalyticsService _analyticsService;

	private readonly IAppLogger _logger;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private NextAiGlobalSignInWindow _nextaiglobalSignInWindow;

	public NextAiGlobalSignInService(SDKMonitor sdkMonitor, AccessTokenGenerator accessTokenGenerator, MainWindow mainWindow, IBugsnagService bugsnagService, IApiClient apiClient, IAnalyticsService analyticsService, IAppLogger logger, IAppSettingsHelper appSettingsHelper)
	{
		_sdkMonitor = sdkMonitor;
		_accessTokenGenerator = accessTokenGenerator;
		_mainWindow = mainWindow;
		_bugsnagService = bugsnagService;
		_apiClient = apiClient;
		_analyticsService = analyticsService;
		_logger = logger;
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
	}

	public async void BrowserNavigating(object sender, EventArgs e)
	{
		if (!(sender is WebView2 webView2))
		{
			if (!(sender is WebBrowser webBrowser))
			{
				return;
			}
			webBrowser.Visibility = Visibility.Collapsed;
			webBrowser.SuppressScriptErrors(hide: true);
			_nextaiglobalSignInWindow.ProgressBar.Visibility = Visibility.Visible;
			NavigatingCancelEventArgs eventArgs = (NavigatingCancelEventArgs)e;
			if (!eventArgs.Uri.OriginalString.Contains("http://localhost/sso?"))
			{
				return;
			}
			string code = SsoCallbackParser.ExtractCode(eventArgs.Uri.AbsoluteUri);
			webBrowser.Navigate("about:blank");
			NextAiTechnologyAuthResponse data = await _apiClient.GetAuthData(code);
			if (data != null)
			{
				InitSdk();
				string text = await _apiClient.GetSubscriptionStatus();
				_analyticsService.RefreshData();
				_analyticsService.SendNotification("[NextAiGlobal] Successful Sign In");
				_nextaiglobalSignInWindow.ProgressBar.Visibility = Visibility.Collapsed;
				webBrowser.Visibility = Visibility.Visible;
				if (!string.IsNullOrEmpty(text) || string.IsNullOrEmpty(data.Message))
				{
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
							_nextaiglobalSignInWindow.Close();
							_mainWindow.ShowSubscriptionExpiredWindow();
							return;
						}
					}
					else
					{
						_nextaiglobalSignInWindow.IsSignIn = true;
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
			_nextaiglobalSignInWindow.Close();
			_mainWindow.Hide();
			return;
		}
		webView2.Visibility = Visibility.Collapsed;
		_nextaiglobalSignInWindow.ProgressBar.Visibility = Visibility.Visible;
		CoreWebView2NavigationStartingEventArgs eventArgs2 = (CoreWebView2NavigationStartingEventArgs)e;
		if (eventArgs2.Uri.StartsWith("com.nextaitechnology.vpn:"))
		{
			string code2 = SsoCallbackParser.ExtractCode(eventArgs2.Uri);
			string value = _appSettingsHelper.GetValue("CodeVerifier");
			await _accessTokenGenerator.Generate(value, code2);
			NextAiGlobalTokenValidatorResponse obj = await _sdkMonitor.NextAiGlobalTokenValidator.ValidateTokenAsync();
			if (obj != null && obj.IsSuccess == 1)
			{
				_sdkMonitor.AccountTypeHelper.SetAccountType(AccountType.NextAiGlobal);
				_analyticsService.RefreshData();
				_analyticsService.SendNotification("[NextAiGlobal] Successful Sign In");
				_nextaiglobalSignInWindow.ProgressBar.Visibility = Visibility.Collapsed;
				webView2.Visibility = Visibility.Visible;
				await SubscriptionCheck(isPrelogged: false);
				_sdkMonitor.TaskBarService.SignOut = false;
				eventArgs2.Cancel = true;
				_nextaiglobalSignInWindow.Close();
				_mainWindow.Hide();
			}
			else
			{
				_mainWindow.Show();
				_nextaiglobalSignInWindow.Close();
				_sdkMonitor.TaskBarService.ContextMenuHide("quit");
			}
		}
	}

	private void InitSdk()
	{
		InitSDkIfNeeded();
		_sdkMonitor.LoginToVpn();
		CheckIfUnderstandCommitted();
	}

	private async Task SubscriptionCheck(bool isPrelogged)
	{
		SubscriptionStatus subscriptionStatus = await _sdkMonitor.VpnEntities.SubscriptionStatusHelper.GetSubscriptionStatusInfo();
		if (subscriptionStatus == null)
		{
			_logger.Error("Subscription status response is empty.", "SubscriptionCheck", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\NextAiGlobalSignInService.cs", 214);
		}
		else if (subscriptionStatus.Valid.Equals("0"))
		{
			if (!string.IsNullOrEmpty(subscriptionStatus.Status))
			{
				_logger?.Warning("NextAiGlobal subscription status. Status: " + subscriptionStatus.Status + "\nValid: " + subscriptionStatus.Valid, "SubscriptionCheck", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\NextAiGlobalSignInService.cs", 222);
				_logger?.Warning("NextAiGlobal subscription status. Error type: " + subscriptionStatus.ErrorType + "\nUser message: " + subscriptionStatus.UserMessage, "SubscriptionCheck", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\NextAiGlobalSignInService.cs", 223);
				switch (subscriptionStatus.Status)
				{
				case "needs_subscription_update":
					_sdkMonitor.VpnEntities.MainWindow.SubscriptionExpiredWindow.SetVisibleControl(isPrelogged ? "CompletePurchase" : "MainSubscription");
					_mainWindow.Hide();
					_sdkMonitor.VpnEntities.MainWindow.SubscriptionExpiredWindow.Show();
					_sdkMonitor.TaskBarService.ContextMenuHide("Quit", "Sign Out");
					break;
				case "needs_subscription":
					_mainWindow.ShowSubscriptionExpiredWindow();
					_sdkMonitor.TaskBarService.ContextMenuHide("Quit", "Sign Out");
					break;
				case "no account found for this access token. please relogin":
					MessageBox.Show("Please Re-login the application.", SystemInfo.AppName);
					break;
				}
			}
		}
		else
		{
			_logger?.Warning("NextAiGlobal subscription status. Subscription type: " + subscriptionStatus.SubscriptionType + "\nValid: " + subscriptionStatus.Valid, "SubscriptionCheck", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\NextAiGlobalSignInService.cs", 247);
			InitSdk();
			_mainWindow.Hide();
			_sdkMonitor.VpnExpandedWindow.Show();
			_sdkMonitor.VpnExpandedWindow.Mainpanel.AnimateLoadingSpinner(animate: true);
			_sdkMonitor.TaskBarService.ContextMenuShow(string.Empty);
			_sdkMonitor.VpnExpandedWindow.ExpandedSideMenu.SetMenuOption(SideMenuOption.Location);
			if (_mainWindow.IsVisible)
			{
				_mainWindow.Hide();
			}
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
		catch (Exception exception)
		{
			_bugsnagService.Notify(exception);
			_logger?.Error(exception, "InitSDkIfNeeded", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\NextAiGlobalSignInService.cs", 276);
		}
	}

	private void GoToMainPanel()
	{
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
		_nextaiglobalSignInWindow.Close();
	}

	private void CheckIfUnderstandCommitted()
	{
		if (_appSettingsHelper.GetValue("IUnderstand").Equals("0"))
		{
			_nextaiglobalSignInWindow.Hide();
			_sdkMonitor.VpnExpandedWindow.BeforeConnectWindowWrapper.ShowDialog();
		}
	}

	public void SignInValidations()
	{
		_sdkMonitor.VpnEntities.NextAiGlobalSignInWindow = new NextAiGlobalSignInWindow(this, _mainWindow, _logger, _appSettingsHelper);
		_nextaiglobalSignInWindow = _sdkMonitor.VpnEntities.NextAiGlobalSignInWindow;
		_nextaiglobalSignInWindow?.Show();
	}

	public async void CheckPrelogged()
	{
		_ = 1;
		try
		{
			if ((await _sdkMonitor.NextAiGlobalTokenValidator.ValidateTokenAsync()).IsSuccess == 1)
			{
				await SubscriptionCheck(isPrelogged: true);
			}
		}
		catch (Exception exception)
		{
			_bugsnagService.Notify(exception);
			_logger?.Error(exception, "CheckPrelogged", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\NextAiGlobalSignInService.cs", 331);
		}
		_mainWindow.EnableSignButtons();
	}
}
