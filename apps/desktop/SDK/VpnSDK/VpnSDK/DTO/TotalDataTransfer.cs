namespace VpnSDK.DTO;

internal class TotalDataTransfer
{
	public long Sent { get; }

	public long Received { get; }

	public TotalDataTransfer(long sent, long received)
	{
		Sent = sent;
		Received = received;
	}
}
