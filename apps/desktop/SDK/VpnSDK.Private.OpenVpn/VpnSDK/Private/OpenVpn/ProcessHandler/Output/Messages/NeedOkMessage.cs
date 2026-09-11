namespace VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

internal class NeedOkMessage : IOutputMessage
{
	internal string Message { get; }

	internal NeedOkMessage(string message)
	{
		Message = message;
	}
}
