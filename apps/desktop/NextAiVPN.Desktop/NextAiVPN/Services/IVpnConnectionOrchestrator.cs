using System.Threading.Tasks;
using NextAiVPN.Enums;
using NextAiVPN.Services.Persistence;
using VpnSDK.Interfaces;

namespace NextAiVPN.Services;

public interface IVpnConnectionOrchestrator
{
	Task ConnectAsync(VpnType mode, ILocation location);

	Task DisconnectAsync(VpnType mode);

	ReconnectVerdict EvaluateStreamingReconnect(int attemptNumber, int maxAttempts);
}
