using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using NextAiVPN.Properties;
using NextAiVPN.Services.Persistence;
using RestSharp;

namespace NextAiVPN.Services;

internal class FeedbackService : IFeedbackService
{
	private readonly IAccountTypeHelper _accountTypeHelper;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IPreferencesRepository _preferencesRepository;

	private readonly IApiClient _apiClient;

	private readonly IAppLogger _logger;

	private readonly string _feedbackUrl;

	private readonly string _version;

	private readonly string _uploadFeedbackLogFileUrl;

	private readonly string _userAgent;

	private readonly RestClient _feedbackClient;

	private readonly RestClient _uploadClient;

	private string _accessToken;

	private string _refreshToken;

	private string _expiresAt;

	private string _userName;

	public FeedbackService(IAccountTypeHelper accountTypeHelper, IAppSettingsHelper appSettingsHelper, IPreferencesRepository preferencesRepository, IApiClient apiClient, IAppLogger logger)
	{
		_accountTypeHelper = accountTypeHelper;
		_appSettingsHelper = appSettingsHelper;
		_preferencesRepository = preferencesRepository;
		_apiClient = apiClient;
		_logger = logger;
		_feedbackUrl = ApiEndpoints.FeedbackEndPoint;
		_version = SystemInfo.AppVersion;
		_uploadFeedbackLogFileUrl = ApiEndpoints.UploadFeedbackLogsFilesEndPoint;
		_userAgent = SystemInfo.UserAgent;
		_feedbackClient = RestClientFactory.Create(new RestClientOptions(_feedbackUrl)
		{
			Timeout = Timeout.InfiniteTimeSpan,
			UserAgent = _userAgent
		});
		_uploadClient = RestClientFactory.Create(new RestClientOptions(_uploadFeedbackLogFileUrl));
	}

	public async Task<string> SendFeedbackNotification(string score, string feedbackBody, bool needToSendDiagnosticFile)
	{
		try
		{
			return await SendFeedbackAsync(score, feedbackBody, needToSendDiagnosticFile);
		}
		catch (Exception ex)
		{
			return ex.Message.Contains("Cannot perform runtime binding on a null reference") ? "-1" : "-2";
		}
	}

