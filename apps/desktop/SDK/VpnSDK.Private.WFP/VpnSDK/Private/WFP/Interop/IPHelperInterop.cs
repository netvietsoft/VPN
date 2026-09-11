using System;
using System.Runtime.InteropServices;

namespace VpnSDK.Private.WFP.Interop;

internal static class IPHelperInterop
{
	internal class NetConstants
	{
		public const int IF_MAX_STRING_SIZE = 256;

		public const int IF_MAX_PHYS_ADDRESS_LENGTH = 32;

		public const int ANY_SIZE = 1;

		public const int IF_TYPE_PPP = 23;

		public const uint NO_ERROR = 0u;
	}

	internal enum NET_IF_CONNECTION_TYPE
	{
		NET_IF_CONNECTION_DEDICATED = 1,
		NET_IF_CONNECTION_PASSIVE,
		NET_IF_CONNECTION_DEMAND,
		NET_IF_CONNECTION_MAXIMUM
	}

	internal enum NET_IF_ADMIN_STATUS
	{
		NET_IF_ADMIN_STATUS_UP = 1,
		NET_IF_ADMIN_STATUS_DOWN,
		NET_IF_ADMIN_STATUS_TESTING
	}

	internal enum IF_OPER_STATUS
	{
		IfOperStatusUp = 1,
		IfOperStatusDown,
		IfOperStatusTesting,
		IfOperStatusUnknown,
		IfOperStatusDormant,
		IfOperStatusNotPresent,
		IfOperStatusLowerLayerDown
	}

	internal enum NET_IF_DIRECTION_TYPE
	{
		NET_IF_DIRECTION_SENDRECEIVE,
		NET_IF_DIRECTION_SENDONLY,
		NET_IF_DIRECTION_RECEIVEONLY,
		NET_IF_DIRECTION_MAXIMUM
	}

	internal enum NET_IF_ACCESS_TYPE
	{
		NET_IF_ACCESS_LOOPBACK = 1,
		NET_IF_ACCESS_BROADCAST,
		NET_IF_ACCESS_POINT_TO_POINT,
		NET_IF_ACCESS_POINT_TO_MULTI_POINT,
		NET_IF_ACCESS_MAXIMUM
	}

	internal enum NDIS_PHYSICAL_MEDIUM
	{
		NdisPhysicalMediumUnspecified,
		NdisPhysicalMediumWirelessLan,
		NdisPhysicalMediumCableModem,
		NdisPhysicalMediumPhoneLine,
		NdisPhysicalMediumPowerLine,
		NdisPhysicalMediumDSL,
		NdisPhysicalMediumFibreChannel,
		NdisPhysicalMedium1394,
		NdisPhysicalMediumWirelessWan,
		NdisPhysicalMediumNative802_11,
		NdisPhysicalMediumBluetooth,
		NdisPhysicalMediumInfiniband,
		NdisPhysicalMediumWiMax,
		NdisPhysicalMediumUWB,
		NdisPhysicalMedium802_3,
		NdisPhysicalMedium802_5,
		NdisPhysicalMediumIrda,
		NdisPhysicalMediumWiredWAN,
		NdisPhysicalMediumWiredCoWan,
		NdisPhysicalMediumOther,
		NdisPhysicalMediumMax
	}

	internal enum NDIS_MEDIUM
	{
		NdisMedium802_3,
		NdisMedium802_5,
		NdisMediumFddi,
		NdisMediumWan,
		NdisMediumLocalTalk,
		NdisMediumDix,
		NdisMediumArcnetRaw,
		NdisMediumArcnet878_2,
		NdisMediumAtm,
		NdisMediumWirelessWan,
		NdisMediumIrda,
		NdisMediumBpc,
		NdisMediumCoWan,
		NdisMedium1394,
		NdisMediumInfiniBand,
		NdisMediumTunnel,
		NdisMediumNative802_11,
		NdisMediumLoopback,
		NdisMediumWiMAX,
		NdisMediumIP,
		NdisMediumMax
	}

	internal enum TUNNEL_TYPE
	{
		TUNNEL_TYPE_NONE = 0,
		TUNNEL_TYPE_OTHER = 1,
		TUNNEL_TYPE_DIRECT = 2,
		TUNNEL_TYPE_6TO4 = 11,
		TUNNEL_TYPE_ISATAP = 13,
		TUNNEL_TYPE_TEREDO = 14,
		TUNNEL_TYPE_IPHTTPS = 15
	}

