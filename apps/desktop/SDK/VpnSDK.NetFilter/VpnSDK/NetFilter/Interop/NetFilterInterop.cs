using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace VpnSDK.NetFilter.Interop;

internal class NetFilterInterop
{
	public enum NF_STATUS
	{
		NF_STATUS_SUCCESS = 0,
		NF_STATUS_FAIL = -1,
		NF_STATUS_INVALID_ENDPOINT_ID = -2,
		NF_STATUS_NOT_INITIALIZED = -3,
		NF_STATUS_IO_ERROR = -4
	}

	public enum NF_DIRECTION
	{
		NF_D_IN = 1,
		NF_D_OUT,
		NF_D_BOTH
	}

	public enum NF_FILTERING_FLAG
	{
		NF_ALLOW = 0,
		NF_BLOCK = 1,
		NF_FILTER = 2,
		NF_SUSPENDED = 4,
		NF_OFFLINE = 8,
		NF_INDICATE_CONNECT_REQUESTS = 0x10,
		NF_DISABLE_REDIRECT_PROTECTION = 0x20,
		NF_PEND_CONNECT_REQUEST = 0x40,
		NF_FILTER_AS_IP_PACKETS = 0x80,
		NF_READONLY = 0x100,
		NF_CONTROL_FLOW = 0x200,
		NF_REDIRECT = 0x400,
		NF_BYPASS_IP_PACKETS = 0x800
	}

	public enum NF_FLAGS
	{
		NFF_NONE = 0,
		NFF_DONT_DISABLE_TEREDO = 1,
		NFF_DONT_DISABLE_TCP_OFFLOADING = 2,
		NFF_DISABLE_AUTO_REGISTER = 4,
		NFF_DISABLE_AUTO_START = 8
	}

	public enum NF_APPEND
	{
		NF_HEAD,
		NF_TAIL
	}

	public enum NF_CONSTS
	{
		NF_MAX_ADDRESS_LENGTH = 28,
		NF_MAX_IP_ADDRESS_LENGTH = 16
	}