	public void SaveLastFeedbackClosed(bool reset)
	{
		try
		{
			string value = (reset ? string.Empty : DateTime.Now.ToString());
			_appSettingsHelper.SetValue("LastFeedbackClosed", value);
			_preferencesRepository.SaveSinglePreference("LastFeedbackClosed".ToLower(), value);
		}
		catch (Exception ex)
		{
			_logger?.Error("Error saving Feedback Sent Time: " + ex.Message, "SaveLastFeedbackClosed", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\FeedbackService.cs", 89);
		}
	}

	public async Task<string> SendFeedbackAsync(string score, string feedbackBody, bool needToSendDiagnosticFile)
	{
		string text = string.Empty;
		GetDataBeforeRequest();
		if (needToSendDiagnosticFile)
		{
			string text2 = await SendDiagnosticDataAsync();
			text = (string.IsNullOrEmpty(text2) ? string.Empty : ("\", \"file\":\"" + text2 + "\""));
		}
		string text3 = (string.IsNullOrEmpty(text) ? "\"" : string.Empty);
		string value = _appSettingsHelper.GetValue("Email");
		feedbackBody = feedbackBody ?? string.Empty;
		string parameter = "{\"platform\":\"windows\", \"version\":\"" + _version + "\", \"score\":\"" + score + "\", \"body\":\"" + feedbackBody.Replace("\"", "'") + "\", \"email\":\"" + value + text + text3 + "}";
		RestRequest request = new RestRequest(string.Empty, Method.Post);
		switch (_accountTypeHelper.GetAccountType())
		{
		case AccountType.NextAiTechnology:
			request.AddHeader("Content-Type", "application/json");
			request.AddHeader("Authorization", "Bearer " + _accessToken);
			request.AddHeader("X-NAMP-Token", _accessToken);
			request.AddHeader("X-NAMP-Refresh-Token", _refreshToken);
			request.AddHeader("X-NAMP-Expires-At", _expiresAt);
			request.AddHeader("X-nc-User", _appSettingsHelper.GetValue("nickname"));
			break;
		case AccountType.NextAiGlobal:
			await _apiClient.RefreshNextAiGlobalTokensIfNeeded();
			request.AddHeader("X-SPS-Token", _appSettingsHelper.GetValue("id_token"));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case AccountType.None:
			break;
		}
		request.AddStringBody(parameter, ContentType.Json);
		RestResponse<FeedbackResponse> restResponse = await _feedbackClient.ExecuteAsync<FeedbackResponse>(request);
		if (_accountTypeHelper.GetAccountType() == AccountType.NextAiTechnology)
		{
			_apiClient.RefreshTokensIfNeeded(restResponse);
		}
		return restResponse.Data.Success;
	}

	private void GetDataBeforeRequest()
	{
		_userName = _appSettingsHelper.GetValue("nickname");
		_accessToken = _appSettingsHelper.GetValue("access_token");
		_refreshToken = _appSettingsHelper.GetValue("refresh_token");
		_expiresAt = _appSettingsHelper.GetValue("expires_at");
	}

	private async Task<string> SendDiagnosticDataAsync()
	{
		RestRequest request = new RestRequest(string.Empty, Method.Post)
		{
			AlwaysMultipartFormData = true
		};
		switch (_accountTypeHelper.GetAccountType())
		{
		case AccountType.NextAiTechnology:
			request.AddHeader("User-Agent", _userAgent);
			request.AddHeader("X-NC-User", _userName);
			request.AddHeader("X-NAMP-Token", _accessToken);
			request.AddHeader("X-NAMP-Refresh-Token", _refreshToken);
			request.AddHeader("X-NAMP-Expires-At", _expiresAt);
			break;
		case AccountType.NextAiGlobal:
			await _apiClient.RefreshNextAiGlobalTokensIfNeeded();
			request.AddHeader("X-SPS-Token", _appSettingsHelper.GetValue("id_token"));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case AccountType.None:
			break;
		}
		byte[] diagnosticDataAsBytes = GetDiagnosticDataAsBytes();
		request.AddFile(Resources.DiagnosticsFileName, diagnosticDataAsBytes, Resources.DiagnosticsFileName + ".txt", "application/blob");
		RestResponse<DiagnosticsFileUploadResponse> restResponse = await _uploadClient.ExecuteAsync<DiagnosticsFileUploadResponse>(request);
		if (_accountTypeHelper.GetAccountType() == AccountType.NextAiTechnology)
		{
			_apiClient.RefreshTokensIfNeeded(restResponse);
		}
		return restResponse.Data.FileId;
	}

	private static byte[] GetDiagnosticDataAsBytes()
	{
		string sourceFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Resources.DiagnosticFilePath);
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Resources.DiagnosticTempFilePath + ".txt");
		RemoveFileIfExist(text);
		File.Copy(sourceFileName, text);
		string[] array = File.ReadAllLines(text);
		IEnumerable<string> contents = array.Skip(Math.Max(0, array.Length - 100000));
		File.WriteAllLines(text, contents);
		string text2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Resources.DiagnosticTempFilePath + ".zip");
		RemoveFileIfExist(text2);
		ZipArchiveHelper.CreateZipArchive(text, text2, Resources.DiagnosticsFileName + ".txt");
		byte[] result = File.ReadAllBytes(text2);
		RemoveFileIfExist(text);
		RemoveFileIfExist(text2);
		return result;
	}

	private static void RemoveFileIfExist(string destinationFileName)
	{
		if (File.Exists(destinationFileName))
		{
			File.Delete(destinationFileName);
		}
	}
}
