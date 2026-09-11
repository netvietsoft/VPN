namespace VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

internal class NeedStrMessage : IOutputMessage
{
	internal string Message { get; }

	internal string Key { get; }

	internal NeedStrMessage(string message, string key)
	{
		Message = message;
		Key = key;
	}
}
