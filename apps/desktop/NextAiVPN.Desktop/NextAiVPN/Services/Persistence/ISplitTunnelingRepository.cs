using System.Collections.Generic;

namespace NextAiVPN.Services.Persistence;

internal interface ISplitTunnelingRepository
{
	IEnumerable<string> GetAppList();

	IEnumerable<(bool isDomainIncluded, string Domain)> GetHostnamesAndIpsListTuple();

	IEnumerable<string> GetHostnamesAndIpsList();

	void SetAppList(IEnumerable<string> appList);

	void SetHostnamesAndIpsList(IEnumerable<string> hostnameList);
}
