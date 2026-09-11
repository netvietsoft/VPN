using System.Collections.Generic;

namespace NextAiVPN.Services;

public interface ITrustedNetworkService
{
	IEnumerable<string> GetNetworks();

	bool Contain(string network);

	void Add(string network);

	void AddRange(IList<string> networks);

	void SetNetworks(IList<string> networks);

	void Remove(string network);

	void RemoveAll();
}
