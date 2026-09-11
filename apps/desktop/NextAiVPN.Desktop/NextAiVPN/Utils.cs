using NextAiVPN.Common;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN;

internal static class Utils
{
	public static IAppLogger Logger;

	private static IAppSettingsHelper _appSettingsHelper;

	private static IPreferencesRepository _preferencesRepository;

	private static IVpnConfigurationSaver _vpnConfigurationSaver;

	private static IFavoritesService _favoritesService;

	private static IFeedbackService _feedbackService;

	public static IStorageRepository LocalSettingsRepository = new LocalStorageRepository(AppSettingsHelper, Logger);

	public static IApiClient Api;

	public static IAppSettingsHelper AppSettingsHelper
	{
		get
		{
			return _appSettingsHelper ?? (_appSettingsHelper = new AppSettingsXmlFileHelper(Logger));
		}
		set
		{
			_appSettingsHelper = value;
		}
	}

	public static IPreferencesRepository PreferencesRepository
	{
		get
		{
			return _preferencesRepository ?? new PreferencesRepository(AppSettingsHelper, Logger, new DpapiSecretProtector());
		}
		set
		{
			_preferencesRepository = value;
		}
	}

	public static IVpnConfigurationSaver VpnConfigurationSaver
	{
		get
		{
			return _vpnConfigurationSaver ?? new VpnConfigurationSaver(AppSettingsHelper, PreferencesRepository, BugsnagService, new MessageBoxNotificationPresenter());
		}
		set
		{
			_vpnConfigurationSaver = value;
		}
	}

	public static IFavoritesService FavoritesService
	{
		get
		{
			return _favoritesService ?? new FavoritesService(AppSettingsHelper, PreferencesRepository, BugsnagService, Logger, new MessageBoxNotificationPresenter());
		}
		set
		{
			_favoritesService = value;
		}
	}

	public static IFeedbackService FeedbackService
	{
		get
		{
			return _feedbackService ?? new FeedbackService(new AccountTypeHelper(AppSettingsHelper), AppSettingsHelper, PreferencesRepository, Api, Logger);
		}
		set
		{
			_feedbackService = value;
		}
	}

	public static IBugsnagService BugsnagService { get; set; } = new NullBugsnagService();

	public static IAnalyticsService MixpanelNotification { get; set; }
}
