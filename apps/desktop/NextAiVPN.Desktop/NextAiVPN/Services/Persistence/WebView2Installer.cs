using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class WebView2Installer : IWebView2Installer
{
	private static readonly HttpClient _httpClient = new HttpClient();

	private readonly IApiClient _apiClient;

	private readonly IAppLogger _logger;

	public WebView2Installer(IApiClient apiClient, IAppLogger logger)
	{
		_apiClient = apiClient;
		_logger = logger;
	}

	public async Task Install()
	{
		_ = 3;
		try
		{
			string text = await _apiClient.GetWebView2InstallerLink();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (File.Exists(VPNConstants.FilePath.WebView2FilePath))
			{
				File.Delete(VPNConstants.FilePath.WebView2FilePath);
			}
			using (HttpResponseMessage response = await _httpClient.GetAsync(new Uri(text), HttpCompletionOption.ResponseHeadersRead, CancellationToken.None))
			{
				response.EnsureSuccessStatusCode();
				using Stream source = await response.Content.ReadAsStreamAsync();
				using FileStream destination = new FileStream(VPNConstants.FilePath.WebView2FilePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
				await source.CopyToAsync(destination);
			}
			StartInstaller();
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "Install", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\WebView2Installer.cs", 72);
			DeleteIncompleteInstaller();
		}
	}

	private void StartInstaller()
	{
		if (File.Exists(VPNConstants.FilePath.WebView2FilePath))
		{
			_logger?.Information("Downloading Microsoft WebView2 Runtime complete", "StartInstaller", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\WebView2Installer.cs", 81);
			Process.Start(VPNConstants.FilePath.WebView2FilePath);
			Process.GetCurrentProcess().Kill();
		}
	}

	private void DeleteIncompleteInstaller()
	{
		try
		{
			if (File.Exists(VPNConstants.FilePath.WebView2FilePath))
			{
				File.Delete(VPNConstants.FilePath.WebView2FilePath);
			}
		}
		catch (Exception exception)
		{
			_logger?.Error(exception, "DeleteIncompleteInstaller", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\WebView2Installer.cs", 98);
		}
	}
}
