using System;

namespace VpnSDK.Private.OpenVpn.ProcessHandler.Output.Messages;

internal class FatalMessage : IOutputMessage
{
	internal Exception Exception { get; }

	internal FatalMessage(Exception exception)
	{
		Exception = exception;
	}
}
