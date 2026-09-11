using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NextAiVPN.Common;

namespace NextAiVPN.Services.Persistence;

internal class SplitTunnelingRepository : ISplitTunnelingRepository
{
	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IAppLogger _logger;

	private readonly IStorageRepository _localSettingsRepository;

	public SplitTunnelingRepository(IAppSettingsHelper appSettingsHelper, IAppLogger logger, IStorageRepository localSettingsRepository)
	{
		_appSettingsHelper = appSettingsHelper ?? throw new ArgumentNullException("appSettingsHelper");
		_logger = logger;
		_localSettingsRepository = localSettingsRepository;
	}

	public IEnumerable<string> GetAppList()
	{
		try
		{
			string value = _appSettingsHelper.GetValue("SplitTunnelingAppList");
			if (string.IsNullOrEmpty(value))
			{
				return new List<string>();
			}
			return value.Split(';');
		}
		catch (Exception ex)
		{
			_logger?.Error("GetAppList() - " + ex.Message, "GetAppList", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SplitTunnelingRepository.cs", 48);
			return new List<string>();
		}
	}

	public IEnumerable<(bool isDomainIncluded, string Domain)> GetHostnamesAndIpsListTuple()
	{
		try
		{
			string value = _appSettingsHelper.GetValue("SplitTunnelingHostnameIpsList");
			if (string.IsNullOrEmpty(value))
			{
				return new List<(bool, string)>();
			}
			return (from item in value.Split(';')
				select item.Split('*') into tmpItem
				select (tmpItem.FirstOrDefault().Equals("1"), tmpItem.LastOrDefault())).ToList();
		}
		catch (Exception ex)
		{
			_logger?.Error("GetHostnamesAndIpsList() - " + ex.Message, "GetHostnamesAndIpsListTuple", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SplitTunnelingRepository.cs", 70);
			return new List<(bool, string)>();
		}
	}

	public IEnumerable<string> GetHostnamesAndIpsList()
	{
		try
		{
			string value = _appSettingsHelper.GetValue(AppSettingsKeys.SplitTunnelingHostnameIpsList);
			if (string.IsNullOrEmpty(value))
			{
				return new List<string>();
			}
			return value.Split(';');
		}
		catch (Exception ex)
		{
			_logger?.Error("GetHostnamesAndIpsList() - " + ex.Message, "GetHostnamesAndIpsList", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\Services\\Persistence\\SplitTunnelingRepository.cs", 90);
			return new List<string>();
		}
	}

	public void SetAppList(IEnumerable<string> appList)
	{
		string value = string.Join(";", appList);
		_appSettingsHelper.SetValue(AppSettingsKeys.SplitTunnelingAppList, value);
		SetSettingsToLocalRepository(AppSettingsKeys.SplitTunnelingAppList, value);
	}

	public void SetHostnamesAndIpsList(IEnumerable<string> hostnameList)
	{
		string value = string.Join(";", hostnameList);
		_appSettingsHelper.SetValue(AppSettingsKeys.SplitTunnelingHostnameIpsList, value);
		SetSettingsToLocalRepository(AppSettingsKeys.SplitTunnelingHostnameIpsList, value);
	}

	private void SetSettingsToLocalRepository(string variableName, string value)
	{
		Task.Run(async delegate
		{
			await _localSettingsRepository.SaveValue(variableName, value);
		});
	}
}
