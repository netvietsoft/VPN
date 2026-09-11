using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsAPICodePack.Net;

[ComImport]
[Guid("DCB00007-570F-4A9B-8D69-199FDBA5723B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface INetworkConnectionEvents
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void NetworkConnectionConnectivityChanged([In] Guid connectionId, [In] ConnectivityStates newConnectivity);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void NetworkConnectionPropertyChanged([In] Guid connectionId, [In] NetworkConnectionPropertyChange flags);
}
