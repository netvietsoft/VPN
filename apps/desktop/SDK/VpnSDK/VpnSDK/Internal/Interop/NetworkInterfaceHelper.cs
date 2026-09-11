using System;
using System.Runtime.InteropServices;

namespace VpnSDK.Internal.Interop;

internal class NetworkInterfaceHelper
{
	internal enum IFTYPE : uint
	{
		IF_TYPE_OTHER = 1u,
		IF_TYPE_REGULAR_1822 = 2u,
		IF_TYPE_HDH_1822 = 3u,
		IF_TYPE_DDN_X25 = 4u,
		IF_TYPE_RFC877_X25 = 5u,
		IF_TYPE_ETHERNET_CSMACD = 6u,
		IF_TYPE_IS088023_CSMACD = 7u,
		IF_TYPE_ISO88024_TOKENBUS = 8u,
		IF_TYPE_ISO88025_TOKENRING = 9u,
		IF_TYPE_ISO88026_MAN = 10u,
		IF_TYPE_STARLAN = 11u,
		IF_TYPE_PROTEON_10MBIT = 12u,
		IF_TYPE_PROTEON_80MBIT = 13u,
		IF_TYPE_HYPERCHANNEL = 14u,
		IF_TYPE_FDDI = 15u,
		IF_TYPE_LAP_B = 16u,
		IF_TYPE_SDLC = 17u,
		IF_TYPE_DS1 = 18u,
		IF_TYPE_E1 = 19u,
		IF_TYPE_BASIC_ISDN = 20u,
		IF_TYPE_PRIMARY_ISDN = 21u,
		IF_TYPE_PROP_POINT2POINT_SERIAL = 22u,
		IF_TYPE_PPP = 23u,
		IF_TYPE_SOFTWARE_LOOPBACK = 24u,
		IF_TYPE_EON = 25u,
		IF_TYPE_ETHERNET_3MBIT = 26u,
		IF_TYPE_NSIP = 27u,
		IF_TYPE_SLIP = 28u,
		IF_TYPE_ULTRA = 29u,
		IF_TYPE_DS3 = 30u,
		IF_TYPE_SIP = 31u,
		IF_TYPE_FRAMERELAY = 32u,
		IF_TYPE_RS232 = 33u,
		IF_TYPE_PARA = 34u,
		IF_TYPE_ARCNET = 35u,
		IF_TYPE_ARCNET_PLUS = 36u,
		IF_TYPE_ATM = 37u,
		IF_TYPE_MIO_X25 = 38u,
		IF_TYPE_SONET = 39u,
		IF_TYPE_X25_PLE = 40u,
		IF_TYPE_ISO88022_LLC = 41u,
		IF_TYPE_LOCALTALK = 42u,
		IF_TYPE_SMDS_DXI = 43u,
		IF_TYPE_FRAMERELAY_SERVICE = 44u,
		IF_TYPE_V35 = 45u,
		IF_TYPE_HSSI = 46u,
		IF_TYPE_HIPPI = 47u,
		IF_TYPE_MODEM = 48u,
		IF_TYPE_AAL5 = 49u,
		IF_TYPE_SONET_PATH = 50u,
		IF_TYPE_SONET_VT = 51u,
		IF_TYPE_SMDS_ICIP = 52u,
		IF_TYPE_PROP_VIRTUAL = 53u,
		IF_TYPE_PROP_MULTIPLEXOR = 54u,
		IF_TYPE_IEEE80212 = 55u,
		IF_TYPE_FIBRECHANNEL = 56u,
		IF_TYPE_HIPPIINTERFACE = 57u,
		IF_TYPE_FRAMERELAY_INTERCONNECT = 58u,
		IF_TYPE_AFLANE_8023 = 59u,
		IF_TYPE_AFLANE_8025 = 60u,
		IF_TYPE_CCTEMUL = 61u,
		IF_TYPE_FASTETHER = 62u,
		IF_TYPE_ISDN = 63u,
		IF_TYPE_V11 = 64u,
		IF_TYPE_V36 = 65u,
		IF_TYPE_G703_64K = 66u,
		IF_TYPE_G703_2MB = 67u,
		IF_TYPE_QLLC = 68u,
		IF_TYPE_FASTETHER_FX = 69u,
		IF_TYPE_CHANNEL = 70u,
		IF_TYPE_IEEE80211 = 71u,
		IF_TYPE_IBM370PARCHAN = 72u,
		IF_TYPE_ESCON = 73u,
		IF_TYPE_DLSW = 74u,
		IF_TYPE_ISDN_S = 75u,
		IF_TYPE_ISDN_U = 76u,
		IF_TYPE_LAP_D = 77u,
		IF_TYPE_IPSWITCH = 78u,
		IF_TYPE_RSRB = 79u,
		IF_TYPE_ATM_LOGICAL = 80u,
		IF_TYPE_DS0 = 81u,
		IF_TYPE_DS0_BUNDLE = 82u,
		IF_TYPE_BSC = 83u,
		IF_TYPE_ASYNC = 84u,
		IF_TYPE_CNR = 85u,
		IF_TYPE_ISO88025R_DTR = 86u,
		IF_TYPE_EPLRS = 87u,
		IF_TYPE_ARAP = 88u,
		IF_TYPE_PROP_CNLS = 89u,
		IF_TYPE_HOSTPAD = 90u,
		IF_TYPE_TERMPAD = 91u,
		IF_TYPE_FRAMERELAY_MPI = 92u,
		IF_TYPE_X213 = 93u,
		IF_TYPE_ADSL = 94u,
		IF_TYPE_RADSL = 95u,
		IF_TYPE_SDSL = 96u,
		IF_TYPE_VDSL = 97u,
		IF_TYPE_ISO88025_CRFPRINT = 98u,
		IF_TYPE_MYRINET = 99u,
		IF_TYPE_VOICE_EM = 100u,
		IF_TYPE_VOICE_FXO = 101u,
		IF_TYPE_VOICE_FXS = 102u,
		IF_TYPE_VOICE_ENCAP = 103u,
		IF_TYPE_VOICE_OVERIP = 104u,
		IF_TYPE_ATM_DXI = 105u,
		IF_TYPE_ATM_FUNI = 106u,
		IF_TYPE_ATM_IMA = 107u,
		IF_TYPE_PPPMULTILINKBUNDLE = 108u,
		IF_TYPE_IPOVER_CDLC = 109u,
		IF_TYPE_IPOVER_CLAW = 110u,
		IF_TYPE_STACKTOSTACK = 111u,
		IF_TYPE_VIRTUALIPADDRESS = 112u,
		IF_TYPE_MPC = 113u,
		IF_TYPE_IPOVER_ATM = 114u,
		IF_TYPE_ISO88025_FIBER = 115u,
		IF_TYPE_TDLC = 116u,
		IF_TYPE_GIGABITETHERNET = 117u,
		IF_TYPE_HDLC = 118u,
		IF_TYPE_LAP_F = 119u,
		IF_TYPE_V37 = 120u,
		IF_TYPE_X25_MLP = 121u,
		IF_TYPE_X25_HUNTGROUP = 122u,
		IF_TYPE_TRANSPHDLC = 123u,
		IF_TYPE_INTERLEAVE = 124u,
		IF_TYPE_FAST = 125u,
		IF_TYPE_IP = 126u,
		IF_TYPE_DOCSCABLE_MACLAYER = 127u,
		IF_TYPE_DOCSCABLE_DOWNSTREAM = 128u,
		IF_TYPE_DOCSCABLE_UPSTREAM = 129u,
		IF_TYPE_A12MPPSWITCH = 130u,
		IF_TYPE_TUNNEL = 131u,
		IF_TYPE_COFFEE = 132u,
		IF_TYPE_CES = 133u,
		IF_TYPE_ATM_SUBINTERFACE = 134u,
		IF_TYPE_L2_VLAN = 135u,
		IF_TYPE_L3_IPVLAN = 136u,
		IF_TYPE_L3_IPXVLAN = 137u,
		IF_TYPE_DIGITALPOWERLINE = 138u,
		IF_TYPE_MEDIAMAILOVERIP = 139u,
		IF_TYPE_DTM = 140u,
		IF_TYPE_DCN = 141u,
		IF_TYPE_IPFORWARD = 142u,
		IF_TYPE_MSDSL = 143u,
		IF_TYPE_IEEE1394 = 144u,
		IF_TYPE_IF_GSN = 145u,
		IF_TYPE_DVBRCC_MACLAYER = 146u,
		IF_TYPE_DVBRCC_DOWNSTREAM = 147u,
		IF_TYPE_DVBRCC_UPSTREAM = 148u,
		IF_TYPE_ATM_VIRTUAL = 149u,
		IF_TYPE_MPLS_TUNNEL = 150u,
		IF_TYPE_SRP = 151u,
		IF_TYPE_VOICEOVERATM = 152u,
		IF_TYPE_VOICEOVERFRAMERELAY = 153u,
		IF_TYPE_IDSL = 154u,
		IF_TYPE_COMPOSITELINK = 155u,
		IF_TYPE_SS7_SIGLINK = 156u,
		IF_TYPE_PROP_WIRELESS_P2P = 157u,
		IF_TYPE_FR_FORWARD = 158u,
		IF_TYPE_RFC1483 = 159u,
		IF_TYPE_USB = 160u,
		IF_TYPE_IEEE8023AD_LAG = 161u,
		IF_TYPE_BGP_POLICY_ACCOUNTING = 162u,
		IF_TYPE_FRF16_MFR_BUNDLE = 163u,
		IF_TYPE_H323_GATEKEEPER = 164u,
		IF_TYPE_H323_PROXY = 165u,
		IF_TYPE_MPLS = 166u,
		IF_TYPE_MF_SIGLINK = 167u,
		IF_TYPE_HDSL2 = 168u,
		IF_TYPE_SHDSL = 169u,
		IF_TYPE_DS1_FDL = 170u,
		IF_TYPE_POS = 171u,
		IF_TYPE_DVB_ASI_IN = 172u,
		IF_TYPE_DVB_ASI_OUT = 173u,
		IF_TYPE_PLC = 174u,
		IF_TYPE_NFAS = 175u,
		IF_TYPE_TR008 = 176u,
		IF_TYPE_GR303_RDT = 177u,
		IF_TYPE_GR303_IDT = 178u,
		IF_TYPE_ISUP = 179u,
		IF_TYPE_PROP_DOCS_WIRELESS_MACLAYER = 180u,
		IF_TYPE_PROP_DOCS_WIRELESS_DOWNSTREAM = 181u,
		IF_TYPE_PROP_DOCS_WIRELESS_UPSTREAM = 182u,
		IF_TYPE_HIPERLAN2 = 183u,
		IF_TYPE_PROP_BWA_P2MP = 184u,
		IF_TYPE_SONET_OVERHEAD_CHANNEL = 185u,
		IF_TYPE_DIGITAL_WRAPPER_OVERHEAD_CHANNEL = 186u,
		IF_TYPE_AAL2 = 187u,
		IF_TYPE_RADIO_MAC = 188u,
		IF_TYPE_ATM_RADIO = 189u,
		IF_TYPE_IMT = 190u,
		IF_TYPE_MVL = 191u,
		IF_TYPE_REACH_DSL = 192u,
		IF_TYPE_FR_DLCI_ENDPT = 193u,
		IF_TYPE_ATM_VCI_ENDPT = 194u,
		IF_TYPE_OPTICAL_CHANNEL = 195u,
		IF_TYPE_OPTICAL_TRANSPORT = 196u,
		IF_TYPE_IEEE80216_WMAN = 237u,
		IF_TYPE_WWANPP = 243u,
		IF_TYPE_WWANPP2 = 244u,
		IF_TYPE_IEEE802154 = 259u,
		IF_TYPE_XBOX_WIRELESS = 281u
	}

