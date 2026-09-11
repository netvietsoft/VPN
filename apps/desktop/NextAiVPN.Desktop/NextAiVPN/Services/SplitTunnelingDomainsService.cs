using System.Collections.Generic;
using System.Linq;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.Services;

internal class SplitTunnelingDomainsService : ISplitTunnelingService
{
	private readonly ISplitTunnelingRepository _splitTunnelingRepository;

	public SplitTunnelingDomainsService(ISplitTunnelingRepository splitTunnelingRepository)
	{
		_splitTunnelingRepository = splitTunnelingRepository;
	}

	public IEnumerable<string> GetList()
	{
		return _splitTunnelingRepository.GetHostnamesAndIpsList();
	}

	public bool Contain(string hostnameOrIp)
	{
		return _splitTunnelingRepository.GetHostnamesAndIpsListTuple().Any(((bool isDomainIncluded, string Domain) x) => x.Domain.Equals(hostnameOrIp));
	}

	public void Add(string hostnameOrIp)
	{
		List<string> list = _splitTunnelingRepository.GetHostnamesAndIpsList().ToList();
		list.Add(hostnameOrIp);
		_splitTunnelingRepository.SetHostnamesAndIpsList(list);
	}

	public void AddRange(IList<string> hostnameAndIpList)
	{
		_splitTunnelingRepository.SetHostnamesAndIpsList(hostnameAndIpList);
	}

	public void Remove(string hostnameOrIp)
	{
		if (Contain(hostnameOrIp))
		{
			List<(bool isDomainIncluded, string Domain)> list = _splitTunnelingRepository.GetHostnamesAndIpsListTuple().ToList();
			(bool, string) item = list.FirstOrDefault(((bool isDomainIncluded, string Domain) x) => x.Domain.Equals(hostnameOrIp));
			list.Remove(item);
			List<string> list2 = list.Select(ConvertToDomainItem).ToList();
			if (list2.Count == 0)
			{
				list2 = new List<string>();
			}
			_splitTunnelingRepository.SetHostnamesAndIpsList(list2);
		}
	}

	private static string ConvertToDomainItem((bool isIncluded, string Domain) value)
	{
		return (value.isIncluded ? "1" : "0") + "*" + value.Domain;
	}

	public void RemoveAll()
	{
		_splitTunnelingRepository.SetHostnamesAndIpsList(new List<string>());
	}
}
