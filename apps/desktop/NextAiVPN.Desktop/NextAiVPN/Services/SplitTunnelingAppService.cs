using System.Collections.Generic;
using System.Linq;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

internal class SplitTunnelingAppService : ISplitTunnelingService
{
	private readonly ISplitTunnelingRepository _splitTunnelingRepository;

	public SplitTunnelingAppService(ISplitTunnelingRepository splitTunnelingRepository)
	{
		_splitTunnelingRepository = splitTunnelingRepository;
	}

	public IEnumerable<string> GetList()
	{
		return _splitTunnelingRepository.GetAppList();
	}

	public bool Contain(string appPath)
	{
		return _splitTunnelingRepository.GetAppList().Contains(appPath);
	}

	public void Add(string appPath)
	{
		List<string> list = _splitTunnelingRepository.GetAppList().ToList();
		list.Add(appPath);
		_splitTunnelingRepository.SetAppList(list);
	}

	public void AddRange(IList<string> appList)
	{
		_splitTunnelingRepository.SetAppList(appList);
	}

	public void Remove(string appPath)
	{
		if (Contain(appPath))
		{
			List<string> list = _splitTunnelingRepository.GetAppList().ToList();
			list.Remove(appPath);
			_splitTunnelingRepository.SetAppList(list);
		}
	}

	public void RemoveAll()
	{
		_splitTunnelingRepository.SetAppList(new List<string>());
	}
}