	internal enum NL_ROUTER_DISCOVERY_BEHAVIOR
	{
		RouterDiscoveryDisabled = 0,
		RouterDiscoveryEnabled = 1,
		RouterDiscoveryDhcp = 2,
		RouterDiscoveryUnchanged = -1
	}

	internal enum NL_LINK_LOCAL_ADDRESS_BEHAVIOR
	{
		LinkLocalAlwaysOff = 0,
		LinkLocalDelayed = 1,
		LinkLocalAlwaysOn = 2,
		LinkLocalUnchanged = -1
	}

	internal struct MIB_IPINTERFACE_ROW : IEquatable<MIB_IPINTERFACE_ROW>
	{
		public ADDRESS_FAMILY Family;

		public NET_LUID InterfaceLuid;

		public uint InterfaceIndex;

		public uint MaxReassemblySize;

		public ulong InterfaceIdentifier;

		public uint MinRouterAdvertisementInterval;

		public uint MaxRouterAdvertisementInterval;

		[MarshalAs(UnmanagedType.U1)]
		public bool AdvertisingEnabled;

		[MarshalAs(UnmanagedType.U1)]
		public bool ForwardingEnabled;

		[MarshalAs(UnmanagedType.U1)]
		public bool WeakHostSend;

		[MarshalAs(UnmanagedType.U1)]
		public bool WeakHostReceive;

