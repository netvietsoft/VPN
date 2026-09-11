using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.Streaming;
using NextAiVPN.UI.Account;
using NextAiVPN.UI.AutoProtectNotificationBallons;
using NextAiVPN.UI.Controls.CircleCounter;
using NextAiVPN.UI.Errors.ConnectionError;
using NextAiVPN.UI.Feedback;
using NextAiVPN.UI.GetHelp;
using NextAiVPN.UI.MainPanelConnectionData;
using NextAiVPN.UI.MainPanelPopUps.IpAddressRotationDisclaimer;
using NextAiVPN.UI.MessageBoxWindows.AutoRenewal;
using NextAiVPN.UI.MessageBoxWindows.SignOutMessageWindow;
using NextAiVPN.UI.Preferences;
using NextAiVPN.UI.Protocols;
using NextAiVPN.UI.Settings;
using NextAiVPN.UI.SplitTunneling;
using NextAiVPN.UI.Streaming.DeviceLimit;
using NextAiVPN.UI.Streaming.FavoriteLocations;
using NextAiVPN.UI.Streaming.InfoWindow;
using NextAiVPN.UI.Streaming.Locations;
using NextAiVPN.UI.Streaming.NoLocation;
using NextAiVPN.UI.TermsAndPolicies;
using NextAiVPN.UI.ThemeAppearance;
using NextAiVPN.UI.ToolsContextMenu;
using NextAiVPN.UI.TrafficOptimizer;
using NextAiVPN.UI.Trial;
using NextAiVPN.UI.TrustedNetwork;
using NextAiVPN.UI.Update;

namespace NextAiVPN;

internal class NextAiVpnDependencyFactory
{
	private readonly MainWindow _mainWindow;

	public NextAiVpnDependencyFactory(MainWindow mainWindow)
	{
		_mainWindow = mainWindow;
	}

