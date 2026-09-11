using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DynamicData.Binding;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Enums;
using NextAiVPN.Enums.Streaming;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;
using NextAiVPN.Streaming;
using NextAiVPN.Streaming.Entities;
using NextAiVPN.Streaming.Exceptions;
using NextAiVPN.UI;
using NextAiVPN.UI.MainPanelConnectionData;
using NextAiVPN.UI.NotifyBalloons;
using NextAiVPN.UI.Settings;
using NextAiVPN.UI.Streaming.Balloons;
using NextAiVPN.UI.Streaming.DeviceLimit;
using NextAiVPN.UI.Trial;
using VpnSDK;
using VpnSDK.DTO;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace NextAiVPN;

public class SDKMonitor : ViewModelBase
{
	public delegate void ConnectionCompleteHandler(string message);

	private readonly int _noNetworkTimeOut;

	private readonly NotifyBalloonError _errorBalloon = new NotifyBalloonError();

	private readonly NotifyBalloonNoNetwork _noNetworkBalloon = new NotifyBalloonNoNetwork();

	private readonly NotifyBalloonConnected _connectedBalloon = new NotifyBalloonConnected();

	private readonly IVpnModesService _vpnModesService;

	private readonly IBugsnagService _bugsnagService;

	private readonly IPreferencesRepository _preferencesRepository;

	private readonly IApiClient _apiClient;

	private readonly IAnalyticsService _analyticsService;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly INotificationPresenter _notificationPresenter;

	private readonly IList<VpnSDK.Interfaces.IServer> _locationUsedServers = new List<VpnSDK.Interfaces.IServer>();

	private string _country;

	private string _city;

	private string _statusButton;

	private string _blinkingConnection;

	private bool _buttonEnable;

	private ILocation _nextaivpnLocation;

	private Visibility _spinner;

	private Visibility _spinnerProtocols;

	private Visibility _btnLocationsVisible;

	private Visibility _txtLoadingLocations;

	private bool _bugsnagNotified;

	private Visibility _connectedStatus;

	private Visibility _reconnectingStatus;

	private string _selectedProtocol;

	private string _ipAddress;

	private int _reconnectCounter;

	private Visibility _protocolPanelVisible;

	private BitmapImage _lastConnectedFlag;

	private NoNetworkSimple _noNetworkNextAiVpn;

	private string _protocolsConnectStatus;

	private Visibility _connectionInfoPanel;

	private CancellationTokenSource _connectionTokenSource;

	private IConnectionConfiguration _connectionConfiguration;

	public bool AllCitiesPing;

	public bool LocationsRefreshing;

	public bool OpenVpnScramble;

	public NetworkConnectionType ConnectionType;

	public NetworkProtocolType SelectedOpenVpnProtocol;

	public VPNWindowExpanded VpnExpandedWindow;

	public bool IsReconnecting;

	public bool ManualDisconnect;

	public bool ConnectFromFavorites;

	public bool IsError;

	public ILocationHelper LocationHelper;

	public Action OnDisconnectAction;

	public IAccountTypeHelper AccountTypeHelper;

	public INextAiGlobalTokenValidator NextAiGlobalTokenValidator;

	public ICredentialStore CredentialStore;

	public readonly SdkManager StreamingSdk;

	private readonly NotifyBalloonStreamingConnected _connectedStreamingBalloon = new NotifyBalloonStreamingConnected();

	public ObservableCollection<IStreamingLocation> StreamingLocations = new ObservableCollection<IStreamingLocation>();

	public VpnType ConnectedVpnMode;

	private bool _isSteamingModeAvailable = true;

	private IStreamingLocation _streamingLocation;

	public SubscriptionExpiredWindow SubscriptionExpiredWindow;

	private ISubscriptionFlowCoordinator _subscriptionFlowCoordinator;

	private ISessionStatsTracker _sessionStatsTracker;

	public IStreamingLocation StreamingLocation
	{
		get
		{
			return _streamingLocation;
		}
		set
		{
			if (value != _streamingLocation)
			{
				_streamingLocation = value;
				VpnExpandedWindow?.Mainpanel.StreamingFavoriteLocationControl.ViewModel.UpdateLocations();
			}
		}
	}

	public bool IsNextAiVpnModeAvailable { get; private set; }

	public bool IsStreamingModeAvailable { get; set; }

	public string NextAiVpnModeErrorMessage { get; private set; }

	public TaskBarIconService TaskBarService { get; set; }

	public IBrowserLinksOpener BrowserLinksOpener { get; set; }

	public ITapDriverInstaller TapDriverInstaller { get; set; }

	public IAuthenticationFlow AuthenticationFlow { get; set; }

	public ISubscriptionFlowCoordinator SubscriptionFlowCoordinator
	{
		get
		{
			return _subscriptionFlowCoordinator;
		}
		set
		{
			_subscriptionFlowCoordinator = value;
			if (value != null)
			{
				value.TrialFirstWindowRequested += ShowTrialFirstWindow;
				value.TrialChipsUpdateRequested += UpdateTrialFirstChips;
			}
		}
	}

	public IVpnConnectionOrchestrator ConnectionOrchestrator { get; set; }

	public ISessionStatsTracker SessionStatsTracker
	{
		get
		{
			return _sessionStatsTracker;
		}
		set
		{
			_sessionStatsTracker = value;
			if (value != null)
			{
				value.Ticked += OnSessionStatsTicked;
				value.UsageUpdated += OnSessionUsageUpdated;
			}
		}
	}

	public ConnectionState State { get; private set; }

	public string ConnectionStatus { get; private set; } = "disconnected";

	public ISDK NextAiVpnSdkManager { get; set; }

	public long NetworkUsageDownload => SessionStatsTracker?.DownloadedBytes ?? 0;

	public string networkUsageDownloadMB => SessionStatsTracker?.DownloadedUsageText ?? "0 MB";

	public long networkUsageUpload => SessionStatsTracker?.UploadedBytes ?? 0;

	public string networkUsageUploadMB => SessionStatsTracker?.UploadedUsageText ?? "0 MB";

	public ILocation NextAiVpnLocation
	{
		get
		{
			return _nextaivpnLocation;
		}
		private set
		{
			if (value != _nextaivpnLocation)
			{
				_nextaivpnLocation = value;
				OnPropertyChanged("NextAiVpnLocation");
			}
		}
	}

	public string Country
	{
		get
		{
			return _country;
		}
		private set
		{
			_country = value;
			OnPropertyChanged("Country");
		}
	}

	public string City
	{
		get
		{
			return _city;
		}
		private set
		{
			_city = value;
			OnPropertyChanged("City");
		}
	}

	public BitmapImage LastConnectedFlag
	{
		get
		{
			return _lastConnectedFlag;
		}
		private set
		{
			_lastConnectedFlag = value;
			OnPropertyChanged("LastConnectedFlag");
		}
	}

	public string StatusButton
	{
		get
		{
			return _statusButton;
		}
		internal set
		{
			_statusButton = value;
			OnPropertyChanged("StatusButton");
		}
	}

	public string ProtocolsConnectStatus
	{
		get
		{
			return _protocolsConnectStatus;
		}
		set
		{
			_protocolsConnectStatus = value;
			OnPropertyChanged("ProtocolsConnectStatus");
		}
	}

	public string blinkingConnection
	{
		get
		{
			return _blinkingConnection;
		}
		private set
		{
			_blinkingConnection = value;
			OnPropertyChanged("blinkingConnection");
		}
	}

	public bool ButtonEnable
	{
		get
		{
			return _buttonEnable;
		}
		private set
		{
			_buttonEnable = value;
			OnPropertyChanged("ButtonEnable");
		}
	}

	public Visibility ProtocolPanelVisible
	{
		get
		{
			return _protocolPanelVisible;
		}
		private set
		{
			_protocolPanelVisible = value;
			OnPropertyChanged("ProtocolPanelVisible");
		}
	}

	public Visibility btnLocationsVisible
	{
		get
		{
			return _btnLocationsVisible;
		}
		private set
		{
			_btnLocationsVisible = value;
			OnPropertyChanged("btnLocationsVisible");
		}
	}

	public Visibility ConnectedStatus
	{
		get
		{
			return _connectedStatus;
		}
		private set
		{
			_connectedStatus = value;
			OnPropertyChanged("ConnectedStatus");
		}
	}

	public Visibility ReconnectingStatus
	{
		get
		{
			return _reconnectingStatus;
		}
		private set
		{
			_reconnectingStatus = value;
			OnPropertyChanged("ReconnectingStatus");
		}
	}

	public Visibility txtLocationsLoading
	{
		get
		{
			return _txtLoadingLocations;
		}
		private set
		{
			_txtLoadingLocations = value;
			OnPropertyChanged("txtLocationsLoading");
		}
	}

	public Visibility Spinner
	{
		get
		{
			return _spinner;
		}
		private set
		{
			_spinner = value;
			OnPropertyChanged("Spinner");
		}
	}

	public Visibility SpinnerProtocols
	{
		get
		{
			return _spinnerProtocols;
		}
		private set
		{
			_spinnerProtocols = value;
			OnPropertyChanged("SpinnerProtocols");
		}
	}

	public Visibility ConnectionInfoPanel
	{
		get
		{
			return _connectionInfoPanel;
		}
		private set
		{
			_connectionInfoPanel = value;
			OnPropertyChanged("ConnectionInfoPanel");
		}
	}

	public string IPAddress
	{
		get
		{
			return _ipAddress;
		}
		private set
		{
			_ipAddress = value;
			OnPropertyChanged("IPAddress");
		}
	}

	public string SelectedProtocol
	{
		get
		{
			return _selectedProtocol;
		}
		private set
		{
			_selectedProtocol = value;
			OnPropertyChanged("SelectedProtocol");
		}
	}