		[MarshalAs(UnmanagedType.U1)]
		public bool UseAutomaticMetric;

		[MarshalAs(UnmanagedType.U1)]
		public bool UseNeighborUnreachabilityDetection;

		[MarshalAs(UnmanagedType.U1)]
		public bool ManagedAddressConfigurationSupported;

		[MarshalAs(UnmanagedType.U1)]
		public bool OtherStatefulConfigurationSupported;

		[MarshalAs(UnmanagedType.U1)]
		public bool AdvertiseDefaultRoute;

		public NL_ROUTER_DISCOVERY_BEHAVIOR RouterDiscoveryBehavior;

		public uint DadTransmits;

		public uint BaseReachableTime;

		public uint RetransmitTime;

		public uint PathMtuDiscoveryTimeout;

		public NL_LINK_LOCAL_ADDRESS_BEHAVIOR LinkLocalAddressBehavior;

		public uint LinkLocalAddressTimeout;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public uint[] ZoneIndices;

		public uint SitePrefixLength;

		public uint Metric;

		public uint NlMtu;

		[MarshalAs(UnmanagedType.U1)]
		public bool Connected;

		[MarshalAs(UnmanagedType.U1)]
		public bool SupportsWakeUpPatterns;

		[MarshalAs(UnmanagedType.U1)]
		public bool SupportsNeighborDiscovery;

