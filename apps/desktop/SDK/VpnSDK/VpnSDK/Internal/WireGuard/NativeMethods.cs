using System;
using System.IO;
using System.Runtime.InteropServices;
using VpnSDK.Core.Helpers;

namespace VpnSDK.Internal.WireGuard;

internal class NativeMethods
{
	public enum ADDRESS_FAMILY : ushort
	{
		AF_UNSPEC = 0,
		AF_INET = 2,
		AF_INET6 = 23
	}

	public struct IN_ADDR
	{
		public unsafe fixed byte bytes[4];
	}

	public struct IN6_ADDR
	{
		public unsafe fixed byte bytes[16];
	}

	public struct SOCKADDR_IN
	{
		public ushort sin_family;

		public ushort sin_port;

		public IN_ADDR sin_addr;
	}

	public struct SOCKADDR_IN6
	{
		public ushort sin6_family;

		public ushort sin6_port;

		public uint sin6_flowinfo;

		public IN6_ADDR sin6_addr;

		public uint sin6_scope_id;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct SOCKADDR_INET
	{
		[FieldOffset(0)]
		[MarshalAs(UnmanagedType.Struct)]
		public SOCKADDR_IN Ipv4;

		[FieldOffset(0)]
		[MarshalAs(UnmanagedType.Struct)]
		public SOCKADDR_IN6 Ipv6;

		[FieldOffset(0)]
		public ADDRESS_FAMILY si_family;
	}

	static NativeMethods()
	{
		string processArchitecture = Utils.GetProcessArchitecture();
		if (string.IsNullOrEmpty(processArchitecture))
		{
			throw new NotSupportedException("Unsupported CPU or OS architecture");
		}
		SetDllDirectory(Path.Combine(PathHelper.GetApplicationDirectory(), "WireGuard", processArchitecture));
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern bool SetDllDirectory(string lpPathName);

	[DllImport("wireguard.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WireGuardOpenAdapter", SetLastError = true)]
	internal static extern IntPtr openAdapter([MarshalAs(UnmanagedType.LPWStr)] string name);

	[DllImport("wireguard.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WireGuardCloseAdapter")]
	internal static extern void freeAdapter(IntPtr adapter);

	[DllImport("wireguard.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WireGuardGetConfiguration", SetLastError = true)]
	internal static extern bool getConfiguration(IntPtr adapter, byte[] iface, ref uint bytes);
}