	internal enum NET_IF_MEDIA_CONNECT_STATE
	{
		MediaConnectStateUnknown,
		MediaConnectStateConnected,
		MediaConnectStateDisconnected
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct NET_LUID
	{
		public struct NET_LUID_Bitvector
		{
			public ulong bitvector;

			public ulong Reserved
			{
				get
				{
					return bitvector & 0xFFFFFF;
				}
				set
				{
					bitvector = value | bitvector;
				}
			}

			public ulong NetLuidIndex
			{
				get
				{
					return (bitvector & 0xFF000000u) / 16777216;
				}
				set
				{
					bitvector = (value * 16777216) | bitvector;
				}
			}

			public ulong IfType
			{
				get
				{
					return (bitvector & 0xFFFF0000u) / 281474976710656L;
				}
				set
				{
					bitvector = (value * 281474976710656L) | bitvector;
				}
			}
		}

		[FieldOffset(0)]
		public ulong Value;

		[FieldOffset(0)]
		public NET_LUID_Bitvector Info;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct MIB_IF_ROW2
	{
		public struct MIB_IF_ROW2_Bitvector
		{
			public byte bitvector;

			public byte HardwareInterface
			{
				get
				{
					return (byte)(bitvector & 1);
				}
				set
				{
					bitvector = (byte)(value | bitvector);
				}
			}

			public byte FilterInterface
			{
				get
				{
					return (byte)((uint)(bitvector & 2) / 2u);
				}
				set
				{
					bitvector = (byte)((value * 2) | bitvector);
				}
			}

			public byte ConnectorPresent
			{
				get
				{
					return (byte)((uint)(bitvector & 4) / 4u);
				}
				set
				{
					bitvector = (byte)((value * 4) | bitvector);
				}
			}

			public byte NotAuthenticated
			{
				get
				{
					return (byte)((uint)(bitvector & 8) / 8u);
				}
				set
				{
					bitvector = (byte)((value * 8) | bitvector);
				}
			}

			public byte NotMediaConnected
			{
				get
				{
					return (byte)((uint)(bitvector & 0x10) / 16u);
				}
				set
				{
					bitvector = (byte)((value * 16) | bitvector);
				}
			}

			public byte Paused
			{
				get
				{
					return (byte)((uint)(bitvector & 0x20) / 32u);
				}
				set
				{
					bitvector = (byte)((value * 32) | bitvector);
				}
			}

			public byte LowPower
			{
				get
				{
					return (byte)((uint)(bitvector & 0x40) / 64u);
				}
				set
				{
					bitvector = (byte)((value * 64) | bitvector);
				}
			}

			public byte EndPointInterface
			{
				get
				{
					return (byte)((uint)(bitvector & 0x80) / 128u);
				}
				set
				{
					bitvector = (byte)((value * 128) | bitvector);
				}
			}
		}

		public NET_LUID InterfaceLuid;

		public uint InterfaceIndex;

		public GUID InterfaceGuid;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
		public string Alias;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
		public string Description;

		public uint PhysicalAddressLength;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public byte[] PhysicalAddress;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public byte[] PermanentPhysicalAddress;

		public uint Mtu;

		public uint Type;

		public TUNNEL_TYPE TunnelType;

		public NDIS_MEDIUM MediaType;

		public NDIS_PHYSICAL_MEDIUM PhysicalMediumType;

		public NET_IF_ACCESS_TYPE AccessType;

		public NET_IF_DIRECTION_TYPE DirectionType;

		public MIB_IF_ROW2_Bitvector InterfaceAndOperStatusFlags;

		public IF_OPER_STATUS OperStatus;

		public NET_IF_ADMIN_STATUS AdminStatus;

		public NET_IF_MEDIA_CONNECT_STATE MediaConnectState;

		public GUID NetworkGuid;

		public NET_IF_CONNECTION_TYPE ConnectionType;

		public ulong TransmitLinkSpeed;

		public ulong ReceiveLinkSpeed;

		public ulong InOctets;

		public ulong InUcastPkts;

		public ulong InNUcastPkts;

		public ulong InDiscards;

		public ulong InErrors;

		public ulong InUnknownProtos;

		public ulong InUcastOctets;

		public ulong InMulticastOctets;

		public ulong InBroadcastOctets;

		public ulong OutOctets;

		public ulong OutUcastPkts;

		public ulong OutNUcastPkts;

		public ulong OutDiscards;

		public ulong OutErrors;

		public ulong OutUcastOctets;

		public ulong OutMulticastOctets;

		public ulong OutBroadcastOctets;

		public ulong OutQLen;
	}

	internal struct MIB_IF_TABLE2
	{
		public uint NumEntries;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public MIB_IF_ROW2[] Table;
	}

	internal class NetNativeMethods
	{
		[DllImport("Iphlpapi.dll")]
		public static extern uint GetIfTable2(ref IntPtr Table);

		[DllImport("Iphlpapi.dll")]
		public static extern void FreeMibTable(IntPtr Memory);

		[DllImport("Iphlpapi.dll")]
		public static extern uint ConvertInterfaceIndexToLuid(uint InterfaceIndex, ref NET_LUID InterfaceLuid);
	}
}
