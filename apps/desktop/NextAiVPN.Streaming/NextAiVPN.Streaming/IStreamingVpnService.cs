using System.Collections.Generic;
using System.Threading.Tasks;
using NextAiVPN.Entities.Streaming;
using NextAiVPN.Streaming.Entities;

namespace NextAiVPN.Streaming;

public interface IStreamingVpnService
{
	Task<IEnumerable<IStreamingLocation>> GetServerListAsync();

	IEnumerable<StreamingLocation> GetServerList();

	string GetServerListString();
}