	public VpnEntitiesServices VpnEntities { get; set; }

	public bool ConnectToUntrustedNetwork { get; set; }

	public DateTime StartTime => SessionStatsTracker?.StartTime ?? DateTime.Now;

	public TimeSpan TimeElapsed => SessionStatsTracker?.TimeElapsed ?? TimeSpan.Zero;

	public bool IsStreamingConnected => StreamingSdk.IsConnected;

	public event ConnectionCompleteHandler ConnectionNotifier;

	public event Action<ConnectionUiEvent> ConnectionUiChanged;

	public event Action<bool> AdvancedSettingsEnableChanged;

	private void RaiseConnectionUi(ConnectionUiEvent uiEvent)
	{
		ConnectionUiChanged?.Invoke(uiEvent);
	}

	internal void SetState(ConnectionState state)
	{
		State = state;
		OnPropertyChanged("State");
		string text = ConnectionStateMapper.ToStatusString(state);
		if (text != null)
		{
			ConnectionStatus = text;
		}
	}

	private void SetSelectedLocationSettings(string countryCode)
	{
		if (NextAiVpnLocation == null)
		{
			return;
		}
		try
		{
			if (string.IsNullOrEmpty(countryCode) && ConnectedVpnMode == VpnType.NextAiVPN)
			{
				VpnExpandedWindow.Mainpanel.ConnectButtonPanel.IsEnabled = false;
				VpnExpandedWindow.Mainpanel.chevronGrid.Visibility = Visibility.Hidden;
				VpnExpandedWindow.Mainpanel.ConnectBtnPanel.Margin = new Thickness(35.0, 0.0, 0.0, 0.0);
			}
			else
			{
				VpnExpandedWindow.Mainpanel.ConnectButtonPanel.IsEnabled = true;
				VpnExpandedWindow.Mainpanel.chevronGrid.Visibility = Visibility.Visible;
				VpnExpandedWindow.Mainpanel.ConnectBtnPanel.Margin = new Thickness(0.0);
			}
		}
		catch (Exception ex)
		{
			_logger.Error(ex.Message, "SetSelectedLocationSettings", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 284);
		}
	}

	public SDKMonitor(VpnEntitiesServices entitiesServices)
	{
		_vpnModesService = entitiesServices.VpnModesService;
		_bugsnagService = entitiesServices.BugsnagService;
		_preferencesRepository = entitiesServices.PreferencesRepository ?? Utils.PreferencesRepository;
		_apiClient = entitiesServices.ApiClient ?? Utils.Api;
		_analyticsService = entitiesServices.AnalyticsService ?? Utils.MixpanelNotification;
		_appSettingsHelper = entitiesServices.AppSettingsHelper ?? Utils.AppSettingsHelper;
		_logger = entitiesServices.Logger ?? Utils.Logger;
		_notificationPresenter = entitiesServices.NotificationPresenter ?? new MessageBoxNotificationPresenter();
		StreamingSdk = entitiesServices.StreamingSdk;
		StreamingLocations = new ObservableCollection<IStreamingLocation>(StreamingSdk.Locations);
		SetStreamingEvents();
		VpnEntities = entitiesServices;
		VpnExpandedWindow = entitiesServices.ExpandedWindow;
		StatusButton = "Connect VPN";
		ProtocolsConnectStatus = "Connect VPN";
		ConnectedStatus = Visibility.Collapsed;
		ReconnectingStatus = Visibility.Collapsed;
		Spinner = Visibility.Collapsed;
		SpinnerProtocols = Visibility.Collapsed;
		_btnLocationsVisible = Visibility.Collapsed;
		_txtLoadingLocations = Visibility.Visible;
		ConnectionInfoPanel = Visibility.Collapsed;
		ProtocolPanelVisible = Visibility.Visible;
		IPAddress = "--.--.--.--";
		try
		{
			_noNetworkTimeOut = 4;
		}
		catch (Exception)
		{
			_noNetworkTimeOut = 15;
		}
		InitSdk();
	}

	public async Task<List<VpnType>> ValidateVpnModes(bool showStreamingMessageBox)
	{
		return await (VpnEntities?.VpnModesValidator?.Validate(showStreamingMessageBox));
	}

	public async Task GetStreamingLocations()
	{
		await StreamingSdk.GetStreamingLocations();
		if (StreamingSdk.Locations.Count <= 0)
		{
			VpnExpandedWindow.ExpandedLocations.StreamingLocations.StreamingLocationsViewModel.ShowHideNoLocationControl(neededToShow: false);
			StreamingLocation = null;
			StreamingLocations.Clear();
			return;
		}
		if (StreamingLocation == null)
		{
			StreamingLocation = StreamingSdk.Locations.First();
		}
		IEnumerable<Task<ushort?>> tasks = from l in StreamingSdk.Locations
			where !l.PingMs.HasValue
			select l.Ping();
		VpnExpandedWindow.ExpandedLocations.StreamingLocations.StreamingLocationsViewModel.ShowHideNoLocationControl(neededToShow: true);
		StreamingLocations = new ObservableCollectionExtended<IStreamingLocation>(StreamingSdk.Locations);
		VpnExpandedWindow.ExpandedLocations.StreamingLocations.StreamingLocationsViewModel.Locations = StreamingLocations;
		VpnExpandedWindow.ExpandedLocations.StreamingLocations.StreamingLocationsViewModel.SearchString = VpnExpandedWindow.ExpandedLocations.StreamingLocations.StreamingLocationsViewModel.SearchString;
		_logger.Information("Updated streaming locations", "GetStreamingLocations", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 557);
		await Task.WhenAll(tasks);
	}

	private void SetStreamingEvents()
	{
		if (StreamingSdk != null)
		{
			StreamingLocation = StreamingSdk.Locations.FirstOrDefault();
			SdkManager streamingSdk = StreamingSdk;
			streamingSdk.VpnConnectionStatusChanged = (Action<object>)Delegate.Combine(streamingSdk.VpnConnectionStatusChanged, new Action<object>(Streaming_VpnConnectionStatusChanged));
			SdkManager streamingSdk2 = StreamingSdk;
			streamingSdk2.Error = (Action<object>)Delegate.Combine(streamingSdk2.Error, new Action<object>(Streaming_Error));
		}
	}

	public virtual void InitSdk()
	{
		NextAiVpnSdkManager = SDKInstance.GetInstance();
		NextAiVpnSdkManager.UserLocationStatusChanged += SdkManager_UserLocationStatusChanged;
		NextAiVpnSdkManager.VpnConnectionStatusChanged += SdkManager_VpnConnectionStatusChanged;
		NextAiVpnSdkManager.TapDeviceInstallationStatusChanged += SdkManager_TapDeviceInstallationStatusChanged;
		NextAiVpnSdkManager.LocationsRefreshStatusChanged += SdkManager_LocationsRefreshStatusChanged;
	}

	public VpnType GetVpnMode()
	{
		return _vpnModesService.GetVpnMode();
	}

	public void SetVpnMode(VpnType vpnType)
	{
		_vpnModesService.SetVpnMode(vpnType);
		ConnectedVpnMode = vpnType;
		VpnModeChangeEvent.OnVpnModeChanged(vpnType);
		if (NextAiVpnLocation != null)
		{
			string countryCode = NextAiVpnLocation.CountryCode;
			if (NextAiVpnLocation.Id.Equals("bestavailable") && NextAiVpnSdkManager.ActiveConnectionInformation != null)
			{
				countryCode = NextAiVpnSdkManager.ActiveConnectionInformation.Location.CountryCode;
			}
			SetSelectedLocationSettings(countryCode);
		}
	}

	private void SdkManager_LocationsRefreshStatusChanged(ISDK sender, RefreshLocationListStatus args)
	{
	}

	private void SdkManager_TapDeviceInstallationStatusChanged(ISDK sender, OperationStatus status)
	{
	}

	private void Monitor(string emessage, string previous, string current, bool manual)
	{
		_logger?.Information("Event: " + emessage + " - Previous: " + previous + " - Current: " + current + " - Bool ManualDisconnect: " + manual, "Monitor", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 615);
	}

	private async void SdkManager_VpnConnectionStatusChanged(ISDK sender, VpnSDK.Enums.ConnectionStatus previous, VpnSDK.Enums.ConnectionStatus current)
	{
		switch (current)
		{
		case VpnSDK.Enums.ConnectionStatus.Connecting:
			Connecting();
			ConnectionNotifier?.Invoke("Connecting");
			Monitor("Connecting", previous.ToString(), current.ToString(), ManualDisconnect);
			return;
		case VpnSDK.Enums.ConnectionStatus.Connected:
			Connected();
			ConnectionNotifier?.Invoke("Connected");
			Monitor("Connected", previous.ToString(), current.ToString(), ManualDisconnect);
			return;
		case VpnSDK.Enums.ConnectionStatus.Disconnecting:
			Disconnecting();
			ConnectionNotifier?.Invoke("Disconnecting");
			Monitor("Disconnecting", previous.ToString(), current.ToString(), ManualDisconnect);
			return;
		case VpnSDK.Enums.ConnectionStatus.Disconnected:
			if (ManualDisconnect)
			{
				Disconnected();
				ConnectionNotifier?.Invoke("Disconnected");
				Monitor("Disconnected", previous.ToString(), current.ToString(), ManualDisconnect);
				return;
			}
			break;
		}
		if (((previous == VpnSDK.Enums.ConnectionStatus.Connected && current == VpnSDK.Enums.ConnectionStatus.Disconnected) || (previous == VpnSDK.Enums.ConnectionStatus.Connecting && current == VpnSDK.Enums.ConnectionStatus.Disconnected)) && !ManualDisconnect && !(await InternetConnection.IsAvailableAsync()))
		{
			await Task.Delay(2000);
			if (!(await InternetConnection.IsAvailableAsync()))
			{
				SetNetworkStatus();
			}
			else if (previous == VpnSDK.Enums.ConnectionStatus.Connected)
			{
				HandleConnectionError("Connection lost, process is reconnecting", isConnectionLost: true);
				Monitor("Exception", previous.ToString(), current.ToString(), ManualDisconnect);
			}
			else if (!IsReconnecting)
			{
				HandleConnectionError("Unable to connect to location");
				Monitor("Exception", previous.ToString(), current.ToString(), ManualDisconnect);
			}
		}
	}

