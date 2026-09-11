using System;

namespace Microsoft.WindowsAPICodePack.Net;

[Flags]
public enum NetworkConnectivityLevels
{
	Connected = 1,
	Disconnected = 2,
	All = Connected | Disconnected
}
