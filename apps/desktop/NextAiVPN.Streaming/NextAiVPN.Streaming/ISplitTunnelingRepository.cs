using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextAiVPN.Streaming;

public interface ISplitTunnelingRepository
{
	Task<IEnumerable<string>> GetItems();

	Task SetItems(IEnumerable<string> items);
}