	public VpnEntitiesServices CreateVpnEntities()
	{
		IBugsnagService bugsnagService = (Utils.BugsnagService = BugsnagServiceFactory.Create(Utils.AppSettingsHelper, Utils.Logger));
		PreferencesRepository preferencesRepository = (PreferencesRepository)(Utils.PreferencesRepository = new PreferencesRepository(Utils.AppSettingsHelper, Utils.Logger, new DpapiSecretProtector()));
		MessageBoxNotificationPresenter notificationPresenter = new MessageBoxNotificationPresenter();
		TokenRefreshHandler tokenRefreshHandler = new TokenRefreshHandler(Utils.AppSettingsHelper, preferencesRepository, bugsnagService, Utils.Logger);
		VpnConfigurationSaver vpnConfigurationSaver = (VpnConfigurationSaver)(Utils.VpnConfigurationSaver = new VpnConfigurationSaver(Utils.AppSettingsHelper, preferencesRepository, bugsnagService, notificationPresenter));
		Utils.FavoritesService = new FavoritesService(Utils.AppSettingsHelper, preferencesRepository, bugsnagService, Utils.Logger, notificationPresenter);
		AppSettingsCredentialStore credentialStore = new AppSettingsCredentialStore(Utils.AppSettingsHelper, Utils.Logger);
		AccessTokenGenerator accessTokenGenerator = new AccessTokenGenerator(new OAuth2NextAiGlobalParameters(), Utils.AppSettingsHelper, Utils.Logger);
		AccountTypeHelper accountTypeHelper = new AccountTypeHelper(Utils.AppSettingsHelper);
		OSVersionService oSVersionService = new OSVersionService(bugsnagService);
		MixpanelNotification mixpanelNotification = (MixpanelNotification)(Utils.MixpanelNotification = new MixpanelNotification(oSVersionService, Utils.AppSettingsHelper, Utils.Logger));
		mixpanelNotification.RefreshData();
		APICommon apiClient = (APICommon)(Utils.Api = new APICommon(accessTokenGenerator, bugsnagService, preferencesRepository, Utils.AppSettingsHelper, Utils.Logger, mixpanelNotification, tokenRefreshHandler));
		SubscriptionInfo subscriptionInfo = new SubscriptionInfo(accountTypeHelper, bugsnagService, apiClient, Utils.AppSettingsHelper, Utils.Logger);
		FeedbackService feedbackService = (FeedbackService)(Utils.FeedbackService = new FeedbackService(accountTypeHelper, Utils.AppSettingsHelper, preferencesRepository, apiClient, Utils.Logger));
		IpHelper ipHelper = new IpHelper();
		VpnEntitiesServices vpnEntitiesServices = new VpnEntitiesServices
		{
			MainWindow = _mainWindow,
			ConfigurationLoggerHelper = new ConfigurationLoggerHelper(oSVersionService, credentialStore, Utils.AppSettingsHelper),
			IpHelper = ipHelper,
			BugsnagService = bugsnagService,
			PreferencesRepository = preferencesRepository,
			ApiClient = apiClient,
			AnalyticsService = mixpanelNotification,
			AppSettingsHelper = Utils.AppSettingsHelper,
			Logger = Utils.Logger,
			NotificationPresenter = notificationPresenter
		};
		StreamingSplitTunnelingRepository splitTunnelingRepository = new StreamingSplitTunnelingRepository(Utils.AppSettingsHelper, Utils.LocalSettingsRepository);
		SdkManager streamingSdk = new SdkManager(new VpnConnectorRas(new SplitTunnelRoutingService(splitTunnelingRepository, Utils.AppSettingsHelper)), new StreamingVpnService(), new CertificatesHelper());
		vpnEntitiesServices.StreamingSdk = streamingSdk;
		VpnTypeDefiner vpnTypeDefiner = new VpnTypeDefiner(Utils.AppSettingsHelper);
		vpnEntitiesServices.VpnModesService = new VpnModesService(vpnTypeDefiner, Utils.AppSettingsHelper);
		BrowserLinksOpener browserLinksOpener = new BrowserLinksOpener(accountTypeHelper, bugsnagService, Utils.Logger, mixpanelNotification);
		SDKMonitor sdk = new SDKMonitor(vpnEntitiesServices)
		{
			BrowserLinksOpener = browserLinksOpener,
			AccountTypeHelper = accountTypeHelper,
			NextAiGlobalTokenValidator = new NextAiGlobalTokenValidator(accessTokenGenerator, credentialStore, Utils.Logger, Utils.AppSettingsHelper, apiClient),
			CredentialStore = credentialStore
		};
		StyleService styleService = (StyleService)(vpnEntitiesServices.StyleService = new StyleService(sdk));
		vpnEntitiesServices.ColorThemeDetector = new ColorThemeDetector();
		new SingleInstanceGuard(sdk, new SingleInstanceDetector()).EnsureSingleInstance();
		FavoriteLocationsServices favoriteLocationsServices = new FavoriteLocationsServices(sdk, Utils.AppSettingsHelper);
		vpnEntitiesServices.SplitTunnelingManager = new SplitTunnelingManager(new SplitTunnelingRepository(Utils.AppSettingsHelper, Utils.Logger, Utils.LocalSettingsRepository), sdk, Utils.AppSettingsHelper, Utils.Logger);
		vpnEntitiesServices.TrafficOptimizerManager = new TrafficOptimizerManager(sdk, Utils.AppSettingsHelper);
		sdk.TaskBarService = new TaskBarIconService(sdk, new CountryFlagService(), favoriteLocationsServices, styleService, new SignOutMessageBoxWindow(new SignOutMessageBoxWindowViewModel()), bugsnagService, apiClient, mixpanelNotification, Utils.Logger);
		sdk.LocationHelper = new LocationHelper(sdk.NextAiVpnSdkManager);
		sdk.TapDriverInstaller = new TapDriverInstaller(mixpanelNotification, bugsnagService, () => sdk.NextAiVpnSdkManager);
		sdk.SessionStatsTracker = new SessionStatsTracker(Utils.AppSettingsHelper, Utils.Logger, () => sdk.NextAiVpnSdkManager, () => sdk.StreamingSdk);
		sdk.AuthenticationFlow = new AuthenticationFlow(subscriptionInfo, credentialStore, Utils.AppSettingsHelper, Utils.Logger, bugsnagService, () => sdk.NextAiVpnSdkManager, () => sdk.StreamingSdk, OSDetector.GetOSVersion, InternetConnection.IsAvailableAsync);
		sdk.SubscriptionFlowCoordinator = new SubscriptionFlowCoordinator(subscriptionInfo, Utils.AppSettingsHelper, Utils.Logger);
		sdk.ConnectionOrchestrator = new VpnConnectionOrchestrator(new IVpnConnectionStrategy[2]
		{
			new NextAiVpnConnectionStrategy(sdk),
			new StreamingConnectionStrategy(sdk)
		}, Utils.AppSettingsHelper, Utils.Logger, () => sdk.NextAiVpnSdkManager.IsConnected || sdk.StreamingSdk.IsConnected || sdk.IsStreamingConnected || sdk.NextAiVpnSdkManager.IsConnecting);
		styleService.GetSdkObject(sdk);
		MessageWindow messageWindow = (vpnEntitiesServices.MessageWindow = new MessageWindow(sdk)
		{
			Height = 372.0,
			NewUser = 
			{
				Visibility = Visibility.Visible
			}
		});
		MainWindow mainWindow = _mainWindow;
		SignInWindow signInWindow = (vpnEntitiesServices.SignInWindow = new SignInWindow());
		mainWindow.SignInWindow = signInWindow;
		vpnEntitiesServices.WebBrowserDefiner = new WebBrowserDefiner(new WebView2Installer(apiClient, Utils.Logger), new BrowserVersionHelper(bugsnagService, Utils.Logger), Utils.Logger, browserLinksOpener);
		SignInService signInService = new SignInService(sdk, vpnEntitiesServices.WebBrowserDefiner, bugsnagService, apiClient, mixpanelNotification, Utils.AppSettingsHelper, Utils.Logger);
		NextAiTechnologySubscriptionProlongationHandler nextaitechnologySubscriptionProlongationHandler = new NextAiTechnologySubscriptionProlongationHandler(subscriptionInfo, Utils.Logger, browserLinksOpener);
		TrialFirstWindowViewModel trialFirstWindowViewModel = new TrialFirstWindowViewModel(subscriptionInfo, sdk, accountTypeHelper, nextaitechnologySubscriptionProlongationHandler, Utils.Logger, browserLinksOpener);
		TrialFirstWindow window = new TrialFirstWindow(trialFirstWindowViewModel);
		trialFirstWindowViewModel.SetWindow(window);
		NextAiGlobalSignInService signInNextAiGlobalService = new NextAiGlobalSignInService(sdk, accessTokenGenerator, _mainWindow, bugsnagService, apiClient, mixpanelNotification, Utils.Logger, Utils.AppSettingsHelper);
		vpnEntitiesServices.TrialFirstViewModel = trialFirstWindowViewModel;
		vpnEntitiesServices.SubscriptionInfo = subscriptionInfo;
		MainWindow mainWindow2 = _mainWindow;
		SignUpWindow signUpWindow = (vpnEntitiesServices.SignUpWindow = new SignUpWindow(sdk, vpnEntitiesServices.WebBrowserDefiner));
		mainWindow2.SignUpWindow = signUpWindow;
		ShowFeedbackWindowService feedbackWindowService = new ShowFeedbackWindowService(Utils.AppSettingsHelper);
		UpdateService updateService = new UpdateService(new CredentialsSaver(Utils.AppSettingsHelper, preferencesRepository), new MsiFileCleaner(Utils.Logger), bugsnagService, Utils.Logger, apiClient);
		VPNWindowExpanded vPNWindowExpanded = (vpnEntitiesServices.ExpandedWindow = new VPNWindowExpanded(sdk, feedbackWindowService, bugsnagService, Utils.Logger, Utils.AppSettingsHelper, new UninstallRegistryVersionUpdater(Utils.Logger))
		{
			WindowStyle = WindowStyle.SingleBorderWindow,
			SubscriptionInfo = subscriptionInfo,
			TrustedNetworkService = new TrustedNetworkService(new TrustedNetworkRepository(new LegacyBinaryFallbackSerializer(new JsonFileSerializer<IEnumerable<string>>(Utils.Logger), Utils.Logger))),
			UpdateService = updateService,
			TermsAndPoliciesViewModel = new ExpandedTermsAndPoliciesViewModel(browserLinksOpener),
			NetworkService = new NetworkService(bugsnagService),
			NotificationCenter = 
			{
				NotificationService = new NotificationService(Utils.AppSettingsHelper, sdk, bugsnagService, Utils.Logger, apiClient),
				BugsnagService = bugsnagService
			}
		});
		sdk.VpnExpandedWindow = vPNWindowExpanded;
		RoleChecker roleChecker = new RoleChecker();
		vPNWindowExpanded.AdminPermissionsRunnerService = new AdminPermissionsRunnerService(new AdminRunner(Utils.AppSettingsHelper), roleChecker, vPNWindowExpanded, Utils.AppSettingsHelper);
		ExpandedAccountViewModel expandedAccountViewModel = new ExpandedAccountViewModel(vPNWindowExpanded, new AutoRenewalMessageBoxWindow(new AutoRenewalMessageBoxWindowViewModel()), subscriptionInfo, nextaitechnologySubscriptionProlongationHandler, bugsnagService, mixpanelNotification, Utils.Logger, browserLinksOpener);
		ExpandedSideMenu expandedSideMenu = vPNWindowExpanded.ExpandedSideMenu;
		expandedSideMenu.OnTabChanged = (Action<SideMenuOption>)Delegate.Combine(expandedSideMenu.OnTabChanged, new Action<SideMenuOption>(expandedAccountViewModel.OnTabChanged));
		vPNWindowExpanded.AccountViewModel = expandedAccountViewModel;
		vPNWindowExpanded.GetHelpViewModel = new ExpandedGetHelpViewModel(vPNWindowExpanded);
		vPNWindowExpanded.PreferencesService = new PreferencesService(vPNWindowExpanded, bugsnagService, Utils.AppSettingsHelper, Utils.Logger);
		EmailValidator emailValidator = new EmailValidator();
		vPNWindowExpanded.FeedbackWindow = new FeedbackWindow(Utils.AppSettingsHelper, new FeedbackWindowViewModel(new FeedbackControlViewModel(vPNWindowExpanded, emailValidator, feedbackService, Utils.AppSettingsHelper, Utils.Logger)))
		{
			ToolTip = true
		};
		StartUpService startUpService = new StartUpService(bugsnagService, Utils.Logger);
		TrustedNetworksWindowViewModel trustedNetworksWindowViewModel = new TrustedNetworksWindowViewModel(vPNWindowExpanded.TrustedNetworkService, vPNWindowExpanded.NetworkService, vPNWindowExpanded, bugsnagService, Utils.Logger);
		AutoProtectNotificationBalloonViewModel dependencies = new AutoProtectNotificationBalloonViewModel(vPNWindowExpanded, apiClient, Utils.AppSettingsHelper)
		{
			Header = string.Format(CultureInfo.InvariantCulture, "Untrusted network detected\n\"{0}\"", string.Empty),
			Message = "NextAiVPN detected an Wi-Fi network",
			ImageSource = IconHelper.GetIcon("NewUserControlIcon")
		};
		ExpandedProtocolsControlViewModel dataContext = new ExpandedProtocolsControlViewModel();
		ExpandedGeneralSettingsControlViewModel dataContext2 = new ExpandedGeneralSettingsControlViewModel();
		UpdateControlViewModel updateControlViewModel = new UpdateControlViewModel(updateService, bugsnagService, apiClient, Utils.Logger);
		SettingsMainControlViewModel settingsMainControlViewModel = new SettingsMainControlViewModel(updateControlViewModel);
		DisconnectMessageBoxHelper disconnectMessageBoxHelper = new DisconnectMessageBoxHelper(sdk);
		settingsMainControlViewModel.CheckUpdates();
		vPNWindowExpanded.SettingsControlViewModel = settingsMainControlViewModel;
		vPNWindowExpanded.GeneralControl.DataContext = settingsMainControlViewModel;
		vPNWindowExpanded.GeneralControl.GeneralSettingsControl.DataContext = dataContext2;
		vPNWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.DataContext = dataContext;
		vPNWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.ProtocolsControl.SetDependencies(sdk, disconnectMessageBoxHelper, roleChecker);
		vPNWindowExpanded.GeneralControl.GeneralSettingsControl.UpdateControl.DataContext = updateControlViewModel;
		vPNWindowExpanded.GeneralControl.GeneralSettingsControl.UpdateControl.UpdateVersionButton.Click += updateControlViewModel.OnUpdateButtonClick;
		vPNWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.GetDependencies(sdk, new TrafficOptimizerWindow(new TrafficOptimizerWindowViewModel(Utils.AppSettingsHelper, Utils.Logger)), disconnectMessageBoxHelper);
		vPNWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.GetStartUpService(startUpService);
		vPNWindowExpanded.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.GetTrustedNetworksWindowViewModel(trustedNetworksWindowViewModel);
		ExpandedStreamingSettingsControlViewModel dataContext3 = new ExpandedStreamingSettingsControlViewModel(new StreamingSplitTunnelingDomainsWindow(new StreamingSplitTunnelingDomainsWindowViewModel(splitTunnelingRepository, Utils.Logger, browserLinksOpener)), disconnectMessageBoxHelper, splitTunnelingRepository, vPNWindowExpanded.GeneralControl.StreamingSettingsControl.SplitTunnelingSwitch, Utils.AppSettingsHelper);
		vPNWindowExpanded.GeneralControl.StreamingSettingsControl.DataContext = dataContext3;
		ThemeAppearanceControlViewModel dataContext4 = new ThemeAppearanceControlViewModel(sdk);
		PreferencesControlViewModel preferencesControlViewModel = (PreferencesControlViewModel)(vPNWindowExpanded.DataContext = new PreferencesControlViewModel(startUpService, vpnConfigurationSaver, mixpanelNotification));
		vPNWindowExpanded.Preferences.StartUpToggle.MouseLeftButtonUp += preferencesControlViewModel.StartUpToggle_OnMouseLeftButtonUp;
		vPNWindowExpanded.ExpandedLocations.FavoriteLocationsTabControl.FavoriteLocationsServices = favoriteLocationsServices;
		vPNWindowExpanded.ExpandedLocations.FavoriteLocationsTabControl.GetSdk(sdk);
		vPNWindowExpanded.ExpandedLocations.AllLocationsControl.GetSdk(sdk);
		StreamingLocationsViewModel streamingLocationsViewModel = new StreamingLocationsViewModel(sdk);
		vPNWindowExpanded.ExpandedLocations.StreamingLocations.DataContext = streamingLocationsViewModel;
		vPNWindowExpanded.ExpandedLocations.StreamingLocations.StreamingLocationsViewModel = streamingLocationsViewModel;
		NoStreamingLocationsViewModel noStreamingLocationsViewModel = new NoStreamingLocationsViewModel(sdk, Utils.Logger);
		vPNWindowExpanded.ExpandedLocations.StreamingLocations.NoStreamingLocationsControl.DataContext = noStreamingLocationsViewModel;
		vPNWindowExpanded.ExpandedLocations.StreamingLocations.NoStreamingLocationsControl.TryAgainButton.Click += noStreamingLocationsViewModel.OnUpdateButtonClick;
		vPNWindowExpanded.ExpandedLocations.StreamingInformationWindow = new StreamingInfoWindow(new StreamingInfoWindowViewModel(sdk));
		FavoriteStreamingLocationsControlViewModel favoriteStreamingLocationsControlViewModel = new FavoriteStreamingLocationsControlViewModel(sdk);
		vPNWindowExpanded.Mainpanel.StreamingFavoriteLocationControl.ViewModel = favoriteStreamingLocationsControlViewModel;
		vPNWindowExpanded.Mainpanel.StreamingFavoriteLocationControl.DataContext = favoriteStreamingLocationsControlViewModel;
		ConnectionErrorControlViewModel connectionErrorControlViewModel = new ConnectionErrorControlViewModel(sdk);
		vPNWindowExpanded.Mainpanel.ConnectionError.DataContext = connectionErrorControlViewModel;
		vPNWindowExpanded.Mainpanel.ConnectionError.ViewModel = connectionErrorControlViewModel;
		IpAddressRotationDisclaimerControlViewModel ipAddressRotationDisclaimerControlViewModel = new IpAddressRotationDisclaimerControlViewModel();
		vPNWindowExpanded.Mainpanel.IpAddressRotationDisclaimer.DataContext = ipAddressRotationDisclaimerControlViewModel;
		vPNWindowExpanded.Mainpanel.IpAddressRotationDisclaimer.ViewModel = ipAddressRotationDisclaimerControlViewModel;
		TrialFirstChipsViewModel dataContext5 = new TrialFirstChipsViewModel(vpnTypeDefiner, accountTypeHelper, Utils.Logger);
		vPNWindowExpanded.Mainpanel.TrialFirstChips.DataContext = dataContext5;
		ConnectionDataViewModel connectionDataViewModel = new ConnectionDataViewModel(sdk);
		vPNWindowExpanded.Mainpanel.ConnectionDataViewModel = connectionDataViewModel;
		vPNWindowExpanded.Mainpanel.ConnectionData.DataContext = connectionDataViewModel;
		vPNWindowExpanded.SetDependencies(dependencies);
		sdk.SetLastConnectedFlag();
		vPNWindowExpanded.NotificationCenter.GetExpandedVpnWindow(vPNWindowExpanded, browserLinksOpener);
		vPNWindowExpanded.BeforeConnectWindowWrapper = new BeforeConnectWindowWrapper(vPNWindowExpanded);
		CircleCounterControlViewModel appCounterControlViewModel = new CircleCounterControlViewModel();
		CircleCounterControlViewModel domainCounterControlViewModel = new CircleCounterControlViewModel();
		SplitTunnelingRepository splitTunnelingRepository2 = new SplitTunnelingRepository(Utils.AppSettingsHelper, Utils.Logger, Utils.LocalSettingsRepository);
		SplitTunnelingAppService splitTunnelingAppService = new SplitTunnelingAppService(splitTunnelingRepository2);
		SplitTunnelingAppWindowViewModel splitTunnelingAppWindowViewModel = new SplitTunnelingAppWindowViewModel(splitTunnelingAppService);
		SplitTunnelingAppWindow appWindow = new SplitTunnelingAppWindow(splitTunnelingAppWindowViewModel);
		SplitTunnelingDomainsService splitTunnelingDomainsService = new SplitTunnelingDomainsService(splitTunnelingRepository2);
		SplitTunnelingDomainWindowViewModel splitTunnelingDomainWindowViewModel = new SplitTunnelingDomainWindowViewModel(splitTunnelingDomainsService, Utils.Logger, browserLinksOpener);
		SplitTunnelingDomainWindow domainWindow = new SplitTunnelingDomainWindow(splitTunnelingDomainWindowViewModel);
		SplitTunnelingMainControlViewModel splitTunnelingMainControlViewModel = (splitTunnelingAppWindowViewModel.SplitTunnelingMainControlViewModel = (splitTunnelingDomainWindowViewModel.SplitTunnelingMainControlViewModel = new SplitTunnelingMainControlViewModel(splitTunnelingAppService, splitTunnelingDomainsService, appWindow, domainWindow, appCounterControlViewModel, domainCounterControlViewModel)));
		SDKMonitor sDKMonitor = sdk;
		sDKMonitor.OnDisconnectAction = (Action)Delegate.Combine(sDKMonitor.OnDisconnectAction, new Action(splitTunnelingMainControlViewModel.OnDisconnect));
		vPNWindowExpanded.SplitTunnelingMainWindow = new SplitTunnelingMainWindow(new SplitTunnelingMainWindowViewModel(splitTunnelingMainControlViewModel));
		ToolsControlViewModel toolsControlViewModel = new ToolsControlViewModel(sdk);
		toolsControlViewModel.SetVisibility(isMainScreen: true);
		vpnEntitiesServices.MainWindowToolsControlViewModel = toolsControlViewModel;
		vPNWindowExpanded.BindViewModels();
		vpnEntitiesServices.VpnModesValidator = new VpnModesValidator(sdk, Utils.AppSettingsHelper, Utils.Logger, apiClient);
		SubscriptionExpiredWindow subscriptionExpiredWindow = new SubscriptionExpiredWindow(sdk);
		_mainWindow.SubscriptionExpiredWindow = subscriptionExpiredWindow;
		subscriptionExpiredWindow.CompletePurchase.SetInstances(browserLinksOpener);
		subscriptionExpiredWindow.MainSubscription.SetInstances(browserLinksOpener);
		DeviceLimitWindowViewModel deviceLimitWindowViewModel = new DeviceLimitWindowViewModel(browserLinksOpener);
		vpnEntitiesServices.DeviceLimitWindowViewModel = deviceLimitWindowViewModel;
		vpnEntitiesServices.SubscriptionStatusHelper = new SubscriptionStatusHelper(accountTypeHelper, bugsnagService, Utils.AppSettingsHelper, Utils.Logger, apiClient, tokenRefreshHandler);
		messageWindow.InitWindow();
		vpnEntitiesServices.Sdk = sdk;
		vpnEntitiesServices.SignInService = signInService;
		vpnEntitiesServices.SignInNextAiGlobalService = signInNextAiGlobalService;
		return vpnEntitiesServices;
	}
}
