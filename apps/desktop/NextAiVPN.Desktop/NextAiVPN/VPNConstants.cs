using System;
using System.IO;
using System.Reflection;

namespace NextAiVPN;

internal static class VPNConstants
{
	public static class FileName
	{
		public const string AppLogsDbFileName = "AppLogsDB.db";

		public const string UserPreferencesFileName = "UserPreferences.sqlite";

		public const string VpnHostServiceSourceFileName = "VpnHostService.fvpn";

		public const string VpnHostServiceExeFileName = "VpnHostService.exe";
	}

	public static class FilePath
	{
		public static string PreferencesDatabaseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "Preferences");

		public static string DiagnosticsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name, "Diagnostics");

		public static string WireGuardDictionaryFilePath = Path.Combine(Directory.GetCurrentDirectory(), "WireGuard");

		public static string NextAiVpnExeFilePath = Path.Combine(Directory.GetCurrentDirectory(), SystemInfo.AppName + ".exe");

		public static string AppLogsDbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "AppLogsDB.db");

		public static string AppSettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "AppSettings.xml");

		public static string UserPreferencesFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "UserPreferences.xml");

		public static string NotificationsJsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "NotificationsJson.json");

		public static string NextAiVpnInstallFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "NextAiVPN-Install.msi");

		public static string WebView2FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "MicrosoftEdgeWebview2Setup.exe");

		public static string WebView2UserDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName, "WebView2");
	}

	public static class Links
	{
		public static string WebView2InstallGuideLink = "https://www.nextaitechnology.com/support/knowledgebase/article.aspx/10631/2266/how-to-install-microsoft-edge-webview2-runtime/";

		public static string SpSSubscriptionLink = "https://www.nextaiglobal.com/application/subscriptions/";
	}

	public static class NotifyBalloons
	{
		public const string HidingWindowHeader = "NextAiVPN still running";

		public const string UntrustedNetworkDetectedHeader = "Untrusted network detected\n\"{0}\"";

		public const string UntrustedNetworkDetectedBody = "NextAiVPN detected an Wi-Fi network";

		public const string AddedNetworkToTrustedHeader = "\"{0}\" was successfully added as trusted network";

		public const string NextAiVpnDefaultBody = "Now enjoy private, secure browsing with NextAiVPN";

		public const string HidingWindowMessage = "Note that although you are closing the application, it will still be running and can be accessed through the system menu.";

		public const string ConnectivityIssueHeader = "Connectivity Issue";

		public const string ConnectivityIssueMessage = "Unable to connect your desired location. You are now connected to another city from the same country.";
	}

	public static class Bugsnag
	{
		public const string DevelopmentReleaseStage = "development";

		public const string ProductionReleaseStage = "production";

		public const string AccessGrantedIgnoredMessage = "Unable to parse valid value, access granted for user";

		public const string ApiKeyMissingMessage = "Bugsnag API key is not configured; error reporting is disabled";

		public const string LoginNextAiVPNException = "[LoginToVpn] - {0} - {1}";

		public const string ExecuteInstallerUpdate = "[ExecuteInstaller - Update] - There was an error executing the installer the file NextAiVPN-Install.msi.AppVersion : {0} - Error: {1}";

		public const string SaveFavorites = "[Utils SaveFavorites] - {0}";

		public const string SaveFavoritesMessage = "Unable to save favorite location : {0}";

		public const string BugsnagConfigMessage = "[{0} Config] - {1}";

		public const string UnableToSaveConfigurationMessage = "Unable to save {0} Configuration: {1}";

		public const string SubscriptionError = "Subscription error -  Path: {0}.{1}()\n{2}";

		public const string CommonError = "[Error] - Path: {0}.{1}() - {2}";
	}

	public static class Messages
	{
		public const string IssueReportCommonTextValidationErrorMessage = "Unable to send your feedback. Please check your Internet connection and try again.";

		public const string IssueReportEmptyBodyValidationErrorMessage = "Tell us about your experience";

		public const string IssueReportBodyValidationErrorMessage = "Write at least 20 characters.";

		public const string IssueReportEmptyEmailValidationErrorMessage = "Please enter your email";

		public const string IssueReportEmailValidationErrorMessage = "Please enter a valid email address";

		public const string NoConnectionMessage = "Please, check your internet connection and try to update app again.";

		public const string InstallerRunErrorMessage = "There has been an error launching the update installer, please try again.";
	}

	public static class Colors
	{
		public const string ColorFFD7C2 = "#FFD7C2";

		public const string ColorF4F4F5 = "#F4F4F5";

		public const string Color59595F = "#59595F";

		public const string ColorFFAB33 = "#FFAB33";

		public const string ColorF2F2F6 = "#F2F2F6";

		public const string Color2A2A2C = "#2A2A2C";

		public const string ColorA3A3A3 = "#A3A3A3";

		public const string ColorC0C0C0 = "#C0C0C0";

		public const string ColorEAEAEA = "#EAEAEA";

		public const string Color787879 = "#787879";

		public const string Color05D480 = "#05D480";

		public const string Color8C8C92 = "#8C8C92";

		public const string Color4A4A4A = "#4A4A4A";

		public const string Color074DB2 = "#074DB2";

		public const string ColorE7E7E7 = "#E7E7E7";

		public const string ColorB2F0C1 = "#B2F0C1";

		public const string Color959596 = "#959596";

		public const string Color00B161 = "#00B161";

		public const string Color1D1D20 = "#1D1D20";

		public const string Color00D072 = "#00D072";

		public const string Color0000D072 = "#0000D072";

		public const string ColorF3F3F3 = "#F3F3F3";

		public const string ColorFF864A = "#FF864A";

		public const string ColorFF848487 = "#FF848487";

		public const string ColorBFBFBF = "#BFBFBF";

		public const string Color37383B = "#37383B";

		public const string ColorDCDCDF = "#DCDCDF";

		public const string Red = "#FF0000";

		public const string ColorFFA172 = "#FFA172";

		public const string ColorFFA274 = "#FFA274";

		public const string Color94959E = "#94959E";

		public const string ColorF55B5B = "#F55B5B";

		public const string Color848487 = "#848487";

		public const string Color3A3B40 = "#3A3B40";

		public const string White = "#FFFFFF";

		public const string ColorDCDDDF = "#DCDDDF";

		public const string ColorFAFAFA = "#FAFAFA";

		public const string Black = "#000000";

		public const string Transparent = "#00FFFFFF";

		public const string ColorFFFAFAFA = "#FFFAFAFA";

		public const string ColorF1F1EE = "#F1F1EE";

		public const string ColorD91300 = "#D91300";

		public static string ColorB3B3B4 = "#B3B3B4";

		public static string Color575758 = "#575758";

		public static string ColorFF553B = "#FF553B";

		public static string ColorB64D00 = "#B64D00";

		public static string ColorE66F19 = "#E66F19";
	}

	public static class SystemTrayMenu
	{
		public const string DontAskAgain = "Don’t ask again";

		public const string ConfigInNextAiVpn = "Config in NextAiVPN";

		public const string Notifications = "Notifications";

		public const string QuitHeader = "Quit";

		public const string HelpHeader = "Help";

		public const string CustomerSupportHeader = "Customer Support";

		public const string TermsOfServiceHeader = "Terms of Service";

		public const string PrivacyPolicyHeader = "Privacy Policy";

		public const string FAQHeader = "FAQ";

		public const string SignOutHeader = "Sign Out";

		public const string SettingsHeader = "Settings";

		public const string SendFeedbackHeader = "Send Feedback";

		public const string ShowNextAiVPNHeader = "Show NextAiVPN";

		public const string BestAvailableHeader = "Best available";

		public const string ConnectingToHeader = "Connecting to:";

		public const string ConnectToHeader = "Connect to:";

		public const string ConnectedToHeader = "Connected to:";

		public const string FavoriteHeader = "Favorite";

		public const string StreamingHeader = "For Streaming";

		public const string AddNewFavoriteHeader = "Add new favorite";

		public const string DisconnectHeader = "Disconnect";
	}

	public const int TrialFirstTrafficLimit = 30720;

	public const int RequiredIeVersion = 11001;

	public const int RequiredWebView2EVersion = 109;

	public const string StartUpTaskSchedulerName = "NextAiVPN_Boot";

	public const string StartUpTaskSchedulerDescription = "Runs NextAiVPN on the boot";

	public const string AutoProtectSkipUniq = "nextaivpn-b2c92a50-ee99-41ba-8e1a-7285170d1190";

	public const string IdentifyingNetworkAdapterState = "Identifying...";

	public const string WireGuardTunnelAdapterState = "WireGuard Tunnel";

	public const string NextAiVpnName = "NextAiVpn";

	public const string UrlScheme = "nextaivpn";

	public const int MaxDiagnosticDataLines = 100000;

	public const long InstallerSizeLimit = 11000000L;

	public const string IncludeSubDomainTooltipText = "When unchecked, settings apply only to the domain entered, not its subdomains. Ideal for targeted configurations or enhanced security.";

	public const string TapDeviceDescription = "NextAiVPN Windows Tap Adapter";

	public const string SingleInstancePipeName = "1533535b-481e-45e3-a316-ed95e89f1fa4";

	public const string BestAvailableId = "bestavailable";

	public const string IpTemplate = "--.--.--.--";
}
