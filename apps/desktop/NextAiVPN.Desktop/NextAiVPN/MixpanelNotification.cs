using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Services;

namespace NextAiVPN;

public class MixpanelNotification : IAnalyticsService
{
	private static readonly HttpClient _httpClient = new HttpClient();

	private const string Url = "https://api.mixpanel.com/track/?data=";

	private string Token = "006820e9b2f2792c5382cac96bf53bba";

	private readonly string _os;

	private readonly string _machineUniqueIdentifier = SystemInfo.MachineId;

	private readonly string _version = SystemInfo.AppVersion;

	private string _userName;

	private string _subscriptionId;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private string EventDate => DateTime.Now.ToUniversalTime().ToString();

	public MixpanelNotification(IOSVersionService osVersionService, IAppSettingsHelper appSettingsHelper, IAppLogger logger)
	{
		_os = osVersionService.GetOSVersion();
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
	}

	public void RefreshData()
	{
		_userName = _appSettingsHelper.GetValue("nickname");
		_subscriptionId = _appSettingsHelper.GetValue("SubscriptionId");
	}

	public async Task SendNotification(string eventName)
	{
		string appEvent = "{\"event\":\"" + eventName + "\",\"properties\": {\"token\": \"" + Token + "\", \"username\": \"" + _userName + "\", \"AppVersion\": \"" + _version + "\", \"EventDate\": \"" + EventDate + "\", \"SubscriptionId\": \"" + _subscriptionId + "\", \"MachineIdentifier\": \"" + _machineUniqueIdentifier + "\", \"OS\": \"" + _os + "\" }}";
		await SendRequestAsync(appEvent);
	}

	public async Task SendCustomConnectNotification(string eventName, string locationCity)
	{
		string appEvent = "{\"event\":\"" + eventName + "\",\"properties\": {\"token\": \"" + Token + "\", \"username\": \"" + _userName + "\", \"AppVersion\": \"" + _version + "\", \"EventDate\": \"" + EventDate + "\", \"protocol\": \"" + GetProtocolSelected() + "\", \"LocationCity\": \"" + locationCity + "\", \"SubscriptionId\": \"" + _subscriptionId + "\", \"MachineIdentifier\": \"" + _machineUniqueIdentifier + "\", \"OS\": \"" + _os + "\" }}";
		await SendRequestAsync(appEvent);
	}

	public async Task SendConnectNotification(string eventName)
	{
		string appEvent = "{\"event\":\"" + eventName + "\",\"properties\": {\"token\": \"" + Token + "\", \"AppVersion\": \"" + _version + "\", \"EventDate\": \"" + EventDate + "\", \"protocol\": \"" + GetProtocolSelected() + "\", \"username\": \"" + _userName + "\", \"SubscriptionId\": \"" + _subscriptionId + "\", \"MachineIdentifier\": \"" + _machineUniqueIdentifier + "\", \"OS\": \"" + _os + "\" }}";
		await SendRequestAsync(appEvent);
	}

	private async Task SendRequestAsync(string appEvent)
	{
		_ = 1;
		try
		{
			if (NetworkInterface.GetIsNetworkAvailable())
			{
				byte[] bytes = Encoding.UTF8.GetBytes(Convert.ToString(appEvent));
				string requestUri = "https://api.mixpanel.com/track/?data=" + Convert.ToBase64String(bytes);
				string text;
				using (HttpResponseMessage response = await _httpClient.PostAsync(requestUri, null))
				{
					text = await response.Content.ReadAsStringAsync();
				}
				if (!string.IsNullOrEmpty(text) && !text.Equals("1"))
				{
					LogError("[MixpanelNotification] - Unable to send information to Mixpanel");
				}
			}
		}
		catch (Exception ex)
		{
			LogError("[MixpanelNotification] - " + ex.Message);
		}
	}

	private void LogError(string message)
	{
		_logger?.Error(message, "LogError", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\MixpanelNotification.cs", 142);
	}

	private string GetProtocolSelected()
	{
		string value = _appSettingsHelper.GetValue("protocol");
		switch (value.ToLower())
		{
		case "ikev2":
		case "wireguard":
			return value;
		case "openvpn":
			if (!_appSettingsHelper.GetValue("protocolType").ToLower().Equals("tcp"))
			{
				return "OpenVPN - UDP";
			}
			return "OpenVPN - TCP";
		default:
			return value;
		}
	}
}
