namespace VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

internal class LogMessage : IOutputMessage
{
	internal string Message { get; }

	internal LogMessage(string message)
	{
		Message = message;
	}
}
