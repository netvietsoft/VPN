using System.Threading;
using System.Threading.Tasks;
using NextAiVPN.Entities.Streaming;

namespace NextAiVPN.Streaming;

public interface IVpnConnector
{
	bool IsActive { get; }

	Task<bool> TryConnect(IStreamingLocation streamingLocation);

	bool TryDisconnect();

	Task<bool> TryDisconnectAsync(CancellationToken cancellationToken = default(CancellationToken));

	void CreateOrUpdateConnection(string serverAddress);

	Task<bool> CreateOrUpdateConnectionAsync(string serverAddress);

	void RemoveConnection();

	Task<bool> RemoveConnectionAsync(CancellationToken cancellationToken = default(CancellationToken));

	void SetCredentials(string userName, string password);
}