		[MarshalAs(UnmanagedType.U1)]
		public bool SupportsRouterDiscovery;

		public uint ReachableTime;

		public NL_INTERFACE_OFFLOAD_ROD TransmitOffload;

		public NL_INTERFACE_OFFLOAD_ROD ReceiveOffload;

		[MarshalAs(UnmanagedType.U1)]
		public bool DisableDefaultRoutes;

		public MIB_IPINTERFACE_ROW(ADDRESS_FAMILY family, NET_LUID interfaceLuid)
		{
			this = default(MIB_IPINTERFACE_ROW);
			InitializeIpInterfaceEntry(out this);
			Family = family;
			InterfaceLuid = interfaceLuid;
		}

		public MIB_IPINTERFACE_ROW(ADDRESS_FAMILY family, uint interfaceIndex)
		{
			this = default(MIB_IPINTERFACE_ROW);
			InitializeIpInterfaceEntry(out this);
			Family = family;
			InterfaceIndex = interfaceIndex;
		}

		public bool Equals(MIB_IPINTERFACE_ROW other)
		{
			if (Family.Equals(other.Family))
			{
				if (InterfaceLuid.Value != other.InterfaceLuid.Value)
				{
					return InterfaceIndex == other.InterfaceIndex;
				}
				return true;
			}
			return false;
		}
	}

	internal enum ADDRESS_FAMILY : ushort
	{
		AF_UNSPEC = 0,
		AF_UNIX = 1,
		AF_INET = 2,
		AF_IMPLINK = 3,
		AF_PUP = 4,
		AF_CHAOS = 5,
		AF_NS = 6,
		AF_IPX = AF_NS,
		AF_ISO = 7,
		AF_OSI = AF_ISO,
		AF_ECMA = 8,
		AF_DATAKIT = 9,
		AF_CCITT = 10,
		AF_SNA = 11,
		AF_DECnet = 12,
		AF_DLI = 13,
		AF_LAT = 14,
		AF_HYLINK = 15,
		AF_APPLETALK = 16,
		AF_NETBIOS = 17,
		AF_VOICEVIEW = 18,
		AF_FIREFOX = 19,
		AF_UNKNOWN1 = 20,
		AF_BAN = 21,
		AF_ATM = 22,
		AF_INET6 = 23,
		AF_CLUSTER = 24,
		AF_12844 = 25,
		AF_IRDA = 26,
		AF_NETDES = 28,
		AF_TCNPROCESS = 29,
		AF_TCNMESSAGE = 30,
		AF_ICLFXBM = 31,
		AF_BTH = 32,
		AF_LINK = 33,
		AF_HYPERV = 34
	}

	internal struct NET_LUID(uint index, IFTYPE type)
	{
		public ulong Value = (index << 24) | ((ulong)type << 48);

		public uint NetLuidIndex
		{
			get
			{
				return (uint)((Value & 0xFFFFFF000000L) >> 24);
			}
			set
			{
				Value = (value << 24) | Value;
			}
		}

		public IFTYPE IfType
		{
			get
			{
				return (IFTYPE)((Value & 0xFFFF000000000000uL) >> 48);
			}
			set
			{
				Value = ((ulong)value << 48) | Value;
			}
		}

		public override string ToString()
		{
			return $"{NetLuidIndex}:{IfType}";
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 1)]
	internal struct NL_INTERFACE_OFFLOAD_ROD
	{
		[Flags]
		public enum SupportedFlags : byte
		{
			NlChecksumSupported = 1,
			NlOptionsSupported = 2,
			TlDatagramChecksumSupported = 4,
			TlStreamChecksumSupported = 8,
			TlStreamOptionsSupported = 0x10,
			FastPathCompatible = 0x20,
			TlLargeSendOffloadSupported = 0x40,
			TlGiantSendOffloadSupported = 0x80
		}

		public SupportedFlags Flags;

		public override string ToString()
		{
			return Flags.ToString();
		}
	}

	[DllImport("iphlpapi.dll", ExactSpelling = true)]
	public static extern uint GetIpInterfaceEntry(ref MIB_IPINTERFACE_ROW row);

	[DllImport("iphlpapi.dll", ExactSpelling = true)]
	public static extern void InitializeIpInterfaceEntry(out MIB_IPINTERFACE_ROW row);

	[DllImport("iphlpapi.dll", ExactSpelling = true)]
	public static extern uint SetIpInterfaceEntry(in MIB_IPINTERFACE_ROW row);
}