	private void Disconnecting()
	{
		SetState(ConnectionState.Disconnecting);
		StatusButton = "Disconnecting";
	}

	public async Task SetNetworkStatus()
	{
		if (IsStreamingConnected || NextAiVpnSdkManager.IsConnected || NextAiVpnSdkManager.IsConnecting)
		{
			await DisconnectVPN();
		}
		_logger?.Information("There is no internet connection", "SetNetworkStatus", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 684);
		VpnExpandedWindow.ShowNoConnection();
		TaskBarService.SetNotifyIcon("nonetwork", _noNetworkBalloon);
	}

	public void Connecting()
	{
		SetState(ConnectionState.Connecting);
		IsError = false;
		Spinner = Visibility.Visible;
		StatusButton = "";
		RaiseConnectionUi(ConnectionUiEvent.Connecting);
		if (IsReconnecting)
		{
			blinkingConnection = "";
			ReconnectingStatus = Visibility.Visible;
			ConnectedStatus = Visibility.Collapsed;
		}
		else
		{
			blinkingConnection = "Connecting VPN";
		}
	}

	public void ConnectingStreaming()
	{
		if (StreamingLocation != null)
		{
			VpnExpandedWindow.Mainpanel.ReconnectingBlink.Visibility = Visibility.Visible;
			VpnExpandedWindow.Mainpanel.ImageReconnecting.Visibility = Visibility.Visible;
			VpnExpandedWindow.ExpandedLocations.StreamingLocations.SetCountryFlagSpinnerAnimation(StreamingLocation.Country);
			SetStreamingLocation(StreamingLocation);
			SetFlagToLocation(StreamingLocation.CountryCode);
			Connecting();
		}
	}

	public virtual async Task ConnectToVPN(ILocation location)
	{
		await ConnectionOrchestrator.ConnectAsync(ConnectedVpnMode, location);
	}

	public virtual async Task ConnectToVPN(VpnType vpnMode)
	{
		await ConnectionOrchestrator.ConnectAsync(vpnMode, NextAiVpnLocation);
	}

	internal async Task ConnectToStreamingServers(IStreamingLocation location)
	{
		if ((await ValidateVpnModes(showStreamingMessageBox: false)).Any((VpnType x) => x == VpnType.Streaming) || IsExpandedWindowNull("ConnectToStreamingServers - VpnExpandedWindow Null"))
		{
			return;
		}
		if (await SubscriptionFlowCoordinator.IsSubscriptionActiveAsync())
		{
			if (await InitiateVpnConnectionToStreaming(location))
			{
				return;
			}
		}
		else
		{
			SubscriptionStatus subscriptionStatus = await VpnEntities.SubscriptionStatusHelper.GetSubscriptionStatusInfo();
			if (subscriptionStatus == null)
			{
				_logger.Warning("Streaming subscription status. Status: null", "ConnectToStreamingServers", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 762);
				return;
			}
			if (subscriptionStatus.Valid.Equals("0") && !string.IsNullOrEmpty(subscriptionStatus.Status))
			{
				_logger.Warning("Streaming subscription status. Status: " + subscriptionStatus.Status + "\nValid: " + subscriptionStatus.Valid, "ConnectToStreamingServers", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 770);
				switch (subscriptionStatus.Status)
				{
				case "needs_subscription_update":
					ShowExpiredSubscriptionWindow();
					break;
				case "needs_subscription":
					VpnEntities.MessageWindow.ShowDialog();
					break;
				case "no account found for this access token. please relogin":
					_notificationPresenter.ShowError("Please Re-login the application.");
					break;
				}
				await DisconnectedStreaming();
			}
		}
		EnableDisableExpandedViews(Visibility.Collapsed);
	}

