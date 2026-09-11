using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextAiVPN.Common;

public interface IIpHelper
{
	Task<string> GetLocalIpAsync();

	Task<string> GetPublicIpAsync(IEnumerable<string> servers);
}
