using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Enums;
using NextAiVPN.UI.MessageBoxWindows.InfoMessageBox;

namespace NextAiVPN.Services.Persistence;

internal class VpnModesValidator : IVpnModeValidator
{
	private readonly SDKMonitor _sdkMonitor;

	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly IApiClient _apiClient;

	public VpnModesValidator(SDKMonitor sdkMonitor, IAppSettingsHelper appSettingsHelper, IAppLogger logger, IApiClient apiClient)
	{
		_sdkMonitor = sdkMonitor;
		_appSettingsHelper = appSettingsHelper;
		_logger = logger;
		_apiClient = apiClient;
	}

	public async Task<List<VpnType>> Validate(bool showStreamingMessageBox)
	{
		ClientConfig clientConfig = await GetClientConfigService().GetClientConfig();
		List<VpnType> result = new List<VpnType>();
		try
		{
			if (clientConfig == null)
			{
				_logger?.Error("No client configuration data", "Validate", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\VpnModesValidator.cs", 61);
				return result;
			}
			if (clientConfig.Success == 0)
			{
				_logger?.Information("clientData.Success == 0. Message" + clientConfig.Message, "Validate", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\VpnModesValidator.cs", 68);
				return result;
			}
			foreach (Data datum in clientConfig.Data)
			{
				if (!datum.Name.Equals("streaming_mode"))
				{
					continue;
				}
				if (datum.Value != null && !datum.Value.Enable)
				{
					_sdkMonitor.IsStreamingModeAvailable = false;
					_sdkMonitor.VpnExpandedWindow.ExpandedLocations.SetVpnMode(VpnType.NextAiVPN, skipValidation: true);
					_sdkMonitor.TaskBarService.HideStreaming();
					result.Add(VpnType.Streaming);
					_logger?.Information("Streaming mode is not available. Message: " + datum.Value.Subtitle, "Validate", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\VpnModesValidator.cs", 85);
					if (showStreamingMessageBox)
					{
						MessageBoxWindowWindow messageBoxWindowWindow = new MessageBoxWindowWindow(datum.Value.Title, datum.Value.Subtitle);
						messageBoxWindowWindow.Width = 400.0;
						messageBoxWindowWindow.ShowDialog();
					}
				}
				else
				{
					_sdkMonitor.IsStreamingModeAvailable = true;
					await _sdkMonitor.GetStreamingLocations();
					_sdkMonitor.TaskBarService.ShowStreaming();
				}
			}
		}
		catch (Exception ex)
		{
			_logger?.Error(ex.Message, "Validate", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\VpnModesValidator.cs", 106);
		}
		return result;
	}

	private IClientConfigService GetClientConfigService()
	{
		if (_sdkMonitor.AccountTypeHelper.GetAccountType() == AccountType.NextAiTechnology)
		{
			return new NextAiTechnologyClientConfigService(_appSettingsHelper, _logger);
		}
		return new NextAiGlobalClientConfigService(_appSettingsHelper, _logger, _apiClient);
	}
}
