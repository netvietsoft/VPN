using System.Net.NetworkInformation;

namespace NextAiVPN.Services;

public interface INetworkService
{
	string GetConnectedNetworkName();

	string GetConnectedNetworkName(NetworkInterface info);
}
