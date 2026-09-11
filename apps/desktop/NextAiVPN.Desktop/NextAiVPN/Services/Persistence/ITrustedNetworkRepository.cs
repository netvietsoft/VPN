using System.Collections.Generic;

namespace NextAiVPN.Services.Persistence;

internal interface ITrustedNetworkRepository
{
	IEnumerable<string> GetNetworks();

	void SetNetworks(IEnumerable<string> networks);
}
