using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NextAiVPN.Common;

namespace NextAiVPN.Streaming;

public class StreamingSplitTunnelingRepository : ISplitTunnelingRepository
{
	private readonly IAppSettingsHelper _appSettingsHelper;

	private readonly IStorageRepository _storageRepository;

	public StreamingSplitTunnelingRepository(IAppSettingsHelper appSettingsHelper, IStorageRepository storageRepository)
	{
		_appSettingsHelper = appSettingsHelper;
		_storageRepository = storageRepository;
	}

	public Task<IEnumerable<string>> GetItems()
	{
		try
		{
			string value = _appSettingsHelper.GetValue("StreamingSplitTunnelingDomainList");
			if (string.IsNullOrEmpty(value))
			{
				return Task.FromResult((IEnumerable<string>)new List<string>());
			}
			return Task.FromResult(from x in value.Split(';')
				where !string.IsNullOrEmpty(x)
				select x);
		}
		catch (Exception ex)
		{
			Logger.Log.Error(ex.Message, "GetItems", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN.Streaming\\StreamingSplitTunnelingRepository.cs", 51);
			return Task.FromResult((IEnumerable<string>)new List<string>());
		}
	}

	public async Task SetItems(IEnumerable<string> items)
	{
		string value = string.Join(";", items);
		_appSettingsHelper.SetValue("StreamingSplitTunnelingDomainList", value);
		await _storageRepository.SaveValue("StreamingSplitTunnelingDomainList", value);
	}
}
