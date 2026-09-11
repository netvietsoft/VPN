using System.Collections.Generic;

namespace NextAiVPN.Services;

public interface ISplitTunnelingService
{
	IEnumerable<string> GetList();

	bool Contain(string item);

	void Add(string item);

	void AddRange(IList<string> itemList);

	void Remove(string item);

	void RemoveAll();
}
