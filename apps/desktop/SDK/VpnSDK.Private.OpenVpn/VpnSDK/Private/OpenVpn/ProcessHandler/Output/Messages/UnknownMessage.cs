namespace VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

internal class UnknownMessage : IOutputMessage
{
	private string Message { get; }

	internal UnknownMessage(string message)
	{
		Message = message;
	}
}
