using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Win32;
using NextAiVPN.Common;
using NextAiVPN.Enums;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.Streaming;
using NextAiVPN.UI;

namespace NextAiVPN;

public partial class MainWindow : Window, IComponentConnector
{
	private readonly object _lockObj = new object();

	private readonly IColorThemeDetector _colorThemeDetector;

	private readonly IStyleService _styleService;

	private NextAiVPN.Services.Persistence.Style _applicationColorStyle;

	private NextAiVPN.Services.Persistence.Style _systemColorStyle;

	private readonly ISignInService _signInNextAiTechnologyService;

	private readonly ISignInService _signInNextAiGlobalService;

	private readonly SDKMonitor _sdk;

	private readonly IWebBrowserDefiner _browserDefiner;

	private readonly IConfigurationLoggerHelper _configurationLoggerHelper;

	private readonly IBugsnagService _bugsnagService;

	private readonly IPreferencesRepository _preferencesRepository;

	public SignInWindow SignInWindow;

	public SignUpWindow SignUpWindow;

	public SubscriptionExpiredWindow SubscriptionExpiredWindow;

	public bool IsClosed;

	public bool IsHidden { get; set; }

	public MainWindow()
	{
		string logFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_trace.log");
		try
		{
			System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] MainWindow.ctor starting\n");
			CheckReleaseStage();
			System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] CheckReleaseStage done\n");
			SubscribeToThemeChanges();
			System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] SubscribeToThemeChanges done\n");
			SetLogger();
			System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] SetLogger done\n");
			try { RegisterUrlScheme(); } catch (Exception regEx) { Utils.Logger?.Warning("RegisterUrlScheme failed: " + regEx.Message, "MainWindow.ctor", "MainWindow.cs", 70); }
			System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] RegisterUrlScheme done\n");
			InitializeComponent();
			System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] InitializeComponent done\n");
			this.Visibility = Visibility.Collapsed;
			this.ShowInTaskbar = false;
			EnsureIfAdditionalFilesExists();
			System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] EnsureIfAdditionalFilesExists done\n");
			VpnEntitiesServices vpnEntitiesServices = new NextAiVpnDependencyFactory(this).CreateVpnEntities();
			System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] CreateVpnEntities done\n");
			_bugsnagService = vpnEntitiesServices.BugsnagService;
			_preferencesRepository = vpnEntitiesServices.PreferencesRepository;
			_styleService = vpnEntitiesServices.StyleService;
			_colorThemeDetector = vpnEntitiesServices.ColorThemeDetector;
			SetAppSystemColors();
			_browserDefiner = vpnEntitiesServices.WebBrowserDefiner;
			ToolsControl.SetDependencies(_styleService);
			ToolsControl.DataContext = vpnEntitiesServices.MainWindowToolsControlViewModel;
			GlobalEvents.RaiseStyleChanged(_colorThemeDetector.GetAppCurrentTheme(), _colorThemeDetector.GetSystemCurrentTheme());
			_sdk = vpnEntitiesServices.Sdk;
			_configurationLoggerHelper = vpnEntitiesServices.ConfigurationLoggerHelper;
			_signInNextAiTechnologyService = vpnEntitiesServices.SignInService;
			_signInNextAiGlobalService = vpnEntitiesServices.SignInNextAiGlobalService;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			bool isLogStarted = false;
			ClearAppData(ref isLogStarted);
			StartBugsnagSession();
			ValidateNewApplicationSettings();
			if (!isLogStarted)
			{
				InitAsync();
			}
			System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] MainWindow.ctor completed successfully\n");
		}
		catch (Exception ex)
		{
			System.IO.File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] EXCEPTION in MainWindow.ctor: {ex}\n");
			Utils.Logger?.Error(ex, "MainWindow.ctor", "MainWindow.cs", 104);
			throw;
		}
	}

	private static void CheckReleaseStage()
	{
		Utils.AppSettingsHelper.SetValue(AppSettingsKeys.ReleaseStage, SystemInfo.ReleaseStage);
	}

	public void DisposeNextAiGlobalSignInWindow()
	{
		_sdk.VpnEntities.NextAiGlobalSignInWindow = null;
	}

	private static void RegisterUrlScheme()
	{
		CustomUrlSchemeRegister customUrlSchemeRegister = new CustomUrlSchemeRegister(Utils.Logger);
		customUrlSchemeRegister.RegisterCustomUrlScheme("nextaivpn", VPNConstants.FilePath.NextAiVpnExeFilePath);
		customUrlSchemeRegister.RegisterCustomUrlScheme("com.nextaitechnology.vpn", VPNConstants.FilePath.NextAiVpnExeFilePath);
	}

	private static void SetLogger()
	{
		Utils.Logger = new AppLogger(VPNConstants.FilePath.DiagnosticsFolderPath);
		Logger.GetLoggerInstance(Utils.Logger);
	}

	public SDKMonitor GetSdk()
	{
		return _sdk;
	}

	private static async Task EnsureIfAppSettingsXmlFileExist()
	{
		await Task.Run(delegate
		{
			new AppSettingsXmlFileGenerator(Utils.AppSettingsHelper).EnsureIfFileIsExist();
		});
	}

	private static async Task EnsureIfLogDbFileExist()
	{
		await Utils.PreferencesRepository.EnsureLogDbIsExist();
		await Task.Delay(100);
	}

	private static async Task EnsureIfAdditionalFilesExists()
	{
		await EnsureIfAppSettingsXmlFileExist();
		await EnsureIfLogDbFileExist();
	}

	private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		ExceptionHandle(e.ExceptionObject as Exception);
	}

	private void ExceptionHandle(Exception ex)
	{
		if (ex != null)
		{
			Utils.Logger?.Error(ex, "ExceptionHandle", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\MainWindow.xaml.cs", 153);
			Utils.BugsnagService.Notify(ex);
		}
	}

	private void SubscribeToThemeChanges()
	{
		SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
		GlobalEvents.StyleChanged += MainWindowStyleChanged;
	}

	private void SetAppSystemColors()
	{
		_applicationColorStyle = _colorThemeDetector.GetAppCurrentTheme();
		_systemColorStyle = _colorThemeDetector.GetSystemCurrentTheme();
	}

	private void StartBugsnagSession()
	{
		_bugsnagService.StartSession();
	}

	private void ClearAppData(ref bool isLogStarted)
	{
		string value = Utils.AppSettingsHelper.GetValue("IsFirstRun");
		if (!string.IsNullOrEmpty(value) && value != "1")
		{
			return;
		}
		try
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName);
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
			Utils.AppSettingsHelper.SetValue("IsFirstRun", "0");
			Utils.AppSettingsHelper.SetValue("FirstRunDate", DateTime.Now.ToString("d", CultureInfo.InvariantCulture));
		}
		catch (Exception ex)
		{
			InitAsync();
			isLogStarted = true;
			string text = "Can't clean " + SystemInfo.AppName + " cache folder " + ex.Message;
			_bugsnagService.Notify("[APICommon GetVersionLink] - " + text);
			Utils.Logger.Error(text, "ClearAppData", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\MainWindow.xaml.cs", 202);
		}
	}

	public void ValidateNewApplicationSettings()
	{
		if (Utils.AppSettingsHelper.GetValue("LastNotificationId").Equals("0"))
		{
			Utils.AppSettingsHelper.SetValue("LastNotificationId", string.Empty);
			Utils.AppSettingsHelper.SetValue("NotificationsFetched", string.Empty);
			Utils.AppSettingsHelper.SetValue("NewNotificationsCount", string.Empty);
		}
		if (Utils.AppSettingsHelper.GetValue("LastLoadedLocations").Equals("0"))
		{
			Utils.AppSettingsHelper.SetValue("LastLoadedLocations", DateTime.Now.ToString());
		}
		if (Utils.AppSettingsHelper.GetValue("sort").Equals("0"))
		{
			Utils.AppSettingsHelper.SetValue("sort", "Most recent");
		}
		if (Utils.AppSettingsHelper.GetValue("LastConnectedId").Equals("0"))
		{
			Utils.AppSettingsHelper.SetValue("LastConnectedId", string.Empty);
		}
	}

	public async Task InitAsync()
	{
		NextAiVPN.Services.NextAiNodeCollectorService.Start();
		await LoadUserPreferences();
		CheckPrelogged();
	}

	private async Task LoadUserPreferences()
	{
		_ = 4;
		try
		{
			await _preferencesRepository.RestoreCredentials();
			if (!File.Exists(PreferencesRepository.GetDbFolderPath() + PreferencesRepository.GetDbPath()))
			{
				await _preferencesRepository.InitializePreferencesDbIfNeeded();
				await EnsureIfAdditionalFilesExists();
			}
			if (_sdk.AccountTypeHelper.GetAccountType() == AccountType.NextAiTechnology)
			{
				string text = await _preferencesRepository.HasActiveUser();
				if (text != "")
				{
					await _preferencesRepository.LoadUserPreferencesToConfig(text, isSignIn: false);
				}
			}
		}
		catch (Exception exception)
		{
			Utils.Logger?.Error(exception, "LoadUserPreferences", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\MainWindow.xaml.cs", 264);
		}
	}

	public bool IsSubscriptionExpiredWindowVisible()
	{
		if (SubscriptionExpiredWindow != null)
		{
			return SubscriptionExpiredWindow.IsVisible;
		}
		return false;
	}

	private async void CheckPrelogged()
	{
		// [VI] Tự động gọi SignInNextAiTechnologyService để bypass login vào Dashboard
		// [EN] Automatically invoke SignInNextAiTechnologyService to bypass login into Dashboard
		_signInNextAiTechnologyService.CheckPrelogged();
	}

	public void DisableSignButtons()
	{
		NextAiGlobalButton.IsEnabled = false;
		NextAiTechnologyButton.IsEnabled = false;
	}

	public void EnableSignButtons()
	{
		NextAiGlobalButton.IsEnabled = true;
		NextAiTechnologyButton.IsEnabled = true;
	}

	public void ShowSubscriptionExpiredWindow()
	{
		Hide();
		_sdk.TaskBarService.ContextMenuHide("Quit", "Sign Out");
		SubscriptionExpiredWindow.SetVisibleControl("CompletePurchase");
		SubscriptionExpiredWindow.Show();
	}

	private void SignIn_Click(object sender, RoutedEventArgs e)
	{
		new SignInCommonWindow(_signInNextAiTechnologyService, _signInNextAiGlobalService, this).Show();
		Hide();
	}

	private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		SignUpWindow = new SignUpWindow(_sdk, _browserDefiner);
		SignUpWindow.Show();
		Hide();
	}

	private void ToS_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		_sdk.BrowserLinksOpener.OpenBrowserLink(1);
	}

	private void PrivacyPolicy_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		_sdk.BrowserLinksOpener.OpenBrowserLink(2);
	}

	private void MenuItem_Tos(object sender, RoutedEventArgs e)
	{
		_sdk.BrowserLinksOpener.OpenBrowserLink(1);
	}

	private void MenuItem_PrivacyPolicy(object sender, RoutedEventArgs e)
	{
		_sdk.BrowserLinksOpener.OpenBrowserLink(2);
	}

	private void MenuItem_CustomerSupport(object sender, RoutedEventArgs e)
	{
		_sdk.BrowserLinksOpener.OpenBrowserLink(3);
	}

	private void MenuItem_Quit(object sender, RoutedEventArgs e)
	{
		_sdk.TaskBarService.QuitCommonFunction();
	}

	private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(Utils.AppSettingsHelper.GetValue("ThemeAppearance")))
		{
			NextAiVPN.Services.Persistence.Style appCurrentTheme = _colorThemeDetector.GetAppCurrentTheme();
			NextAiVPN.Services.Persistence.Style systemCurrentTheme = _colorThemeDetector.GetSystemCurrentTheme();
			GlobalEvents.RaiseStyleChanged(appCurrentTheme, systemCurrentTheme);
		}
	}

	private void MainWindowStyleChanged(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style sysStyle)
	{
		SetApplicationStyles(appStyle, sysStyle);
	}

	public void SetApplicationStyles(NextAiVPN.Services.Persistence.Style appStyle, NextAiVPN.Services.Persistence.Style sysStyle)
	{
		lock (_lockObj)
		{
			if (!_styleService.HasSdkObject())
			{
				_styleService.GetSdkObject(_sdk);
			}
			if (!appStyle.Equals(NextAiVPN.Services.Persistence.Style.Skip) && !appStyle.Equals(_applicationColorStyle))
			{
				_applicationColorStyle = appStyle;
				StyleModeDefiner.AppStyle = _applicationColorStyle;
				_styleService.SetStyle(_applicationColorStyle);
				_styleService.SetContextMenuStyle(_applicationColorStyle, _sdk.TaskBarService.TaskbarIcon);
			}
		}
	}

	private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
	{
		try
		{
			WindowHeader.GetParent(this);
			RestoreThemeAppearance();
			_configurationLoggerHelper.LogAll();
			AutoBypassLogin();
		}
		catch (Exception ex)
		{
			Utils.Logger?.Error(ex, "MainWindow_OnLoaded", "MainWindow.cs", 391);
		}
	}

	private void RestoreThemeAppearance()
	{
		string text = Utils.AppSettingsHelper.GetValue("ThemeAppearance")?.ToLowerInvariant();
		NextAiVPN.Services.Persistence.Style appStyle = ((!(text == "dark")) ? ((!(text == "light")) ? NextAiVPN.Services.Persistence.Style.Light : NextAiVPN.Services.Persistence.Style.Light) : NextAiVPN.Services.Persistence.Style.Dark);
		GlobalEvents.RaiseStyleChanged(appStyle, _systemColorStyle);
	}

	private void SignUp_OnMouseEnter(object sender, MouseEventArgs e)
	{
		SignUp.TextDecorations = TextDecorations.Underline;
	}

	private void SignUp_OnMouseLeave(object sender, MouseEventArgs e)
	{
		SignUp.TextDecorations = null;
	}

	private void ToS_OnMouseEnter(object sender, MouseEventArgs e)
	{
		ToS.TextDecorations = TextDecorations.Underline;
	}

	private void ToS_OnMouseLeave(object sender, MouseEventArgs e)
	{
		ToS.TextDecorations = null;
	}

	private void PrivacyPolicyFooter_OnMouseEnter(object sender, MouseEventArgs e)
	{
		PrivacyPolicyFooter.TextDecorations = TextDecorations.Underline;
	}

	private void PrivacyPolicyFooter_OnMouseLeave(object sender, MouseEventArgs e)
	{
		PrivacyPolicyFooter.TextDecorations = null;
	}

	private void MainWindow_OnClosed(object sender, EventArgs e)
	{
		IsClosed = true;
		try
		{
			GlobalEvents.StyleChanged -= MainWindowStyleChanged;
			_sdk?.NextAiVpnSdkManager?.Dispose();
			_sdk?.StreamingSdk?.Dispose();
		}
		catch
		{
		}
	}

	private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		e.Cancel = true;
		this.Hide();
		this.Visibility = Visibility.Collapsed;
	}

	private void NextAiGlobalButton_OnClick(object sender, RoutedEventArgs e)
	{
		if (_browserDefiner.DefineBrowser() != WebBrowserResult.WebView2)
		{
			Utils.Logger.Warning("Sign in nextaiglobal. WebView2 runtime is not found", "NextAiGlobalButton_OnClick", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\MainWindow.xaml.cs", 489);
			return;
		}
		_signInNextAiGlobalService.SignInValidations();
		Hide();
	}

	private void NextAiGlobalButton_OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is Button but)
		{
			LoginButtonMouseEnter(but);
		}
	}

	private static void LoginButtonMouseEnter(Button but)
	{
		but.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#2c2c2c");
	}

	private void NextAiGlobalButton_OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is Button but)
		{
			LoginButtonMouseLeave(but);
		}
	}

	private static void LoginButtonMouseLeave(Button but)
	{
		but.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#1E1E20");
	}

	private void NextAiTechnologyButton_OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is Button but)
		{
			LoginButtonMouseEnter(but);
		}
	}

	private void NextAiTechnologyButton_OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is Button but)
		{
			LoginButtonMouseLeave(but);
		}
	}

	/// <summary>
	/// [VI] Tu dong bo qua dang nhap va nap thang giao dien Dashboard VPN.
	/// [EN] Automatically bypasses login and displays VPN Dashboard.
	/// </summary>
	private void NextAiTechnologyButton_OnClick(object sender, RoutedEventArgs e)
	{
		if (_browserDefiner.DefineBrowser() != WebBrowserResult.WebView2)
		{
			Utils.Logger.Warning("Sign in nextaitechnology. WebView2 runtime is not found", "NextAiTechnologyButton_OnClick", "MainWindow.xaml.cs", 500);
			return;
		}
		_signInNextAiTechnologyService.SignInValidations();
		Hide();
	}

	private void AutoBypassLogin()
	{
		try
		{
			_sdk?.AccountTypeHelper?.SetAccountType(NextAiVPN.Enums.AccountType.NextAiTechnology);
			if (_sdk?.VpnExpandedWindow != null)
			{
				_sdk.VpnExpandedWindow.Show();
				_sdk.VpnExpandedWindow.ExpandedSideMenu?.SetMenuOption(NextAiVPN.Enums.SideMenuOption.Location);
				this.Hide();
				this.Visibility = Visibility.Collapsed;
				this.ShowInTaskbar = false;
			}
		}
		catch (Exception ex)
		{
			Utils.Logger?.Error(ex, "AutoBypassLogin", "MainWindow.cs", 0);
		}
	}
}
