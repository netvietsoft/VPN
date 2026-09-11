using System.Threading.Tasks;
using NextAiVPN.Entities;

namespace NextAiVPN.Services;

internal interface IClientConfigService
{
	Task<ClientConfig> GetClientConfig();
}
