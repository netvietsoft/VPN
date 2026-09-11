using System;

namespace Microsoft.WindowsAPICodePack.Net;

[Flags]
internal enum ConnectivityStates
{
	None = 0,
	IPv4Internet = 0x40,
	IPv4LocalNetwork = 0x20,
	IPv4NoTraffic = 1,
	IPv4Subnet = 0x10,
	IPv6Internet = 0x400,
	IPv6LocalNetwork = 0x200,
	IPv6NoTraffic = 2,
	IPv6Subnet = 0x100
}
