namespace VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

internal class BytecountMessage : IOutputMessage
{
	internal int BytesIn { get; }

	internal int BytesOut { get; }

	internal BytecountMessage(int bytesIn, int bytesOut)
	{
		BytesIn = bytesIn;
		BytesOut = bytesOut;
	}
}
