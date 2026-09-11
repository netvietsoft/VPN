using VpnSDK.Private.OpenVpn.Enums;

namespace VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

internal class StateMessage : IOutputMessage
{
	internal ConnectionState State { get; }

	internal StateMessage(ConnectionState state)
	{
		State = state;
	}
}
