namespace VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

internal class EchoMessage : IOutputMessage
{
	internal string Message { get; }

	internal EchoMessage(string message)
	{
		Message = message;
	}
}
