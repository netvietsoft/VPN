using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsAPICodePack.Net;

[ComImport]
[Guid("DCB00001-570F-4A9B-8D69-199FDBA5723B")]
[TypeLibType(TypeLibTypeFlags.FOleAutomation)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface INetworkListManagerEvents
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void ConnectivityChanged([In] ConnectivityStates newConnectivity);
}