	public enum NF_DRIVER_TYPE
	{
		DT_UNKNOWN,
		DT_TDI,
		DT_WFP
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_RULE
	{
		public int protocol;

		public uint processId;

		public byte direction;

		public ushort localPort;

		public ushort remotePort;

		public ushort ip_family;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] localIpAddress;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] localIpAddressMask;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] remoteIpAddress;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] remoteIpAddressMask;

		public uint filteringFlag;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_PORT_RANGE
	{
		public ushort valueLow;

		public ushort valueHigh;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
	public struct NF_RULE_EX
	{
		public int protocol;

		public uint processId;

		public byte direction;

		public ushort localPort;

		public ushort remotePort;

		public ushort ip_family;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] localIpAddress;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] localIpAddressMask;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] remoteIpAddress;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] remoteIpAddressMask;

		public uint filteringFlag;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string processName;

		public NF_PORT_RANGE localPortRange;

		public NF_PORT_RANGE remotePortRange;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
		public byte[] redirectTo;

		public uint localProxyProcessId;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_TCP_CONN_INFO
	{
		public uint filteringFlag;

		public uint processId;

		public byte direction;

		public ushort ip_family;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
		public byte[] localAddress;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
		public byte[] remoteAddress;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_UDP_CONN_INFO
	{
		public uint processId;

		public ushort ip_family;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
		public byte[] localAddress;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_UDP_OPTIONS
	{
		public uint flags;

		public int optionsLength;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public byte[] options;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_UDP_CONN_REQUEST
	{
		public uint filteringFlag;

		public uint processId;

		public ushort ip_family;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
		public byte[] localAddress;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
		public byte[] remoteAddress;
	}

	public enum NF_IP_FLAG
	{
		NFIF_NONE,
		NFIF_READONLY
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_IP_PACKET_OPTIONS
	{
		public ushort ip_family;

		public uint ipHeaderSize;

		public uint compartmentId;

		public uint interfaceIndex;

		public uint subInterfaceIndex;

		public uint flags;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_FLOWCTL_DATA
	{
		public ulong inLimit;

		public ulong outLimit;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_FLOWCTL_MODIFY_DATA
	{
		public uint fcHandle;

		public NF_FLOWCTL_DATA data;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_FLOWCTL_STAT
	{
		public ulong inBytes;

		public ulong outBytes;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NF_FLOWCTL_SET_DATA
	{
		public ulong endpointId;

		public uint fcHandle;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
	public struct NF_BINDING_RULE
	{
		public int protocol;

		public uint processId;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string processName;

		public ushort localPort;

		public ushort ip_family;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] localIpAddress;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] localIpAddressMask;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] newLocalIpAddress;

		public ushort newLocalPort;

		public ulong filteringFlag;
	}

	public struct NF_RULE_EXTENDED
	{
		public NF_RULE_EX rule;

		public uint feature;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_threadStart();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_threadEnd();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_tcpConnectRequest(ulong id, ref NF_TCP_CONN_INFO pConnInfo);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_tcpConnected(ulong id, ref NF_TCP_CONN_INFO pConnInfo);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_tcpClosed(ulong id, ref NF_TCP_CONN_INFO pConnInfo);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_tcpReceive(ulong id, IntPtr buf, int len);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_tcpSend(ulong id, IntPtr buf, int len);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_tcpCanReceive(ulong id);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_tcpCanSend(ulong id);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_udpCreated(ulong id, ref NF_UDP_CONN_INFO pConnInfo);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_udpConnectRequest(ulong id, ref NF_UDP_CONN_REQUEST pConnReq);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_udpClosed(ulong id, ref NF_UDP_CONN_INFO pConnInfo);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_udpReceive(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_udpSend(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_udpCanReceive(ulong id);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_udpCanSend(ulong id);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_ipReceive(IntPtr buf, int len, ref NF_IP_PACKET_OPTIONS ipOptions);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void cbd_ipSend(IntPtr buf, int len, ref NF_IP_PACKET_OPTIONS ipOptions);

	public interface NF_EventHandler
	{
		void threadStart();

		void threadEnd();

		void tcpConnectRequest(ulong id, ref NF_TCP_CONN_INFO pConnInfo);

		void tcpConnected(ulong id, NF_TCP_CONN_INFO pConnInfo);

		void tcpClosed(ulong id, NF_TCP_CONN_INFO pConnInfo);

		void tcpReceive(ulong id, IntPtr buf, int len);

		void tcpSend(ulong id, IntPtr buf, int len);

		void tcpCanReceive(ulong id);

		void tcpCanSend(ulong id);

		void udpCreated(ulong id, NF_UDP_CONN_INFO pConnInfo);

		void udpConnectRequest(ulong id, ref NF_UDP_CONN_REQUEST pConnReq);

		void udpClosed(ulong id, NF_UDP_CONN_INFO pConnInfo);

		void udpReceive(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options, int optionsLen);

		void udpSend(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options, int optionsLen);

		void udpCanReceive(ulong id);

		void udpCanSend(ulong id);
	}

	public interface NF_IPEventHandler
	{
		void ipReceive(IntPtr buf, int len, ref NF_IP_PACKET_OPTIONS ipOptions);

		void ipSend(IntPtr buf, int len, ref NF_IP_PACKET_OPTIONS ipOptions);
	}

	private class NF_EventHandlerFwd
	{
		public static NF_EventHandler m_pEventHandler;

		public static void threadStart()
		{
			m_pEventHandler.threadStart();
		}

		public static void threadEnd()
		{
			m_pEventHandler.threadEnd();
		}

		public static void tcpConnectRequest(ulong id, ref NF_TCP_CONN_INFO pConnInfo)
		{
			m_pEventHandler.tcpConnectRequest(id, ref pConnInfo);
		}

		public static void tcpConnected(ulong id, ref NF_TCP_CONN_INFO pConnInfo)
		{
			m_pEventHandler.tcpConnected(id, pConnInfo);
		}

		public static void tcpClosed(ulong id, ref NF_TCP_CONN_INFO pConnInfo)
		{
			m_pEventHandler.tcpClosed(id, pConnInfo);
		}

		public static void tcpReceive(ulong id, IntPtr buf, int len)
		{
			m_pEventHandler.tcpReceive(id, buf, len);
		}

		public static void tcpSend(ulong id, IntPtr buf, int len)
		{
			m_pEventHandler.tcpSend(id, buf, len);
		}

		public static void tcpCanReceive(ulong id)
		{
			m_pEventHandler.tcpCanReceive(id);
		}

		public static void tcpCanSend(ulong id)
		{
			m_pEventHandler.tcpCanSend(id);
		}

		public static void udpCreated(ulong id, ref NF_UDP_CONN_INFO pConnInfo)
		{
			m_pEventHandler.udpCreated(id, pConnInfo);
		}

		public static void udpConnectRequest(ulong id, ref NF_UDP_CONN_REQUEST pConnReq)
		{
			m_pEventHandler.udpConnectRequest(id, ref pConnReq);
		}

		public static void udpClosed(ulong id, ref NF_UDP_CONN_INFO pConnInfo)
		{
			m_pEventHandler.udpClosed(id, pConnInfo);
		}

		public unsafe static void udpReceive(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options)
		{
			if (options.ToInt64() != 0L)
			{
				int optionsLen = 8 + ((NF_UDP_OPTIONS)Marshal.PtrToStructure(options, typeof(NF_UDP_OPTIONS))).optionsLength;
				m_pEventHandler.udpReceive(id, remoteAddress, buf, len, options, optionsLen);
			}
			else
			{
				m_pEventHandler.udpReceive(id, remoteAddress, buf, len, (IntPtr)(void*)null, 0);
			}
		}

		public unsafe static void udpSend(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options)
		{
			if (options.ToInt64() != 0L)
			{
				int optionsLen = 8 + ((NF_UDP_OPTIONS)Marshal.PtrToStructure(options, typeof(NF_UDP_OPTIONS))).optionsLength;
				m_pEventHandler.udpSend(id, remoteAddress, buf, len, options, optionsLen);
			}
			else
			{
				m_pEventHandler.udpSend(id, remoteAddress, buf, len, (IntPtr)(void*)null, 0);
			}
		}

		public static void udpCanReceive(ulong id)
		{
			m_pEventHandler.udpCanReceive(id);
		}

		public static void udpCanSend(ulong id)
		{
			m_pEventHandler.udpCanSend(id);
		}
	}

	private class NF_IPEventHandlerFwd
	{
		public static NF_IPEventHandler m_pEventHandler;

		public static void ipReceive(IntPtr buf, int len, ref NF_IP_PACKET_OPTIONS ipOptions)
		{
			m_pEventHandler.ipReceive(buf, len, ref ipOptions);
		}

		public static void ipSend(IntPtr buf, int len, ref NF_IP_PACKET_OPTIONS ipOptions)
		{
			m_pEventHandler.ipSend(buf, len, ref ipOptions);
		}
	}

	public class NFUtil
	{
		public static SocketAddress convertAddress(byte[] buf)
		{
			if (buf == null)
			{
				return new SocketAddress(AddressFamily.InterNetwork);
			}
			SocketAddress socketAddress = new SocketAddress((AddressFamily)buf[0], 28);
			for (int i = 0; i < 28; i++)
			{
				socketAddress[i] = buf[i];
			}
			return socketAddress;
		}

		public static string addressToString(SocketAddress addr)
		{
			IPEndPoint iPEndPoint = ((addr.Family != AddressFamily.InterNetworkV6) ? new IPEndPoint(0L, 0) : new IPEndPoint(IPAddress.IPv6None, 0));
			iPEndPoint = (IPEndPoint)iPEndPoint.Create(addr);
			return iPEndPoint.ToString();
		}

		public static IPEndPoint stringToAddress(string saddr)
		{
			int num = saddr.LastIndexOf(':');
			if (num > 0)
			{
				IPAddress address = IPAddress.Parse(saddr.Substring(0, num));
				ushort port = Convert.ToUInt16(saddr.Substring(num + 1, saddr.Length - num - 1));
				return new IPEndPoint(address, port);
			}
			return new IPEndPoint(IPAddress.Parse(saddr), 0);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct NF_EventHandlerInternal
	{
		public cbd_threadStart threadStart;

		public cbd_threadEnd threadEnd;

		public cbd_tcpConnectRequest tcpConnectRequest;

		public cbd_tcpConnected tcpConnected;

		public cbd_tcpClosed tcpClosed;

		public cbd_tcpReceive tcpReceive;

		public cbd_tcpSend tcpSend;

		public cbd_tcpCanReceive tcpCanReceive;

		public cbd_tcpCanSend tcpCanSend;

		public cbd_udpCreated udpCreated;

		public cbd_udpConnectRequest udpConnectRequest;

		public cbd_udpClosed udpClosed;

		public cbd_udpReceive udpReceive;

		public cbd_udpSend udpSend;

		public cbd_udpCanReceive udpCanReceive;

		public cbd_udpCanSend udpCanSend;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct NF_IPEventHandlerInternal
	{
		public cbd_ipReceive ipReceive;

		public cbd_ipSend ipSend;
	}

	public class NFAPI
	{
		public enum NF_SOCKET_OPTIONS
		{
			TCP_SOCKET_NODELAY = 1,
			TCP_SOCKET_KEEPALIVE,
			TCP_SOCKET_OOBINLINE,
			TCP_SOCKET_BSDURGENT,
			TCP_SOCKET_ATMARK,
			TCP_SOCKET_WINDOW
		}

		private unsafe static IntPtr m_pEventHandlerRaw = (IntPtr)(void*)null;

		private static NF_EventHandlerInternal m_pEventHandler;

		private unsafe static IntPtr m_pIPEventHandlerRaw = (IntPtr)(void*)null;

		private static NF_IPEventHandlerInternal m_pIPEventHandler;

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_init(string driverName, IntPtr pHandler);

		public static NF_STATUS nf_init(string driverName, NF_EventHandler pHandler)
		{
			NF_EventHandlerFwd.m_pEventHandler = pHandler;
			nf_adjustProcessPriviledges();
			m_pEventHandler = default(NF_EventHandlerInternal);
			m_pEventHandler.threadStart = NF_EventHandlerFwd.threadStart;
			m_pEventHandler.threadEnd = NF_EventHandlerFwd.threadEnd;
			m_pEventHandler.tcpConnectRequest = NF_EventHandlerFwd.tcpConnectRequest;
			m_pEventHandler.tcpConnected = NF_EventHandlerFwd.tcpConnected;
			m_pEventHandler.tcpClosed = NF_EventHandlerFwd.tcpClosed;
			m_pEventHandler.tcpReceive = NF_EventHandlerFwd.tcpReceive;
			m_pEventHandler.tcpSend = NF_EventHandlerFwd.tcpSend;
			m_pEventHandler.tcpCanReceive = NF_EventHandlerFwd.tcpCanReceive;
			m_pEventHandler.tcpCanSend = NF_EventHandlerFwd.tcpCanSend;
			m_pEventHandler.udpCreated = NF_EventHandlerFwd.udpCreated;
			m_pEventHandler.udpConnectRequest = NF_EventHandlerFwd.udpConnectRequest;
			m_pEventHandler.udpClosed = NF_EventHandlerFwd.udpClosed;
			m_pEventHandler.udpReceive = NF_EventHandlerFwd.udpReceive;
			m_pEventHandler.udpSend = NF_EventHandlerFwd.udpSend;
			m_pEventHandler.udpCanReceive = NF_EventHandlerFwd.udpCanReceive;
			m_pEventHandler.udpCanSend = NF_EventHandlerFwd.udpCanSend;
			m_pEventHandlerRaw = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NF_EventHandlerInternal)));
			Marshal.StructureToPtr(m_pEventHandler, m_pEventHandlerRaw, fDeleteOld: true);
			return nf_init(driverName, m_pEventHandlerRaw);
		}

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern void nf_free();

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_registerDriver(string driverName);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_registerDriverEx(string driverName, string driverPath);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_unRegisterDriver(string driverName);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_tcpSetConnectionState(ulong id, int suspended);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_tcpPostSend(ulong id, IntPtr buf, int len);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_tcpPostReceive(ulong id, IntPtr buf, int len);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_tcpClose(ulong id);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_udpSetConnectionState(ulong id, int suspended);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_udpPostSend(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_udpPostReceive(ulong id, IntPtr remoteAddress, IntPtr buf, int len, IntPtr options);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_ipPostReceive(IntPtr buf, int len, ref NF_IP_PACKET_OPTIONS options);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_ipPostSend(IntPtr buf, int len, ref NF_IP_PACKET_OPTIONS options);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		private static extern NF_STATUS nf_addRule(ref NF_RULE pRule, int toHead);

		private static void updateAddressLength(ref byte[] buf)
		{
			if (buf == null)
			{
				buf = new byte[16];
			}
			else if (buf.Length < 16)
			{
				Array.Resize(ref buf, 16);
			}
		}

		public static NF_STATUS nf_addRule(NF_RULE pRule, int toHead)
		{
			updateAddressLength(ref pRule.localIpAddress);
			updateAddressLength(ref pRule.localIpAddressMask);
			updateAddressLength(ref pRule.remoteIpAddress);
			updateAddressLength(ref pRule.remoteIpAddressMask);
			return nf_addRule(ref pRule, toHead);
		}

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		private static extern NF_STATUS nf_addRuleEx(ref NF_RULE_EX pRule, int toHead);

		public static NF_STATUS nf_addRuleEx(NF_RULE_EX pRule, int toHead)
		{
			updateAddressLength(ref pRule.localIpAddress);
			updateAddressLength(ref pRule.localIpAddressMask);
			updateAddressLength(ref pRule.remoteIpAddress);
			updateAddressLength(ref pRule.remoteIpAddressMask);
			return nf_addRuleEx(ref pRule, toHead);
		}

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		private static extern NF_STATUS nf_setRules(IntPtr pRules, int count);

		public static NF_STATUS nf_setRules(NF_RULE[] rules)
		{
			IntPtr pRules = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NF_RULE)) * rules.Length);
			long num = pRules.ToInt64();
			for (int i = 0; i < rules.Length; i++)
			{
				NF_RULE structure = rules[i];
				updateAddressLength(ref structure.localIpAddress);
				updateAddressLength(ref structure.localIpAddressMask);
				updateAddressLength(ref structure.remoteIpAddress);
				updateAddressLength(ref structure.remoteIpAddressMask);
				Marshal.StructureToPtr(structure, new IntPtr(num), fDeleteOld: false);
				num += Marshal.SizeOf(typeof(NF_RULE));
			}
			return nf_setRules(pRules, rules.Length);
		}

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		private static extern NF_STATUS nf_setRulesEx(IntPtr pRules, int count);

		public static NF_STATUS nf_setRulesEx(NF_RULE_EX[] rules)
		{
			IntPtr pRules = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NF_RULE_EX)) * rules.Length);
			long num = pRules.ToInt64();
			for (int i = 0; i < rules.Length; i++)
			{
				NF_RULE_EX structure = rules[i];
				updateAddressLength(ref structure.localIpAddress);
				updateAddressLength(ref structure.localIpAddressMask);
				updateAddressLength(ref structure.remoteIpAddress);
				updateAddressLength(ref structure.remoteIpAddressMask);
				Marshal.StructureToPtr(structure, new IntPtr(num), fDeleteOld: false);
				num += Marshal.SizeOf(typeof(NF_RULE_EX));
			}
			return nf_setRulesEx(pRules, rules.Length);
		}

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_deleteRules();

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint nf_setTCPTimeout(uint timeout);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_tcpDisableFiltering(ulong id);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_udpDisableFiltering(ulong id);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool nf_tcpIsProxy(uint processId);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern void nf_setOptions(uint nThreads, uint flags);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_completeTCPConnectRequest(ulong id, ref NF_TCP_CONN_INFO pConnInfo);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_completeUDPConnectRequest(ulong id, ref NF_UDP_CONN_REQUEST pConnInfo);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_getTCPConnInfo(ulong id, ref NF_TCP_CONN_INFO pConnInfo);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_getUDPConnInfo(ulong id, ref NF_UDP_CONN_INFO pConnInfo);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool nf_getProcessNameW(uint processId, IntPtr buf, int len);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool nf_getProcessNameFromKernel(uint processId, IntPtr buf, int len);

		public static string nf_getProcessNameFromKernel(uint processId)
		{
			IntPtr intPtr = Marshal.AllocCoTaskMem(520);
			string result = "System";
			try
			{
				if (nf_getProcessNameFromKernel(processId, intPtr, 520))
				{
					result = Marshal.PtrToStringUni(intPtr);
				}
			}
			finally
			{
				Marshal.FreeCoTaskMem(intPtr);
			}
			return result;
		}

		public static string nf_getProcessName(uint processId)
		{
			IntPtr intPtr = Marshal.AllocCoTaskMem(520);
			string result = "System";
			try
			{
				if (nf_getProcessNameW(processId, intPtr, 520))
				{
					result = Marshal.PtrToStringUni(intPtr);
				}
			}
			finally
			{
				Marshal.FreeCoTaskMem(intPtr);
			}
			return result;
		}

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern void nf_adjustProcessPriviledges();

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		private static extern void nf_setIPEventHandler(IntPtr pHandler);

		public static void nf_setIPEventHandler(NF_IPEventHandler pHandler)
		{
			NF_IPEventHandlerFwd.m_pEventHandler = pHandler;
			m_pIPEventHandler = default(NF_IPEventHandlerInternal);
			m_pIPEventHandler.ipReceive = NF_IPEventHandlerFwd.ipReceive;
			m_pIPEventHandler.ipSend = NF_IPEventHandlerFwd.ipSend;
			m_pIPEventHandlerRaw = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NF_IPEventHandlerInternal)));
			Marshal.StructureToPtr(m_pIPEventHandler, m_pIPEventHandlerRaw, fDeleteOld: true);
			nf_setIPEventHandler(m_pIPEventHandlerRaw);
		}

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_addFlowCtl(ref NF_FLOWCTL_DATA pData, ref uint pFcHandle);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_deleteFlowCtl(uint fcHandle);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_setTCPFlowCtl(ulong id, uint fcHandle);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_setUDPFlowCtl(ulong id, uint fcHandle);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_modifyFlowCtl(uint fcHandle, ref NF_FLOWCTL_DATA pData);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_getFlowCtlStat(uint fcHandle, ref NF_FLOWCTL_STAT pStat);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_getTCPStat(ulong id, ref NF_FLOWCTL_STAT pStat);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_getUDPStat(ulong id, ref NF_FLOWCTL_STAT pStat);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_tcpSetSockOpt(ulong id, NF_SOCKET_OPTIONS optname, ref int optval, int optlen);

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_tcpSetSockOpt(ulong id, NF_SOCKET_OPTIONS optname, IntPtr optval, int optlen);

		public static NF_STATUS nf_tcpSetSockOpt(ulong id, NF_SOCKET_OPTIONS optname, bool optval)
		{
			int optval2 = (optval ? 1 : 0);
			return nf_tcpSetSockOpt(id, optname, ref optval2, Marshal.SizeOf(typeof(int)));
		}

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		private static extern NF_STATUS nf_addBindingRule(ref NF_BINDING_RULE pRule, int toHead);

		public static NF_STATUS nf_addBindingRule(NF_BINDING_RULE pRule, int toHead)
		{
			updateAddressLength(ref pRule.localIpAddress);
			updateAddressLength(ref pRule.localIpAddressMask);
			updateAddressLength(ref pRule.newLocalIpAddress);
			return nf_addBindingRule(ref pRule, toHead);
		}

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern NF_STATUS nf_deleteBindingRules();

		[DllImport("nfapi", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint nf_getDriverType();
	}
}
