using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;
using NextAiVPN.Streaming;
using NextAiVPN.UI.NextAiGlobal.SignIn;
using NextAiVPN.UI.Streaming.DeviceLimit;
using NextAiVPN.UI.ToolsContextMenu;
using NextAiVPN.UI.Trial;

namespace NextAiVPN.Services;

public class VpnEntitiesServices
{
	public MainWindow MainWindow { get; set; }

	public VPNWindowExpanded ExpandedWindow { get; set; }

	public SDKMonitor Sdk { get; set; }

	public SignInWindow SignInWindow { get; set; }

	public NextAiGlobalSignInWindow NextAiGlobalSignInWindow { get; set; }

	public SignUpWindow SignUpWindow { get; set; }

	public MessageWindow MessageWindow { get; set; }

	public ISignInService SignInService { get; set; }

	public IWebBrowserDefiner WebBrowserDefiner { get; set; }

	public ISplitTunnelingManager SplitTunnelingManager { get; set; }

	public SdkManager StreamingSdk { get; set; }

	public IVpnModesService VpnModesService { get; set; }

	public DeviceLimitWindowViewModel DeviceLimitWindowViewModel { get; set; }

	public ISubscriptionStatusHelper SubscriptionStatusHelper { get; set; }

	public IVpnModeValidator VpnModesValidator { get; set; }

	public ISignInService SignInNextAiGlobalService { get; set; }

	public ITrafficOptimizerManager TrafficOptimizerManager { get; set; }

	public IConfigurationLoggerHelper ConfigurationLoggerHelper { get; set; }

	public ToolsControlViewModel MainWindowToolsControlViewModel { get; set; }

	public IStyleService StyleService { get; set; }

	public IColorThemeDetector ColorThemeDetector { get; set; }

	public TrialFirstWindowViewModel TrialFirstViewModel { get; set; }

	public ISubscriptionInfo SubscriptionInfo { get; set; }

	public IIpHelper IpHelper { get; set; }

	public IBugsnagService BugsnagService { get; set; } = new NullBugsnagService();

	public IPreferencesRepository PreferencesRepository { get; set; }

	public IApiClient ApiClient { get; set; }

	public IAnalyticsService AnalyticsService { get; set; }

	public IAppSettingsHelper AppSettingsHelper { get; set; }

	public IAppLogger Logger { get; set; }

	public INotificationPresenter NotificationPresenter { get; set; }
}
