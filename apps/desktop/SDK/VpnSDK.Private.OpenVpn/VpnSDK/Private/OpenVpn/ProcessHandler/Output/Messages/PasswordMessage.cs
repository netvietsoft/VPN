using VpnSDK.Private.OpenVpn.Enums;

namespace VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

internal class PasswordMessage : IOutputMessage
{
	internal string Message { get; }

	internal AuthenticationState State { get; }

	internal PasswordMessage(string message, AuthenticationState state)
	{
		Message = message;
		State = state;
	}
}