	private async Task<bool> InitiateVpnConnectionToStreaming(IStreamingLocation location)
	{
		ConnectedVpnMode = VpnType.Streaming;
		EnableDisableExpandedViews(Visibility.Visible);
		try
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					VpnExpandedWindow?.SettingsControlViewModel?.CheckUpdates();
				}
				catch (Exception ex)
				{
					_logger?.Error(ex, "CheckUpdates.QueueUserWorkItem", "SDKMonitor.cs", 819);
				}
			});
			if (await StreamingSdk.Connect(location))
			{
				SendConnectedMixpanelNotification(location);
			}
		}
		catch (StreamingConnectionException ex)
		{
			EnableStreamingSplitTunnelingManageButton(state: true);
			EnableDisableExpandedViews(Visibility.Collapsed);
			await Task.Delay(30000);
			await StreamingReconnect(ex, 7);
		}
		catch (Exception ex2)
		{
			EnableStreamingSplitTunnelingManageButton(state: true);
			EnableDisableExpandedViews(Visibility.Collapsed);
			await StreamingReconnect(ex2, 3);
			return true;
		}
		return false;
	}

	private async Task StreamingReconnect(Exception ex, int connectionAttempts)
	{
		ReconnectVerdict verdict = ConnectionOrchestrator.EvaluateStreamingReconnect(_reconnectCounter + 1, connectionAttempts);
		if (verdict == ReconnectVerdict.Stop)
		{
			return;
		}
		await GetStreamingLocations();
		_reconnectCounter++;
		if (verdict == ReconnectVerdict.Retry)
		{
			IsReconnecting = true;
			_logger.Information($"Streaming reconnection attempt {_reconnectCounter}", "StreamingReconnect", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 839);
			await ConnectToStreamingServers(StreamingLocation);
			return;
		}
		VpnExpandedWindow.Mainpanel.NotificationControlViewModel.UserControlVisibility = Visibility.Visible;
		ShowErrorScreen();
		VpnExpandedWindow.ExpandedLocations.StreamingLocations.SetCountryFlag(StreamingLocation.Country, StreamingLocation.CountryCode);
		if (await InternetConnection.IsAvailableAsync())
		{
			_bugsnagService.Notify("[ConnectToVPN Streaming] method: ConnectToStreamingServers - message: " + ex.Message);
		}
	}

	private void SendConnectedMixpanelNotification(IStreamingLocation location)
	{
		try
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				_analyticsService.SendCustomConnectNotification("Connected streaming", location.FullLocationName);
			});
		}
		catch (Exception exception)
		{
			_logger.Error(exception, "SendConnectedMixpanelNotification", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 867);
		}
	}

	private void ShowExpiredSubscriptionWindow()
	{
		if (SubscriptionExpiredWindow == null)
		{
			SubscriptionExpiredWindow = new SubscriptionExpiredWindow(this);
		}
		SubscriptionExpiredWindow.SetVisibleControl("MainSubscription");
		SubscriptionExpiredWindow.Show();
		if (VpnExpandedWindow != null)
		{
			VpnExpandedWindow.Hide();
		}
	}

	private async void Streaming_Error(object obj)
	{
		await StreamingSdk.RemoveVpn();
		SubscriptionStatus subscriptionStatus = await VpnEntities.SubscriptionStatusHelper.GetSubscriptionStatusInfo();
		if (subscriptionStatus.ErrorType != null && !subscriptionStatus.ErrorType.Equals("0"))
		{
			_logger.Warning($"Streaming subscription status. Error type: {subscriptionStatus.ErrorType}\nMessage: {subscriptionStatus.UserMessage}\nValid: {subscriptionStatus.Valid}", "Streaming_Error", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 893);
			VpnExpandedWindow.Dispatcher.Invoke(delegate
			{
				_appSettingsHelper.SetValue("IsStreamingDeviceLimitReached", "1");
				Task.Run(async delegate
				{
					await DisconnectedStreaming();
				});
				VpnEntities.DeviceLimitWindowViewModel.Message = ((!string.IsNullOrEmpty(subscriptionStatus.UserMessage)) ? subscriptionStatus.UserMessage : "Subscription error");
				new DeviceLimitWindow(VpnEntities.DeviceLimitWindowViewModel).ShowDialog();
			});
		}
		else
		{
			_appSettingsHelper.SetValue("IsStreamingDeviceLimitReached", "0");
			VpnExpandedWindow.Dispatcher.Invoke(delegate
			{
				ShowErrorScreen();
			});
			_bugsnagService.Notify($"[ConnectToVPN Streaming Error] method: {"Streaming_Error"} - message: {obj}");
		}
	}

	private void Streaming_VpnConnectionStatusChanged(object obj)
	{
		StreamingConnectionStatus streamingConnectionStatus = (StreamingConnectionStatus)obj;
		switch (streamingConnectionStatus)
		{
		case StreamingConnectionStatus.Disconnected:
			VpnExpandedWindow.Dispatcher.BeginInvoke(new Func<Task>(DisconnectedStreaming), null);
			break;
		case StreamingConnectionStatus.Connecting:
			VpnExpandedWindow.Dispatcher.BeginInvoke(new Action(ConnectingStreaming), null);
			break;
		case StreamingConnectionStatus.Connected:
			VpnExpandedWindow.Dispatcher.BeginInvoke(new Action(ConnectedStreaming), null);
			break;
		case StreamingConnectionStatus.Disconnecting:
			VpnExpandedWindow.Dispatcher.BeginInvoke(new Action(Disconnecting), null);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		ConnectionNotifier?.Invoke(streamingConnectionStatus.ToString());
	}

	internal async Task ConnectToNextAiVpnServers(ILocation location)
	{
		if (IsExpandedWindowNull("ConnectToNextAiVpnServers - VpnExpandedWindow Null"))
		{
			return;
		}
		ConnectedVpnMode = VpnType.NextAiVPN;
		EnableDisableExpandedViews(Visibility.Visible);
		if (!InternetConnection.IsAvailable())
		{
			_logger?.Information("There is no internet connection", "ConnectToNextAiVpnServers", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 946);
			VpnExpandedWindow.ShowNoConnection();
			TaskBarService.SetNotifyIcon("nonetwork", _noNetworkBalloon);
		}
		else
		{
			// [VI] Kết nối trực tiếp vào Universal Gateway của NextAI VPN không kiểm tra TAP hay subscription cũ
			// [EN] Connect directly to NextAI VPN Universal Gateway without TAP or legacy subscription checks
			if (await InitiateVpnConnectionToNextAiVpn(location))
			{
				return;
			}
		}
		EnableDisableExpandedViews(Visibility.Collapsed);
	}

	private async Task NextAiGlobalConnection(ILocation location)
	{
		if (await SubscriptionFlowCoordinator.IsSubscriptionActiveAsync())
		{
			await InitiateVpnConnectionToNextAiVpn(location);
			return;
		}
		SubscriptionStatus subscriptionStatus = await VpnEntities.SubscriptionStatusHelper.GetSubscriptionStatusInfo();
		if (subscriptionStatus.Valid.Equals("0") && !string.IsNullOrEmpty(subscriptionStatus.Status))
		{
			_logger.Warning("NextAiGlobal subscription status. Status: " + subscriptionStatus.Status + "\nValid: " + subscriptionStatus.Valid, "NextAiGlobalConnection", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1052);
			_logger.Warning("NextAiGlobal subscription status. Error type: " + subscriptionStatus.ErrorType + "\nUser message: " + subscriptionStatus.UserMessage, "NextAiGlobalConnection", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1053);
			switch (subscriptionStatus.Status)
			{
			case "needs_subscription":
			case "needs_subscription_update":
				VpnEntities.MessageWindow.ShowDialog();
				break;
			case "no account found for this access token. please relogin":
				_notificationPresenter.ShowError("Please Re-login the application.");
				break;
			}
		}
	}

	internal bool HasMultipleServersForCurrentLocation()
	{
		IList<VpnSDK.Interfaces.IServer> nextaivpnLocationServers = GetNextAiVpnLocationServers(NextAiVpnLocation);
		if (nextaivpnLocationServers != null)
		{
			return nextaivpnLocationServers.Count > 1;
		}
		return false;
	}

	private IList<VpnSDK.Interfaces.IServer> GetNextAiVpnLocationServers(ILocation location)
	{
		IList<VpnSDK.Interfaces.IServer> result = null;
		try
		{
			if (location.Id.ToLower().Equals("bestavailable"))
			{
				string countryCode = NextAiVpnSdkManager.ActiveConnectionInformation.Location.CountryCode;
				string cityCode = NextAiVpnSdkManager.ActiveConnectionInformation.Location.CityCode;
				ILocation location2 = NextAiVpnSdkManager.Locations.FirstOrDefault((ILocation x) => x.CityCode.Equals(cityCode) && x.CountryCode.Equals(countryCode));
				if (location2 != null && !location2.Id.ToLower().Equals("bestavailable"))
				{
					result = GetNextAiVpnLocationServers(location2);
				}
			}
			PropertyInfo property = location.GetType().GetProperty("Children");
			if (property != null)
			{
				result = property.GetValue(location) as IList<VpnSDK.Interfaces.IServer>;
			}
		}
		catch (Exception ex)
		{
			_logger.Error(ex.Message, "GetNextAiVpnLocationServers", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1108);
		}
		return result;
	}

	public async Task ShuffleIp()
	{
		_ = 3;
		try
		{
			IList<VpnSDK.Interfaces.IServer> servers = GetNextAiVpnLocationServers(NextAiVpnLocation);
			if (servers == null || servers.Count <= 0)
			{
				return;
			}
			if (_locationUsedServers.Count >= servers.Count)
			{
				_locationUsedServers.Clear();
			}
			List<VpnSDK.Interfaces.IServer> filtered = servers.Where((VpnSDK.Interfaces.IServer x) => _locationUsedServers.All((VpnSDK.Interfaces.IServer y) => y.Id != x.Id)).ToList();
			if (filtered.Count < 1)
			{
				_locationUsedServers.Clear();
				await ConnectToNextAiVpnServer(servers.First((VpnSDK.Interfaces.IServer x) => x.Ip.ToString() != IPAddress));
			}
			char? lastChar = GetLastChar(IPAddress);
			if (lastChar.HasValue)
			{
				string text = IPAddress.Substring(0, IPAddress.Length - 1);
				char? c = lastChar;
				string newIp = text + c;
				VpnSDK.Interfaces.IServer server = filtered.LastOrDefault((VpnSDK.Interfaces.IServer x) => x.Ip.ToString() != newIp);
				if (server != null)
				{
					_locationUsedServers.Add(server);
					await ConnectToNextAiVpnServer(server);
					return;
				}
				await ConnectToNextAiVpnServer(servers.First((VpnSDK.Interfaces.IServer x) => x.Ip.ToString() != IPAddress));
			}
			await ConnectToNextAiVpnServer(servers.First((VpnSDK.Interfaces.IServer x) => x.Ip.ToString() != IPAddress));
		}
		catch (Exception ex)
		{
			_logger.Error(ex.Message, "ShuffleIp", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1159);
		}
	}

	private async Task ConnectToNextAiVpnServer(VpnSDK.Interfaces.IServer server)
	{
		await DisconnectVPN();
		await NextAiVpnSdkManager.Connect(server, _connectionConfiguration, _connectionTokenSource.Token);
	}

	private static char? GetLastChar(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return null;
		}
		return input[input.Length - 1];
	}

	private async Task<bool> InitiateVpnConnectionToNextAiVpn(ILocation location)
	{
		VpnExpandedWindow?.Mainpanel.NetworkInfoShow(Visibility.Collapsed);
		if (State == ConnectionState.Connected && NextAiVpnSdkManager.IsConnected)
		{
			await DisconnectVPN();
		}
		try
		{
			Connecting();
			await Task.Delay(350);

			if (location != null)
			{
				await NextAiVPN.Services.NextAiLocationService.SelectUpstreamProxyAsync(location.Id);
			}

			// [VI] Kích hoạt Windows System Proxy để điều hướng toàn bộ trình duyệt qua Gateway port 10000
			// [EN] Activate Windows System Proxy to route all browsers through Gateway port 10000
			NextAiVPN.Services.NextAiLocationService.EnableSystemProxy("127.0.0.1", 10000);
			NextAiVPN.Services.NextAiLocationService.SetActiveConnection(NextAiVpnSdkManager, location ?? NextAiVpnLocation, true);

			Connected();

			string displayIp = "Connected (Secure)";
			if (location != null)
			{
				if (!string.IsNullOrEmpty(location.Id) && location.Id.Contains(":"))
				{
					var parts = location.Id.Split(':');
					displayIp = $"{parts[0]} ({location.City})";
				}
				else if (!string.IsNullOrEmpty(location.City) && !location.City.Equals("Auto", StringComparison.OrdinalIgnoreCase))
				{
					displayIp = $"{location.City}, {location.Country}";
				}
				else
				{
					displayIp = $"{location.Country}";
				}
			}
			IPAddress = displayIp;
			SelectedProtocol = "GATEWAY:10000";
			if (VpnExpandedWindow != null)
			{
				VpnExpandedWindow.Dispatcher.Invoke(() =>
				{
					if (VpnExpandedWindow.Mainpanel?.ConnectionDataViewModel != null)
					{
						VpnExpandedWindow.Mainpanel.ConnectionDataViewModel.IpAddress = displayIp;
						VpnExpandedWindow.Mainpanel.ConnectionDataViewModel.Protocol = "GATEWAY:10000";
						VpnExpandedWindow.Mainpanel.ConnectionDataViewModel.ControlVisibility = Visibility.Visible;
					}
					VpnExpandedWindow.Mainpanel?.BtnConnectColor(connected: true);
				});
			}
			return true;
		}
		catch (Exception ex)
		{
			_logger?.Error(ex, "InitiateVpnConnectionToNextAiVpn", "SDKMonitor.cs", 1200);
			HandleConnectionError("Unable to connect to NextAI Gateway: " + ex.Message);
			return false;
		}
	}

	private bool IsExpandedWindowNull(string errorMessage)
	{
		if (VpnExpandedWindow == null)
		{
			_logger.Error(errorMessage, "IsExpandedWindowNull", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1322);
			_bugsnagService.Notify(errorMessage);
			return true;
		}
		return false;
	}

	private void EnableDisableExpandedViews(Visibility visibility)
	{
		VpnExpandedWindow.Dispatcher.Invoke(delegate
		{
			VpnExpandedWindow?.ExpandedLocations.SetDisablerRectangleVisibility(visibility);
			VpnExpandedWindow.Mainpanel.DisablerRectangle.Visibility = visibility;
		});
	}

	private void EnableDisableSettingsOnLogin(bool isEnabled)
	{
		if (isEnabled)
		{
			VpnExpandedWindow?.SettingsControlViewModel.EnableControl();
			VpnExpandedWindow?.ExpandedLocations.EnableControl();
			if (_isSteamingModeAvailable)
			{
				TaskBarService?.ContextMenuShow(string.Empty, " - ");
				if (!IsStreamingConnected || !NextAiVpnSdkManager.IsConnected)
				{
					TaskBarService?.HideConnectingLocationMenuItem();
				}
			}
			else
			{
				TaskBarService.ContextMenuShow("For Streaming", string.Empty);
			}
		}
		else
		{
			VpnExpandedWindow?.SettingsControlViewModel.DisableControl();
			VpnExpandedWindow?.ExpandedLocations.DisableControl();
			TaskBarService?.ContextMenuHide("Quit");
		}
	}

	public void Connected()
	{
		ConnectedCommon();
		RaiseConnectionUi(ConnectionUiEvent.ShuffleIpAvailability);
		TaskBarService.SetNotifyIcon("connected", _connectedBalloon);
		_apiClient.SaveConnectedTo(NextAiVpnLocation.Id);
		SessionStatsTracker.StopSession();
		SessionStatsTracker.StartSession();
		SendMixpanelConnectedEvent(NextAiVpnSdkManager.ActiveConnectionInformation.Location.City);
		EnableAdvancedSettings(isEnabled: false);
		SetLocationIfBestAvailable(isConnected: true);
	}

	private void EnableAdvancedSettings(bool isEnabled)
	{
		AdvancedSettingsEnableChanged?.Invoke(isEnabled);
	}

	private void ConnectedCommon()
	{
		_reconnectCounter = 0;
		if (!InternetConnection.IsAvailable())
		{
			VpnExpandedWindow.HideNoConnection();
			TaskBarService.SetNotifyIcon("regular", _noNetworkBalloon);
			SetState(ConnectionState.Disconnected);
		}
		SessionStatsTracker.TrackNetworkInterface(NetworkService.GetActiveNetworkInterface());
		_bugsnagNotified = false;
		StatusButton = "Disconnect";
		Spinner = Visibility.Collapsed;
		SpinnerProtocols = Visibility.Collapsed;
		ConnectionInfoPanel = Visibility.Visible;
		ConnectedStatus = Visibility.Visible;
		ReconnectingStatus = Visibility.Collapsed;
		ProtocolPanelVisible = Visibility.Collapsed;
		IncreaseConnectionCounter();
		SetState(ConnectionState.Connected);
		IsReconnecting = false;
		SelectedProtocol = GetConnectedProtocol();
		RaiseConnectionUi(ConnectionUiEvent.Connected);
	}

	private ExpandedStreamingSettingsControlViewModel GetStreamingSettingsViewModel()
	{
		return VpnExpandedWindow.GeneralControl.StreamingSettingsControl.DataContext as ExpandedStreamingSettingsControlViewModel;
	}

	public void ConnectedStreaming()
	{
		try
		{
			EnableStreamingSplitTunnelingManageButton(state: false);
			Task.Run(async delegate
			{
				ConnectionDataViewModel connectionDataViewModel = VpnExpandedWindow.Mainpanel.ConnectionDataViewModel;
				return connectionDataViewModel.IpAddress = await GetStreamingServerIp(StreamingLocation.Servers);
			});
			_appSettingsHelper.SetValue("IsStreamingDeviceLimitReached", "0");
			VpnExpandedWindow.ExpandedLocations.StreamingLocations.SetCountryFlagConnected(StreamingLocation.Country);
			SessionStatsTracker.CaptureStreamingStartPoints();
			ConnectedCommon();
			TaskBarService.SetNotifyIcon("connected", _connectedStreamingBalloon, VpnType.Streaming);
			SessionStatsTracker.StopStreamingTracking();
			SessionStatsTracker.ResetClock();
			SessionStatsTracker.StartStreamingTracking();
			SendMixpanelConnectedEvent(StreamingLocation.FullLocationName);
			SetStreamingLocation(StreamingLocation);
			SetFlagToLocation(StreamingLocation.CountryCode);
		}
		catch (Exception ex)
		{
			_logger.Error(ex.Message, "ConnectedStreaming", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1448);
		}
	}

	private async Task<string> GetStreamingServerIp(IList<NextAiVPN.Streaming.Entities.IServer> streamingLocationServers)
	{
		string text = await VpnEntities.IpHelper.GetPublicIpAsync(streamingLocationServers.Select((NextAiVPN.Streaming.Entities.IServer x) => x.Name));
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return "--.--.--.--";
	}

	private string GetConnectedProtocol()
	{
		switch (ConnectedVpnMode)
		{
		case VpnType.NextAiVPN:
			return _appSettingsHelper.GetValue("protocol");
		case VpnType.Streaming:
			return _appSettingsHelper.GetValue("StreamingProtocol");
		default:
			_logger.Error("No such protocols", "GetConnectedProtocol", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1471);
			return _appSettingsHelper.GetValue("protocol");
		}
	}

	private void SetLocationIfBestAvailable(bool isConnected)
	{
		if (NextAiVpnLocation == null)
		{
			NextAiVpnLocation = NextAiVpnSdkManager.Locations.FirstOrDefault((ILocation x) => x.Id.Equals("bestavailable"));
			if (NextAiVpnLocation == null)
			{
				return;
			}
		}
		if (!NextAiVpnLocation.Id.Equals("bestavailable"))
		{
			SetSelectedLocationSettings(NextAiVpnLocation.CountryCode);
		}
		else if (isConnected)
		{
			if (NextAiVpnSdkManager.ActiveConnectionInformation == null)
			{
				SetLocationBestAvailableByDefault();
				return;
			}
			Country = NextAiVpnSdkManager.ActiveConnectionInformation.Location.Country;
			City = NextAiVpnSdkManager.ActiveConnectionInformation.Location.City;
			SetFlagToLocation(NextAiVpnSdkManager.ActiveConnectionInformation.Location.CountryCode);
			SetSelectedLocationSettings(NextAiVpnSdkManager.ActiveConnectionInformation.Location.CountryCode);
		}
		else
		{
			SetLocationBestAvailableByDefault();
		}
	}

	private void IncreaseConnectionCounter()
	{
		IncreaseVariableCounterByOne("ConnectionCounter");
		IncreaseVariableCounterByOne("ConnectionsToShowFeedbackCounter");
		IncreaseVariableCounterByOne("ConnectionsAfterSkipCounter");
	}

	private void IncreaseVariableCounterByOne(string variableName)
	{
		int.TryParse(_appSettingsHelper.GetValue(variableName), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result);
		result++;
		_appSettingsHelper.SetValue(variableName, result.ToString());
	}

	private void SetLocationBestAvailableByDefault()
	{
		Country = "Best Available";
		City = string.Empty;
		SetFlagToLocation(null);
		SetSelectedLocationSettings(NextAiVpnLocation.CountryCode);
	}

	public void SetFlagToLocation(string countryCode)
	{
		if (VpnExpandedWindow == null)
		{
			return;
		}
		try
		{
			string locationImageSource = ((!string.IsNullOrEmpty(countryCode)) ? ("/Resources/Flags/" + countryCode.ToLower() + ".png") : ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? "/Resources/Flags/bestavailable.png" : "/Resources/Flags/bestavailable_darkMode.png"));
			VpnExpandedWindow.Mainpanel.SetLocationImageSource(locationImageSource);
		}
		catch (Exception)
		{
		}
	}

	public void Disconnected()
	{
		DisconnectedCommon();
		SessionStatsTracker.StopSession();
		EnableAdvancedSettings(isEnabled: true);
		SetLocationIfBestAvailable(isConnected: false);
	}

	private Task DisconnectedCommon()
	{
		return Task.Run(delegate
		{
			ManualDisconnect = false;
			StatusButton = "Connect VPN";
			ProtocolsConnectStatus = "Connect VPN";
			Spinner = Visibility.Collapsed;
			ConnectionInfoPanel = Visibility.Collapsed;
			ConnectedStatus = Visibility.Collapsed;
			ProtocolPanelVisible = Visibility.Visible;
			ConnectFromFavorites = false;
			RaiseConnectionUi(ConnectionUiEvent.Disconnected);
			InvokeOnUiThread(delegate
			{
				TaskBarService.SetNotifyIcon("regular", null);
				SetState(ConnectionState.Disconnected);
			});
			_logger?.Information("Disconnected from the VPN.", "DisconnectedCommon", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1588);
		});
	}

	private static void InvokeOnUiThread(Action action)
	{
		Dispatcher dispatcher = Application.Current?.Dispatcher;
		if (dispatcher != null)
		{
			dispatcher.Invoke(action);
		}
		else
		{
			action();
		}
	}

	private void EnableStreamingSplitTunnelingManageButton(bool state)
	{
		GetStreamingSettingsViewModel().IsManageButtonActive = state;
	}

	private async Task DisconnectedStreaming()
	{
		await DisconnectedCommon();
		EnableStreamingSplitTunnelingManageButton(state: true);
		VpnExpandedWindow.ExpandedLocations.StreamingLocations.SetCountryFlag(StreamingLocation.Country, StreamingLocation.CountryCode);
		SessionStatsTracker.StopStreamingTracking();
	}

	public void HandleConnectionError(string error, bool isConnectionLost = false)
	{
		RaiseConnectionUi(ConnectionUiEvent.ConnectionFailed);
		ConnectFromFavorites = false;
		SpinnerProtocols = Visibility.Collapsed;
		if (!_bugsnagNotified)
		{
			if (!isConnectionLost)
			{
				_bugsnagService.Notify("[ConnectToVPN] - " + error);
			}
			_bugsnagNotified = true;
		}
		if (State == ConnectionState.Connected)
		{
			StatusButton = "";
			ConnectedStatus = Visibility.Collapsed;
			blinkingConnection = "";
			SetNetworkStatus();
			SaveDataUsageOnDisconnect();
			IsReconnecting = true;
			SetState(ConnectionState.Reconnecting);
			_reconnectCounter++;
		}
		if (!IsReconnecting)
		{
			RaiseConnectionUi(ConnectionUiEvent.FavoritesConnectError);
			EnableAdvancedSettings(isEnabled: true);
			ShowErrorScreen();
		}
		else if (!NextAiVpnSdkManager.IsConnecting)
		{
			ConnectToVPN(NextAiVpnLocation);
		}
		if (IsReconnecting)
		{
			if (_reconnectCounter == 1)
			{
				TaskBarService.SetNotifyIcon("nonetwork", _noNetworkBalloon);
				_reconnectCounter++;
			}
			RaiseConnectionUi(ConnectionUiEvent.ReconnectSpinner);
		}
		_logger?.Error($"The VPN connection encountered an error: {error}. Seconds without network: {_reconnectCounter}", "HandleConnectionError", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1677);
	}

	private void ShowErrorScreen()
	{
		ReconnectingStatus = Visibility.Collapsed;
		ManualDisconnect = false;
		Spinner = Visibility.Collapsed;
		IsError = true;
		TaskBarService.SetNotifyIcon("error", _errorBalloon);
		SetState(ConnectionState.Error);
		RaiseConnectionUi(ConnectionUiEvent.ErrorScreenShown);
		StatusButton = "Reconnect";
		ProtocolsConnectStatus = "Reconnect";
		SpinnerProtocols = Visibility.Collapsed;
	}

	private void HideErrorScreen()
	{
		RaiseConnectionUi(ConnectionUiEvent.ErrorScreenHidden);
		SetState(ConnectionState.Disconnected);
	}

	private async void SdkManager_UserLocationStatusChanged(ISDK sender, OperationStatus status, NetworkGeolocation args)
	{
		if (GetVpnMode() == VpnType.Streaming)
		{
			return;
		}
		switch (status)
		{
		case OperationStatus.Completed:
			if (args == null)
			{
				break;
			}
			try
			{
				IPAddress = ((args.IPAddress == null) ? "--.--.--.--" : args.IPAddress.ToString());
				VpnExpandedWindow.Mainpanel.ConnectionDataViewModel.IpAddress = IPAddress;
				if (await InternetConnection.IsAvailableAsync())
				{
					break;
				}
				if (IsReconnecting || NextAiVpnSdkManager.IsConnecting)
				{
					await Task.Delay(8000);
					if (!NextAiVpnSdkManager.IsDisposed)
					{
						if (IsReconnecting)
						{
							_bugsnagService.Notify("[ConnectToVPN] - Connection lost, process is reconnecting");
							IsReconnecting = false;
						}
						if (!NextAiVpnSdkManager.IsConnecting)
						{
							ConnectToVPN(NextAiVpnLocation);
						}
					}
				}
				else
				{
					TaskBarService.SetNotifyIcon("regular", _noNetworkBalloon);
					VpnExpandedWindow.HideNoConnection();
				}
				break;
			}
			catch (Exception ex)
			{
				_bugsnagService.Notify(ex.Message);
				break;
			}
		case OperationStatus.InProgress:
		case OperationStatus.Failed:
			break;
		}
	}

	public void ShowNoSubscriptionWindow()
	{
		VpnEntities.MainWindow.SubscriptionExpiredWindow.SetVisibleControl(SubscriptionFlowCoordinator.GetNoSubscriptionControl());
		VpnEntities.MainWindow.SubscriptionExpiredWindow.Show();
		VpnExpandedWindow.Hide();
		_logger.Information("Show no subscription window", "ShowNoSubscriptionWindow", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1764);
	}

	private void ShowTrialFirstWindow()
	{
		Application.Current?.Dispatcher.InvokeAsync(delegate
		{
			VpnEntities.TrialFirstViewModel?.OpenDialog();
		});
	}

	private async void UpdateTrialFirstChips(TrialChipsUpdate update)
	{
		TrialFirstChipsViewModel trialFirstChipsViewModel = GetTrialFirstChipsViewModel();
		if (trialFirstChipsViewModel != null)
		{
			switch (update)
			{
			case TrialChipsUpdate.ShowAndRefresh:
				trialFirstChipsViewModel.ControlVisibility = Visibility.Visible;
				await trialFirstChipsViewModel.RefreshData(VpnEntities.SubscriptionInfo);
				break;
			case TrialChipsUpdate.Refresh:
				await trialFirstChipsViewModel.RefreshData(VpnEntities.SubscriptionInfo);
				break;
			case TrialChipsUpdate.Hide:
				trialFirstChipsViewModel.ControlVisibility = Visibility.Collapsed;
				break;
			}
		}
	}

	private TrialFirstChipsViewModel GetTrialFirstChipsViewModel()
	{
		return VpnExpandedWindow.Mainpanel.TrialFirstChips.DataContext as TrialFirstChipsViewModel;
	}

	public async Task LoginToVpn()
	{
		if (NextAiVpnSdkManager != null)
		{
			try
			{
				HideErrorScreen();
				EnableDisableExpandedViews(Visibility.Visible);
				EnableDisableSettingsOnLogin(isEnabled: false);
				if (!(await AuthenticationFlow.HasActiveSubscriptionAsync()))
				{
					ShowNoSubscriptionWindow();
					return;
				}
				SubscriptionFlowCoordinator.ShowTrialFirstIfNeededAsync();
				bool flag = await AuthenticationFlow.EnsureKillSwitchPermissionAsync(IsRunOnStartUpAllowed);
				if (VpnExpandedWindow != null && !flag)
				{
					VpnExpandedWindow.GeneralControl.GeneralSettingsControl.AdvancedSettingsControl.KillSwitchToggle.Toggled1 = false;
				}
				VpnCredentials cred = AuthenticationFlow.GetCredentials();
				VpnExpandedWindow?.SetTrademarkLogo();
				ApplyNextAiVpnLoginResult(await AuthenticationFlow.LoginToNextAiVpnAsync(cred.VpnUsername, cred.VpnPassword));
				if (!(await (VpnEntities?.VpnModesValidator?.Validate(showStreamingMessageBox: false))).Contains(VpnType.Streaming))
				{
					await ApplyStreamingLoginResult(await AuthenticationFlow.LoginToStreamingAsync(cred.VpnUsername, cred.VpnPassword));
				}
				else
				{
					HideStreamingMode();
				}
				if (_isSteamingModeAvailable || IsNextAiVpnModeAvailable)
				{
					_logger.Information("User: " + cred.VpnUsername + " - logged in", "LoginToVpn", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1853);
				}
				_analyticsService.RefreshData();
				await _preferencesRepository.LoadUserPreferencesToConfig(_appSettingsHelper.GetValue("nickname"), isSignIn: true);
				await ShowNewNotificationWarningsAsync();
				if (_noNetworkNextAiVpn != null && _noNetworkNextAiVpn.IsVisible)
				{
					_noNetworkNextAiVpn.SetClosingState(state: false);
					_noNetworkNextAiVpn.Close();
					VpnExpandedWindow?.Mainpanel.SetNetworkInfo();
					VpnExpandedWindow?.Show();
					VpnExpandedWindow?.ExpandedSideMenu?.SetMenuOption(SideMenuOption.Location);
					TaskBarService?.ContextMenuShow(string.Empty);
					_noNetworkNextAiVpn = null;
				}
				await SetLastConnectedIfNeeded();
				SetLastConnectedFlag();
				if (OSDetector.GetOSVersion() >= 10)
				{
					VpnExpandedWindow?.ExpandedLocations.SetVpnMode(GetVpnMode());
				}
				else
				{
					HideStreamingMode();
					await VpnExpandedWindow.PreferencesService.RestoreAsync();
				}
				_appSettingsHelper.SetValue("IsLoggedIn", "1");
				EnableDisableExpandedViews(Visibility.Collapsed);
				EnableDisableSettingsOnLogin(isEnabled: true);
				EnableButtonsAndConnect();
				if (await InternetConnection.IsAvailableAsync())
				{
					VpnExpandedWindow?.SetConnectedNetwork();
				}
				if (NextAiVpnSdkManager != null && IsNextAiVpnModeAvailable && VpnExpandedWindow != null)
				{
					await VpnExpandedWindow.ExpandedLocations.AllLocationsControl.PingLocations();
				}
			}
			catch (Exception ex)
			{
				_logger?.Warning("[LoginToVpn] Exception bypassed: " + ex.Message);
			}
			finally
			{
				EnableDisableExpandedViews(Visibility.Collapsed);
				EnableDisableSettingsOnLogin(isEnabled: true);
				EnableButtonsAndConnect();
			}
		}
		else
		{
			_logger.Error("SDKManager object is NULL", "LoginToVpn", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 1919);
			_bugsnagService.Notify("SDKManager object is NULL");
			EnableDisableExpandedViews(Visibility.Collapsed);
			EnableDisableSettingsOnLogin(isEnabled: true);
			EnableButtonsAndConnect();
		}
	}

	private void ApplyNextAiVpnLoginResult(LoginAttemptResult result)
	{
		switch (result.Status)
		{
		case LoginAttemptStatus.Success:
		{
			try
			{
				if (NextAiVpnSdkManager.Locations == null || NextAiVpnSdkManager.Locations.Count == 0)
				{
					var fallback = NextAiVPN.Services.NextAiLocationService.GetDefaultFallbackLocations();
					NextAiVPN.Services.NextAiLocationService.InjectLocationsIntoSdk(NextAiVpnSdkManager, fallback);
				}

				// [VI] Đồng bộ ngay lập tức và duy trì tiến trình cập nhật danh sách proxy từ Backend CMS định kỳ 20 giây
				// [EN] Sync immediately and maintain background task refreshing proxy locations from CMS every 20s
				_ = Task.Run(async () =>
				{
					while (true)
					{
						try
						{
							var cmsLocs = await NextAiVPN.Services.NextAiLocationService.LoadLocationsAsync();
							if (cmsLocs != null && cmsLocs.Count > 0)
							{
								NextAiVPN.Services.NextAiLocationService.InjectLocationsIntoSdk(NextAiVpnSdkManager, cmsLocs);
								Application.Current?.Dispatcher?.Invoke(() =>
								{
									VpnExpandedWindow?.ExpandedLocations?.AllLocationsControl?.GetSdk(this);
								});
							}
						}
						catch { }
						await Task.Delay(TimeSpan.FromSeconds(20));
					}
				});
			}
			catch (Exception ex)
			{
				_logger?.Warning("[ApplyNextAiVpnLoginResult] Error injecting CMS locations: " + ex.Message);
			}

			VpnExpandedWindow?.ExpandedLocations?.AllLocationsControl?.GetSdk(this);
			VpnExpandedWindow?.ExpandedLocations?.SetFavoriteLocationTabHeaderText();
			string lastConnectedId = _appSettingsHelper.GetValue("LastConnectedId");
			NextAiVpnLocation = ((!string.IsNullOrEmpty(lastConnectedId) && NextAiVpnSdkManager.Locations != null) ? NextAiVpnSdkManager.Locations.FirstOrDefault((ILocation x) => x.Id.Equals(lastConnectedId)) : NextAiVpnSdkManager.Locations?.FirstOrDefault((ILocation x) => x.Id.Equals("bestavailable")));
			if (NextAiVpnLocation == null && NextAiVpnSdkManager.Locations != null && NextAiVpnSdkManager.Locations.Count > 0)
			{
				NextAiVpnLocation = NextAiVpnSdkManager.Locations[0];
			}
			if (NextAiVpnLocation != null)
			{
				SetLocation(NextAiVpnLocation);
			}
			IsNextAiVpnModeAvailable = true;
			break;
		}
		case LoginAttemptStatus.ModeUnavailable:
			SetModeUnavailable(result.ErrorMessage);
			break;
		case LoginAttemptStatus.NoNetwork:
			SetModeUnavailable(result.ErrorMessage);
			LoginToVpn();
			if (_noNetworkNextAiVpn == null)
			{
				_noNetworkNextAiVpn = new NoNetworkSimple();
				_noNetworkNextAiVpn.Show();
				TaskBarService?.ContextMenuHide("Quit");
				VpnExpandedWindow?.Hide();
			}
			break;
		case LoginAttemptStatus.Failed:
			SetModeUnavailable(result.ErrorMessage);
			LoginException($"{result.Exception}\n{result.ErrorMessage}", result.Exception);
			break;
		case LoginAttemptStatus.Skipped:
			break;
		}
	}

	private async Task ApplyStreamingLoginResult(LoginAttemptResult result)
	{
		switch (result.Status)
		{
		case LoginAttemptStatus.Success:
			try
			{
				await VpnExpandedWindow.ExpandedLocations.StreamingLocations.StreamingLocationsViewModel.SetLocations();
				_isSteamingModeAvailable = true;
				break;
			}
			catch (LocationListException ex)
			{
				SetModeUnavailable(ex.Message);
				break;
			}
			catch (Exception ex2)
			{
				SendNotificationToBugsnag(ex2.GetType().ToString(), ex2.Message);
				break;
			}
		case LoginAttemptStatus.ModeUnavailable:
			SetModeUnavailable(result.ErrorMessage);
			break;
		}
	}

	private bool IsRunOnStartUpAllowed()
	{
		bool isAdmin = false;
		Application.Current.Dispatcher.Invoke(delegate
		{
			isAdmin = VpnExpandedWindow.AdminPermissionsRunnerService.TryToRunOnStartUp();
		});
		return isAdmin;
	}

	public void ShowStreamingMode()
	{
		_isSteamingModeAvailable = true;
		_logger.Information("SHOW STREAMING MODE", "ShowStreamingMode", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 2015);
		VpnExpandedWindow.SettingsControlViewModel.AdvancedSettingsHeaderBorderVisibility = Visibility.Visible;
		VpnExpandedWindow.ExpandedLocations.SetVpnMode(VpnType.NextAiVPN);
		VpnExpandedWindow.ExpandedLocations.VpnModesBottomSeparator.Visibility = Visibility.Visible;
		VpnExpandedWindow.ExpandedLocations.StreamingModeStackPanel.Visibility = Visibility.Visible;
		TaskBarService.ContextMenuShow(string.Empty);
	}

	public void HideStreamingMode()
	{
		_isSteamingModeAvailable = false;
		_logger.Information("HIDE STREAMING MODE", "HideStreamingMode", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 2026);
		VpnExpandedWindow.SettingsControlViewModel.AdvancedSettingsHeaderBorderVisibility = Visibility.Collapsed;
		VpnExpandedWindow.ExpandedLocations.SetVpnMode(VpnType.NextAiVPN);
		VpnExpandedWindow.ExpandedLocations.VpnModesBottomSeparator.Visibility = Visibility.Collapsed;
		VpnExpandedWindow.ExpandedLocations.StreamingModeStackPanel.Visibility = Visibility.Collapsed;
		VpnExpandedWindow.ExpandedLocations.StreamingInfoItem.Visibility = Visibility.Collapsed;
		TaskBarService.ContextMenuShow("For Streaming", string.Empty);
	}

	private void SetModeUnavailable(string errorMessage)
	{
		NextAiVpnModeErrorMessage = errorMessage;
		EnableDisableExpandedViews(Visibility.Collapsed);
		EnableDisableSettingsOnLogin(isEnabled: true);
	}

	private void LoginException(string loginErrorStr, Exception exception)
	{
		ShowLoginError(loginErrorStr);
		_logger.Error(exception, "LoginException", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 2048);
		SendNotificationToBugsnag(exception.GetType().ToString(), exception.Message);
	}

	private async Task ShowNewNotificationWarningsAsync()
	{
		await TaskBarService.ShowNewNotificationWarningAsync();
		VpnExpandedWindow.ExpandedSideMenu.ShowNewNotificationWarning();
	}

	public void VerifyTypeDriver()
	{
		// [VI] Bỏ qua kiểm tra TAP Driver gây khóa UI / [EN] Bypass blocking TAP driver modal
		_logger?.Information("VerifyTypeDriver called - TAP driver check bypassed for WireGuard/Gateway.");
	}

	public void ShowLoginError(string errorMessage)
	{
		VpnExpandedWindow.Mainpanel.Visibility = Visibility.Collapsed;
		VpnExpandedWindow.LoginError.Visibility = Visibility.Visible;
		VpnExpandedWindow.ExpandedLocations.LoadingState.Visibility = Visibility.Visible;
		VpnExpandedWindow.LoginError.ErrorMessage = errorMessage;
	}

	public void HideLoginError()
	{
		VpnExpandedWindow.Mainpanel.Visibility = Visibility.Visible;
		VpnExpandedWindow.LoginError.Visibility = Visibility.Collapsed;
		VpnExpandedWindow.ExpandedLocations.LoadingState.Visibility = Visibility.Collapsed;
	}

	private void SendNotificationToBugsnag(string type, string message)
	{
		_bugsnagService.Notify(string.Format(CultureInfo.InvariantCulture, "[LoginToVpn] - {0} - {1}", type, message));
	}

	public async Task SetLastConnectedIfNeeded()
	{
		try
		{
			await Task.Run(delegate
			{
				if (string.IsNullOrEmpty(_appSettingsHelper.GetValue("LastConnectedId")))
				{
					_apiClient.SaveConnectedTo(string.Empty);
					_apiClient.SaveLastConnected("bestavailable");
					_apiClient.SaveLastConnectedId("bestavailable");
					try
					{
						VpnExpandedWindow.SdkObject.NextAiVpnLocation = VpnExpandedWindow.SdkObject.NextAiVpnSdkManager.Locations.First((ILocation x) => x.Id.Equals("bestavailable"));
					}
					catch (Exception)
					{
						VpnExpandedWindow.SdkObject.Country = "Best Available";
					}
				}
			});
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "SetLastConnectedIfNeeded", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 2115);
		}
	}

	public void SendMixpanelConnectedEvent(string location)
	{
		try
		{
			_analyticsService.SendCustomConnectNotification("Connected", location);
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "SendMixpanelConnectedEvent", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 2127);
		}
	}

	public void SetLastConnectedFlag()
	{
		try
		{
			System.Windows.Media.ImageSource flag = Converters.FlagConverterHelper.GetFlagByCountryCode(NextAiVpnLocation?.CountryCode);
			if (flag is BitmapImage bmp)
			{
				LastConnectedFlag = bmp;
			}
			else
			{
				string uriString = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Light) ? "/Resources/Flags/bestavailable.png" : "/Resources/Flags/bestavailable_darkMode.png");
				try { LastConnectedFlag = new BitmapImage(new Uri(uriString, UriKind.Relative)); } catch { }
			}

			if (VpnExpandedWindow?.Mainpanel?.locationImage != null && flag != null)
			{
				VpnExpandedWindow.Mainpanel.locationImage.Source = flag;
			}
		}
		catch (Exception ex)
		{
			_logger?.Error("Error setting last connected flag: " + ex.Message, "SetLastConnectedFlag", "SDKMonitor.cs", 2156);
		}
	}

	public async Task RefreshLocationsIfNeeded()
	{
		try
		{
			if (ShouldReloadLocations() && NetworkInterface.GetIsNetworkAvailable())
			{
				LocationsRefreshing = true;
				await NextAiVpnSdkManager.RefreshServerInfo();
				EnableButtonsAndConnect();
				SaveLastLocationsLoaded();
			}
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "RefreshLocationsIfNeeded", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 2177);
		}
	}

	private void SaveLastLocationsLoaded()
	{
		_appSettingsHelper.SetValue("LastLoadedLocations", DateTime.Now.ToString());
	}

	private bool ShouldReloadLocations()
	{
		string value = _appSettingsHelper.GetValue("LastLoadedLocations");
		if (string.IsNullOrEmpty(value))
		{
			SaveLastLocationsLoaded();
			return false;
		}
		DateTime value2 = Convert.ToDateTime(value, CultureInfo.CurrentCulture.DateTimeFormat);
		return DateTime.Now.Subtract(value2).TotalHours >= 24.0;
	}

	public void EnableButtonsAndConnect()
	{
		try
		{
			btnLocationsVisible = Visibility.Visible;
			txtLocationsLoading = Visibility.Collapsed;
			if (VpnExpandedWindow != null)
			{
				VpnExpandedWindow.Mainpanel.AnimateLoadingSpinner(animate: false);
				VpnExpandedWindow.ExpandedSideMenu.LocationsSideMenu.IsEnabled = true;
			}
			ButtonEnable = true;
			if (NextAiVpnSdkManager.IsConnected)
			{
				VpnExpandedWindow.Mainpanel.BtnConnectColor(connected: true);
			}
			if (!string.IsNullOrEmpty(_appSettingsHelper.GetValue("ConnectedTo")) && _appSettingsHelper.GetValue("AutoConnect").Equals("1") && !LocationsRefreshing && GetVpnMode() == VpnType.NextAiVPN)
			{
				ConnectToVPN(NextAiVpnLocation);
			}
			LocationsRefreshing = false;
		}
		catch (Exception)
		{
			ButtonEnable = true;
		}
	}

	public void SetProtocol(int protocol, string type)
	{
		switch (ConnectedVpnMode)
		{
		case VpnType.NextAiVPN:
			switch (protocol)
			{
			case 1:
				ConnectionType = NetworkConnectionType.IKEv2;
				SelectedProtocol = "IKEv2";
				break;
			case 2:
				ConnectionType = NetworkConnectionType.OpenVPN;
				SelectedProtocol = "OpenVPN";
				if (!(type == "udp"))
				{
					if (type == "tcp")
					{
						SelectedOpenVpnProtocol = NetworkProtocolType.TCP;
					}
				}
				else
				{
					SelectedOpenVpnProtocol = NetworkProtocolType.UDP;
				}
				break;
			case 3:
				ConnectionType = NetworkConnectionType.WireGuard;
				SelectedProtocol = "WireGuard®";
				break;
			}
			break;
		case VpnType.Streaming:
			ConnectionType = NetworkConnectionType.IKEv2;
			SelectedProtocol = "IKEv2";
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SetScramble(bool flag)
	{
		OpenVpnScramble = flag;
	}

	public void SetLocation(ILocation loc)
	{
		if (loc == null)
		{
			return;
		}
		try
		{
			NextAiVpnLocation = loc;
			if (loc.Id == "bestavailable")
			{
				Country = "Best Available";
				City = string.Empty;
				_ = Task.Run(() =>
				{
					try
					{
						_apiClient.SaveLastConnected("bestavailable_darkMode");
						_apiClient.SaveLastConnectedId(loc.Id);
						_apiClient.SaveConnectedTo("bestavailable_darkMode");
						_apiClient.SetIsBestAvailable("1");
					}
					catch { }
				});
			}
			else
			{
				Country = ((IRegion)loc).Country;
				City = ((IRegion)loc).City;
				_ = Task.Run(() =>
				{
					try
					{
						_apiClient.SaveLastConnected(loc.CountryCode.ToLower());
						_apiClient.SaveConnectedTo(loc.Id);
						_apiClient.SaveLastConnectedId(loc.Id);
						_apiClient.SetIsBestAvailable("0");
					}
					catch { }
				});
			}
			SetLastConnectedFlag();
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "SetLocation", "SDKMonitor.cs", 2312);
		}
	}

	public void SetStreamingLocation(IStreamingLocation loc)
	{
		if (loc == null)
		{
			StreamingLocation = StreamingSdk.Locations.FirstOrDefault();
			if (StreamingLocation != null)
			{
				Country = StreamingLocation.Country;
				City = StreamingLocation.City;
				SetLastConnectedFlag();
			}
			return;
		}
		try
		{
			StreamingLocation = loc;
			Country = loc.Country;
			City = loc.City;
			SetLastConnectedFlag();
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "SetStreamingLocation", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 2339);
		}
	}

	public void SaveDataUsageOnDisconnect()
	{
		SessionStatsTracker.SaveDataUsageOnDisconnect();
	}

	public async Task DisconnectVPN()
	{
		await ConnectionOrchestrator.DisconnectAsync(ConnectedVpnMode);
	}

	internal async Task DisconnectFromStreaming()
	{
		try
		{
			EnableDisableExpandedViews(Visibility.Visible);
			ManualDisconnect = true;
			await StreamingSdk.Disconnect();
			EnableDisableExpandedViews(Visibility.Collapsed);
		}
		catch (Exception ex)
		{
			_logger.Error("method: DisconnectFromStreaming - Error: \n" + ex.Message, "DisconnectFromStreaming", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 2367);
		}
	}

	internal async Task DisconnectFromGateway()
	{
		try
		{
			SaveDataUsageOnDisconnect();
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "DisconnectFromGateway", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 2379);
		}
		ManualDisconnect = true;
		try
		{
			OnDisconnectAction?.Invoke();
			NextAiVPN.Services.NextAiLocationService.DisableSystemProxy();
			NextAiVPN.Services.NextAiLocationService.SetActiveConnection(NextAiVpnSdkManager, null, false);
			await NextAiVpnSdkManager.Disconnect();
		}
		catch (Exception ex)
		{
			_logger.Error("method: DisconnectFromGateway - Error: \n" + ex.Message, "DisconnectFromGateway", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\SDKMonitor.cs", 2391);
		}
		finally
		{
			await DisconnectedCommon();
			if (VpnExpandedWindow != null)
			{
				VpnExpandedWindow.Mainpanel.BtnConnectColor(connected: false);
			}
		}
	}

	private void OnSessionStatsTicked(object sender, EventArgs e)
	{
		OnPropertyChanged("TimeElapsed");
		if (!IsReconnecting || ReconnectingStatus != Visibility.Visible)
		{
			return;
		}
		_sessionStatsTracker.ResetClock();
		if (_reconnectCounter == 2 && ConnectionType == NetworkConnectionType.OpenVPN)
		{
			_reconnectCounter = _noNetworkTimeOut + 1;
		}
		if (_reconnectCounter < _noNetworkTimeOut)
		{
			if (VpnExpandedWindow != null)
			{
				VpnExpandedWindow.Mainpanel.SetConnectedStatusScreen("reconnecting");
				VpnExpandedWindow.Mainpanel.BtnConnectColor(connected: false);
			}
			Spinner = Visibility.Visible;
			_sessionStatsTracker.ResetClock();
		}
	}

	private void OnSessionUsageUpdated(object sender, EventArgs e)
	{
		OnPropertyChanged("NetworkUsageDownload");
		OnPropertyChanged("networkUsageDownloadMB");
		OnPropertyChanged("networkUsageUpload");
		OnPropertyChanged("networkUsageUploadMB");
		if (VpnExpandedWindow != null)
		{
			VpnExpandedWindow.Dispatcher.Invoke(() =>
			{
				if (VpnExpandedWindow.Mainpanel?.ConnectionDataViewModel != null)
				{
					VpnExpandedWindow.Mainpanel.ConnectionDataViewModel.NetworkUsageDownloadMb = _sessionStatsTracker.DownloadedUsageText;
					VpnExpandedWindow.Mainpanel.ConnectionDataViewModel.NetworkUsageUploadMb = _sessionStatsTracker.UploadedUsageText;
					if (State == ConnectionState.Connected)
					{
						VpnExpandedWindow.Mainpanel.ConnectionDataViewModel.ControlVisibility = Visibility.Visible;
					}
				}
				SetDeleteLogsImage();
			});
		}
	}

	private void SetDeleteLogsImage()
	{
		if (VpnExpandedWindow.AdvanceViewLogs.HasLogs() && !VpnExpandedWindow.AdvanceViewLogs.deleteLogsImage.Source.ToString().Contains("trashLogs.png"))
		{
			VpnExpandedWindow.AdvanceViewLogs.deleteLogsImage.Source = new BitmapImage(new Uri(IconHelper.GetIcon("trashLogs"), UriKind.Relative));
		}
	}

	public void SetClearLogs()
	{
		SessionStatsTracker.ResetUsage(NextAiVpnSdkManager.IsConnected);
	}

	public bool CheckTapDriverInstalled()
	{
		return true;
	}

	public void AddBlockerToggle(bool isEnabled)
	{
		if (isEnabled)
		{
			EnableAdBlocker();
		}
		else
		{
			DisableAdBlocker();
		}
	}

	private void EnableAdBlocker()
	{
		_appSettingsHelper.SetValue("IsAdBlocker", "1");
		NextAiVpnSdkManager.DnsFilterMode = (DnsFilteringMode)1; // [VI] NextAiVPN DNS
	}

	private void DisableAdBlocker()
	{
		_appSettingsHelper.SetValue("IsAdBlocker", "0");
		NextAiVpnSdkManager.DnsFilterMode = DnsFilteringMode.Disabled;
	}
}
