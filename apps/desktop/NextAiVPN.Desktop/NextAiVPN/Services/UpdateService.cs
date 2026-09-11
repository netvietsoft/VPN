using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NextAiVPN.Common;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

internal class UpdateService : IUpdateService
{
	private static readonly HttpClient _httpClient = new HttpClient
	{
		Timeout = Timeout.InfiniteTimeSpan
	};

	private readonly ICredentialsSaver _credentialsSaver;

	private readonly IMsiFileCleaner _msiFileCleaner;

	private readonly IBugsnagService _bugsnagService;

	private readonly IAppLogger _logger;

	private readonly IApiClient _apiClient;

	private readonly DispatcherTimer _timer;

	private CancellationTokenSource _downloadCts;

	private bool _isUpdateFailed;

	private const string InstallerName = "NextAiVPN-Install.msi";

	public event Action<string> DownloadFailed;

	public UpdateService(ICredentialsSaver credentialsSaver, IMsiFileCleaner msiFileCleaner, IBugsnagService bugsnagService, IAppLogger logger, IApiClient apiClient)
	{
		_credentialsSaver = credentialsSaver;
		_msiFileCleaner = msiFileCleaner;
		_bugsnagService = bugsnagService;
		_logger = logger;
		_apiClient = apiClient;
		_timer = new DispatcherTimer();
		_timer.Tick += _timer_Tick;
		_timer.Interval = TimeSpan.FromSeconds(5L);
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		if (!InternetConnection.IsAvailable())
		{
			DownloadingFailed("Please, check your internet connection and try to update app again.");
		}
	}

	private void DownloadingFailed(string message)
	{
		_isUpdateFailed = true;
		_timer.Stop();
		DownloadFailed?.Invoke(message);
		_downloadCts?.Cancel();
	}

	public void Update()
	{
		_isUpdateFailed = false;
		_downloadCts?.Cancel();
		_downloadCts = new CancellationTokenSource();
		string nextAiVpnVersionLink = _apiClient.GetNextAiVpnVersionLink();
		if (nextAiVpnVersionLink != "error")
		{
			_credentialsSaver.Save();
			_msiFileCleaner.CleanUp();
			_timer.Start();
			DownloadInstallerAsync(nextAiVpnVersionLink, _downloadCts.Token);
		}
	}

	private async Task DownloadInstallerAsync(string nextAiVpnLink, CancellationToken cancellationToken)
	{
		_ = 2;
		try
		{
			using HttpResponseMessage response = await _httpClient.GetAsync(nextAiVpnLink, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			response.EnsureSuccessStatusCode();
			using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken);
			using FileStream target = new FileStream(VPNConstants.FilePath.NextAiVpnInstallFilePath, FileMode.Create);
			await source.CopyToAsync(target, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			return;
		}
		catch (Exception ex2)
		{
			_logger?.Error("Installer download failed - " + ex2.Message, "DownloadInstallerAsync", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\UpdateService.cs", 122);
			if (!_isUpdateFailed)
			{
				DownloadingFailed("Please, check your internet connection and try to update app again.");
			}
			return;
		}
		_timer.Stop();
		StartUpdate();
	}

	public async Task CheckForUpdate(Border updateControl)
	{
		string text = await _apiClient.GetLatestVersionNumber();
		if (text != "Error" && !string.IsNullOrEmpty(text))
		{
			updateControl.Visibility = Visibility.Visible;
		}
		else
		{
			updateControl.Visibility = Visibility.Collapsed;
		}
	}

	public async Task<bool> IsUpdateAvailable()
	{
		string text = await _apiClient.GetLatestVersionNumber();
		return text != "Error" && !string.IsNullOrEmpty(text);
	}

	private void StartUpdate()
	{
		if (_isUpdateFailed)
		{
			return;
		}
		try
		{
			FileInfo fileInfo = new FileInfo(VPNConstants.FilePath.NextAiVpnInstallFilePath);
			if (fileInfo.Exists)
			{
				if (!InternetConnection.IsAvailable() || fileInfo.Length < 11000000)
				{
					DownloadingFailed("Please, check your internet connection and try to update app again.");
					return;
				}
				_logger?.Information("Start updating from version: " + SystemInfo.AppVersion, "StartUpdate", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\UpdateService.cs", 181);
				Process process = new Process();
				process.StartInfo.FileName = "msiexec";
				process.StartInfo.WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SystemInfo.AppName);
				process.StartInfo.Arguments = " /norestart /i NextAiVPN-Install.msi";
				process.Start();
				Process.GetCurrentProcess().Kill();
			}
		}
		catch (Exception ex)
		{
			DownloadingFailed("There has been an error launching the update installer, please try again.");
			_bugsnagService.Notify(string.Format(CultureInfo.InvariantCulture, "[ExecuteInstaller - Update] - There was an error executing the installer the file NextAiVPN-Install.msi.AppVersion : {0} - Error: {1}", SystemInfo.AppVersion, ex.Message));
		}
	}
}
