using System.Runtime.InteropServices;

namespace VpnSDK.Common.Interop;

internal class IpHelperInterop
{
	private const string DllName = "iphlpapi.dll";

	[DllImport("iphlpapi.dll", CharSet = CharSet.Unicode)]
	public static extern int CreateIpForwardEntry(nint pRoute);

	[DllImport("iphlpapi.dll", CharSet = CharSet.Unicode)]
	public static extern int DeleteIpForwardEntry(nint pRoute);

	[DllImport("iphlpapi.dll", CharSet = CharSet.Unicode)]
	public static extern int GetIpForwardTable(nint pIpForwardTable, ref int pdwSize, bool bOrder);
}
