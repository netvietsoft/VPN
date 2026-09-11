using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;
using RestSharp;

namespace NextAiVPN.Services;

internal class NotificationService : INotificationService
{
	private static readonly HttpClient _ipAddressClient = new HttpClient();

	private static readonly Uri _ipAddressEndpoint = new Uri("https://api.ipify.org");

	private readonly SDKMonitor _sdkMonitor;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IBugsnagService _bugsnagService;

	private readonly IAppLogger _logger;

	private readonly IApiClient _apiClient;

	private readonly RestClient _restClient;

	public NotificationService(IAppSettingsHelper appSettingsHelper, SDKMonitor sdkMonitor, IBugsnagService bugsnagService, IAppLogger logger, IApiClient apiClient)
	{
		_appSettingsHelper = appSettingsHelper;
		_sdkMonitor = sdkMonitor;
		_bugsnagService = bugsnagService;
		_logger = logger;
		_apiClient = apiClient;
		_restClient = RestClientFactory.Create(new RestClientOptions());
	}

	public async Task<ObservableCollection<Notification>> GetNotificationsAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		List<Notification> list = (await GetListFromServerAsync(null, cancellationToken)).OrderByDescending((Notification d) => d.Date).ToList();
		SaveNewNotificationsCount(list.Count);
		return new ObservableCollection<Notification>(list);
	}

	public void SaveSortSelected(string sortValue)
	{
		_appSettingsHelper.SetValue("sort", sortValue);
	}

	public void SaveNewNotificationsCount(int count)
	{
		string value = string.Empty;
		switch (count)
		{
		case 0:
			value = "0 New notifications";
			break;
		case 1:
			value = "1 New notification";
			break;
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
		case 31:
		case 32:
		case 33:
		case 34:
		case 35:
		case 36:
		case 37:
		case 38:
		case 39:
		case 40:
		case 41:
		case 42:
		case 43:
		case 44:
		case 45:
		case 46:
		case 47:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
		case 58:
		case 59:
		case 60:
		case 61:
		case 62:
		case 63:
		case 64:
		case 65:
		case 66:
		case 67:
		case 68:
		case 69:
		case 70:
		case 71:
		case 72:
		case 73:
		case 74:
		case 75:
		case 76:
		case 77:
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 83:
		case 84:
		case 85:
		case 86:
		case 87:
		case 88:
		case 89:
		case 90:
		case 91:
		case 92:
		case 93:
		case 94:
		case 95:
		case 96:
		case 97:
		case 98:
		case 99:
			value = count + " New notifications";
			break;
		default:
			if (count > 99)
			{
				value = "+99 New notifications";
			}
			break;
		}
		_appSettingsHelper.SetValue("NewNotificationsCount", value);
	}

	public async Task<bool> HasNewNotificationAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		string lastId = _appSettingsHelper.GetValue("LastNotificationId");
		bool result = (await GetListFromServerAsync((!string.IsNullOrEmpty(lastId)) ? lastId : string.Empty, cancellationToken)).Count > 0;
		_appSettingsHelper.SetValue("LastNotificationId", lastId);
		return result;
	}

	private async Task<List<Notification>> GetListFromServerAsync(string lastNotificationId = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		List<Notification> notifications = new List<Notification>();
		try
		{
			string userAgent = SystemInfo.UserAgent;
			string accessToken = _appSettingsHelper.GetValue("access_token");
			string refreshToken = _appSettingsHelper.GetValue("refresh_token");
			string expiresAt = _appSettingsHelper.GetValue("expires_at");
			string username = _appSettingsHelper.GetValue("nickname");
			string baseUrl = ApiEndpoints.BaseEndPoint;
			string value = await GetUserIpAsync(cancellationToken);
			string body = "{\"platform\":\"windows\",\"backrealtype\":\"true\", \"version\":\"" + SystemInfo.AppVersion + "\"" + lastNotificationId + "}";
			RestRequest request = new RestRequest(baseUrl + "/api/v1/notification", Method.Post).AddHeader("User-Agent", userAgent);
			switch (_sdkMonitor.AccountTypeHelper.GetAccountType())
			{
			case AccountType.NextAiTechnology:
				request.AddHeader("X-nc-User", username);
				request.AddHeader("X-NAMP-Token", accessToken);
				request.AddHeader("X-NAMP-Refresh-Token", refreshToken);
				request.AddHeader("X-NAMP-Expires-At", expiresAt);
				request.AddHeader("Accept", "application/json");
				request.AddHeader("Content-Type", "application/json");
				request.AddHeader("Accept-Language", RequestContext.BuildAcceptLanguage());
				request.AddHeader("Host", baseUrl.Replace("https://", ""));
				request.AddHeader("x-real-ip", value);
				request.AddHeader("x-forwarded-for", value);
				request.AddHeader("accept-encoding", "gzip, deflate, br");
				break;
			case AccountType.NextAiGlobal:
				request.AddHeader("X-SPS-Token", _appSettingsHelper.GetValue("id_token"));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case AccountType.None:
				break;
			}
			request.AddStringBody(body, ContentType.Json);
			RestResponse<NotificationApiResponse> restResponse = await _restClient.ExecuteAsync<NotificationApiResponse>(request, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			if (_sdkMonitor.AccountTypeHelper.GetAccountType() == AccountType.NextAiTechnology)
			{
				_apiClient.RefreshTokensIfNeeded(restResponse);
			}
			NotificationApiResponse data = restResponse.Data;
			if (data != null && data.Data != null)
			{
				notifications.AddRange(data.Data.Select((NotificationData notification) => new Notification
				{
					Id = notification.Id,
					Title = notification.Title,
					Description = notification.Description,
					Date = notification.Date,
					IsRead = false,
					TitleFont = new FontFamily("/Fonts/#Museo Sans 700"),
					TitleFontColor = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#2A2A2C"),
					Type = notification.Type.ToString(),
					GoToAccountCTAVisibility = ((!notification.Type.ToString().Equals("2")) ? Visibility.Collapsed : Visibility.Visible)
				}));
			}
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "GetListFromServerAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\NotificationService.cs", 179);
		}
		try
		{
			SetNotificationsFetchedDateAndId(notifications.Max((Notification x) => int.Parse(x.Id)).ToString());
		}
		catch (Exception)
		{
			SetNotificationsFetchedDateAndId(string.Empty);
		}
		return notifications;
	}

	private void SetNotificationsFetchedDateAndId(string lastId)
	{
		_appSettingsHelper.SetValue("NotificationsFetched", DateTime.Now.ToShortDateString());
		_appSettingsHelper.SetValue("LastNotificationId", lastId);
	}

	private async Task<string> GetUserIpAsync(CancellationToken cancellationToken)
	{
		try
		{
			return (_sdkMonitor == null) ? (await GetIp3RdPartyAsync(cancellationToken)) : _sdkMonitor.IPAddress;
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex2)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "[Error] - Path: {0}.{1}() - {2}", "NotificationService", "GetUserIpAsync", ex2.Message);
			_logger?.Error(message, "GetUserIpAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\NotificationService.cs", 217);
			_bugsnagService.Notify(message);
			return await GetIp3RdPartyAsync(cancellationToken);
		}
	}

	private static Task<string> GetIp3RdPartyAsync(CancellationToken cancellationToken)
	{
		return _ipAddressClient.GetStringAsync(_ipAddressEndpoint, cancellationToken);
	}

	private static void AddTestNotification(IList<Notification> notificationList)
	{
		notificationList.Add(new Notification
		{
			Title = "Gt To Account",
			Description = "Secure your internet connection. Renew in Account now.",
			Date = DateTime.Today,
			GoToAccountCTAVisibility = Visibility.Visible
		});
		notificationList.Add(new Notification
		{
			Title = "YESTERDAY",
			Description = "gvhabhdsasd \ngvhabhdsasd \ngvhabhdsasd \ngvhabhdsasd \ngvhabhdsasd \ngvhabhdsasd \n ",
			Date = DateTime.Today.AddDays(-1.0)
		});
		notificationList.Add(new Notification
		{
			Title = "type 2",
			Description = "desc",
			Date = DateTime.Today,
			Type = "2",
			GoToAccountCTAVisibility = Visibility.Visible
		});
		notificationList.Add(new Notification
		{
			Header = "1 Header read",
			Description = "1 Description\n \ud83c\udf81 present\n ♥ \ud83c\udf34",
			TitleFontColor = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#000000"),
			Date = DateTime.Now,
			IsRead = true,
			Title = "New version available \ud83c\udf52",
			UpdateCTAVisibility = Visibility.Visible,
			ImageSource = "/Assets/info.png",
			Id = "14fdd235"
		});
		notificationList.Add(new Notification
		{
			Description = "https://www.google.com Description https://www.figma.com and https://vpn.ncapi.io/version https://www.figsddma.com.ua/sj",
			TitleFontColor = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#A3A3A3"),
			Date = DateTime.Now,
			IsRead = false,
			Title = "2 Title",
			DotsMenuVisibility = Visibility.Visible,
			ImageSource = "/Resources/Flags/ua.png",
			Id = "12fsdfsdg35",
			Type = "2",
			GoToAccountCTAVisibility = Visibility.Visible
		});
	}
}
