namespace VpnSDK.Private.WFP.Interop;

internal static class WFPGuids
{
	public static readonly GUID FWPM_LAYER_INBOUND_IPPACKET_V4 = new GUID
	{
		Data1 = 3362771391u,
		Data2 = 8653,
		Data3 = 18814,
		Data4 = new byte[8] { 160, 187, 23, 66, 92, 136, 92, 88 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_IPPACKET_V4_DISCARD = new GUID
	{
		Data1 = 3047305424u,
		Data2 = 43200,
		Data3 = 17650,
		Data4 = new byte[8] { 145, 110, 153, 27, 83, 222, 209, 247 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_IPPACKET_V6 = new GUID
	{
		Data1 = 4112528075u,
		Data2 = 39196,
		Data3 = 18151,
		Data4 = new byte[8] { 151, 29, 38, 1, 69, 154, 145, 202 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_IPPACKET_V6_DISCARD = new GUID
	{
		Data1 = 3139748473u,
		Data2 = 37812,
		Data3 = 18338,
		Data4 = new byte[8] { 131, 173, 174, 22, 152, 181, 8, 133 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_IPPACKET_V4 = new GUID
	{
		Data1 = 509386670u,
		Data2 = 35460,
		Data3 = 16693,
		Data4 = new byte[8] { 163, 49, 149, 11, 84, 34, 158, 205 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_IPPACKET_V4_DISCARD = new GUID
	{
		Data1 = 149208245u,
		Data2 = 46663,
		Data3 = 18675,
		Data4 = new byte[8] { 149, 60, 229, 221, 189, 3, 147, 126 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_IPPACKET_V6 = new GUID
	{
		Data1 = 2746461035u,
		Data2 = 13668,
		Data3 = 18572,
		Data4 = new byte[8] { 145, 23, 243, 78, 130, 20, 39, 99 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_IPPACKET_V6_DISCARD = new GUID
	{
		Data1 = 2501105604u,
		Data2 = 43316,
		Data3 = 18908,
		Data4 = new byte[8] { 145, 167, 108, 203, 128, 204, 2, 227 }
	};

	public static readonly GUID FWPM_LAYER_IPFORWARD_V4 = new GUID
	{
		Data1 = 2821377060u,
		Data2 = 20193,
		Data3 = 20193,
		Data4 = new byte[8] { 180, 101, 253, 29, 37, 203, 16, 164 }
	};

	public static readonly GUID FWPM_LAYER_IPFORWARD_V4_DISCARD = new GUID
	{
		Data1 = 2661197683u,
		Data2 = 12206,
		Data3 = 16912,
		Data4 = new byte[8] { 143, 23, 52, 18, 158, 243, 105, 235 }
	};

	public static readonly GUID FWPM_LAYER_IPFORWARD_V6 = new GUID
	{
		Data1 = 2073446424u,
		Data2 = 6599,
		Data3 = 18746,
		Data4 = new byte[8] { 183, 31, 131, 44, 54, 132, 210, 140 }
	};

	public static readonly GUID FWPM_LAYER_IPFORWARD_V6_DISCARD = new GUID
	{
		Data1 = 827476573u,
		Data2 = 7678,
		Data3 = 18223,
		Data4 = new byte[8] { 187, 147, 81, 142, 233, 69, 216, 162 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_TRANSPORT_V4 = new GUID
	{
		Data1 = 1495719880u,
		Data2 = 58319,
		Data3 = 17446,
		Data4 = new byte[8] { 162, 131, 220, 57, 63, 93, 15, 157 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_TRANSPORT_V4_DISCARD = new GUID
	{
		Data1 = 2890569779u,
		Data2 = 63133,
		Data3 = 17992,
		Data4 = new byte[8] { 178, 97, 109, 200, 72, 53, 239, 57 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_TRANSPORT_V6 = new GUID
	{
		Data1 = 1665828511u,
		Data2 = 64547,
		Data3 = 19344,
		Data4 = new byte[8] { 176, 193, 191, 98, 10, 54, 174, 111 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_TRANSPORT_V6_DISCARD = new GUID
	{
		Data1 = 711981397u,
		Data2 = 15147,
		Data3 = 18898,
		Data4 = new byte[8] { 152, 72, 173, 157, 114, 220, 170, 183 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_TRANSPORT_V4 = new GUID
	{
		Data1 = 166075114u,
		Data2 = 53780,
		Data3 = 18146,
		Data4 = new byte[8] { 155, 33, 178, 107, 11, 47, 40, 200 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_TRANSPORT_V4_DISCARD = new GUID
	{
		Data1 = 3320907089u,
		Data2 = 48560,
		Data3 = 17367,
		Data4 = new byte[8] { 163, 19, 80, 226, 17, 244, 214, 138 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_TRANSPORT_V6 = new GUID
	{
		Data1 = 3782433758u,
		Data2 = 319,
		Data3 = 18005,
		Data4 = new byte[8] { 179, 81, 164, 158, 21, 118, 45, 240 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_TRANSPORT_V6_DISCARD = new GUID
	{
		Data1 = 4097040233u,
		Data2 = 52413,
		Data3 = 18478,
		Data4 = new byte[8] { 185, 178, 87, 22, 86, 88, 195, 179 }
	};

	public static readonly GUID FWPM_LAYER_STREAM_V4 = new GUID
	{
		Data1 = 998860092u,
		Data2 = 49520,
		Data3 = 18916,
		Data4 = new byte[8] { 177, 205, 224, 238, 238, 225, 154, 62 }
	};

	public static readonly GUID FWPM_LAYER_STREAM_V4_DISCARD = new GUID
	{
		Data1 = 633651906u,
		Data2 = 9727,
		Data3 = 17234,
		Data4 = new byte[8] { 130, 249, 197, 74, 74, 71, 38, 220 }
	};

	public static readonly GUID FWPM_LAYER_STREAM_V6 = new GUID
	{
		Data1 = 1204360058u,
		Data2 = 32452,
		Data3 = 18099,
		Data4 = new byte[8] { 182, 228, 72, 233, 38, 177, 237, 164 }
	};

	public static readonly GUID FWPM_LAYER_STREAM_V6_DISCARD = new GUID
	{
		Data1 = 279289799u,
		Data2 = 46632,
		Data3 = 19521,
		Data4 = new byte[8] { 158, 184, 207, 55, 213, 81, 3, 207 }
	};

	public static readonly GUID FWPM_LAYER_DATAGRAM_DATA_V4 = new GUID
	{
		Data1 = 1023983438u,
		Data2 = 17910,
		Data3 = 18736,
		Data4 = new byte[8] { 169, 34, 65, 112, 152, 226, 0, 39 }
	};

	public static readonly GUID FWPM_LAYER_DATAGRAM_DATA_V4_DISCARD = new GUID
	{
		Data1 = 417542342u,
		Data2 = 29256,
		Data3 = 20050,
		Data4 = new byte[8] { 170, 171, 71, 46, 214, 119, 4, 253 }
	};

	public static readonly GUID FWPM_LAYER_DATAGRAM_DATA_V6 = new GUID
	{
		Data1 = 4198891055u,
		Data2 = 15546,
		Data3 = 17447,
		Data4 = new byte[8] { 135, 252, 87, 185, 164, 177, 13, 0 }
	};

	public static readonly GUID FWPM_LAYER_DATAGRAM_DATA_V6_DISCARD = new GUID
	{
		Data1 = 164749281u,
		Data2 = 39814,
		Data3 = 19010,
		Data4 = new byte[8] { 190, 157, 140, 49, 91, 146, 165, 208 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_ICMP_ERROR_V4 = new GUID
	{
		Data1 = 1632213392u,
		Data2 = 15542,
		Data3 = 20100,
		Data4 = new byte[8] { 185, 80, 83, 185, 75, 105, 100, 243 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_ICMP_ERROR_V4_DISCARD = new GUID
	{
		Data1 = 2796646517u,
		Data2 = 60335,
		Data3 = 16467,
		Data4 = new byte[8] { 164, 231, 33, 60, 129, 33, 237, 229 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_ICMP_ERROR_V6 = new GUID
	{
		Data1 = 1710865919u,
		Data2 = 15149,
		Data3 = 20061,
		Data4 = new byte[8] { 184, 198, 199, 32, 101, 31, 232, 152 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_ICMP_ERROR_V6_DISCARD = new GUID
	{
		Data1 = 2800209088u,
		Data2 = 2299,
		Data3 = 18061,
		Data4 = new byte[8] { 164, 114, 151, 113, 213, 89, 94, 9 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_ICMP_ERROR_V4 = new GUID
	{
		Data1 = 1094254848u,
		Data2 = 22092,
		Data3 = 19250,
		Data4 = new byte[8] { 188, 29, 113, 128, 72, 53, 77, 124 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_ICMP_ERROR_V4_DISCARD = new GUID
	{
		Data1 = 3008990518u,
		Data2 = 1377,
		Data3 = 17800,
		Data4 = new byte[8] { 166, 191, 233, 85, 227, 246, 38, 75 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_ICMP_ERROR_V6 = new GUID
	{
		Data1 = 2142255968u,
		Data2 = 31629,
		Data3 = 19962,
		Data4 = new byte[8] { 186, 221, 152, 1, 118, 252, 78, 18 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_ICMP_ERROR_V6_DISCARD = new GUID
	{
		Data1 = 1710417479u,
		Data2 = 36108,
		Data3 = 20295,
		Data4 = new byte[8] { 177, 155, 51, 164, 211, 241, 53, 124 }
	};

	public static readonly GUID FWPM_LAYER_ALE_RESOURCE_ASSIGNMENT_V4 = new GUID
	{
		Data1 = 306697837u,
		Data2 = 2912,
		Data3 = 18965,
		Data4 = new byte[8] { 141, 68, 113, 85, 208, 245, 58, 12 }
	};

	public static readonly GUID FWPM_LAYER_ALE_RESOURCE_ASSIGNMENT_V4_DISCARD = new GUID
	{
		Data1 = 190321314u,
		Data2 = 50175,
		Data3 = 20170,
		Data4 = new byte[8] { 184, 141, 199, 158, 32, 172, 99, 34 }
	};

	public static readonly GUID FWPM_LAYER_ALE_RESOURCE_ASSIGNMENT_V6 = new GUID
	{
		Data1 = 1436963041u,
		Data2 = 24330,
		Data3 = 20170,
		Data4 = new byte[8] { 166, 83, 136, 245, 59, 38, 170, 140 }
	};

	public static readonly GUID FWPM_LAYER_ALE_RESOURCE_ASSIGNMENT_V6_DISCARD = new GUID
	{
		Data1 = 3418986683u,
		Data2 = 50463,
		Data3 = 19482,
		Data4 = new byte[8] { 187, 79, 151, 117, 252, 172, 171, 47 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_LISTEN_V4 = new GUID
	{
		Data1 = 2293980589u,
		Data2 = 30423,
		Data3 = 16935,
		Data4 = new byte[8] { 156, 113, 223, 10, 62, 215, 190, 126 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_LISTEN_V4_DISCARD = new GUID
	{
		Data1 = 924711642u,
		Data2 = 40742,
		Data3 = 17917,
		Data4 = new byte[8] { 180, 235, 194, 158, 178, 18, 137, 63 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_LISTEN_V6 = new GUID
	{
		Data1 = 2060049956u,
		Data2 = 6109,
		Data3 = 18452,
		Data4 = new byte[8] { 180, 189, 169, 251, 201, 90, 50, 27 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_LISTEN_V6_DISCARD = new GUID
	{
		Data1 = 1617967879u,
		Data2 = 25544,
		Data3 = 18665,
		Data4 = new byte[8] { 173, 163, 18, 177, 175, 64, 166, 23 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4 = new GUID
	{
		Data1 = 3788349415u,
		Data2 = 62645,
		Data3 = 17011,
		Data4 = new byte[8] { 150, 192, 89, 46, 72, 123, 134, 80 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4_DISCARD = new GUID
	{
		Data1 = 2666178971u,
		Data2 = 48418,
		Data3 = 16935,
		Data4 = new byte[8] { 145, 159, 0, 115, 198, 51, 87, 177 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6 = new GUID
	{
		Data1 = 2746494103u,
		Data2 = 40708,
		Data3 = 18034,
		Data4 = new byte[8] { 184, 126, 206, 233, 196, 131, 37, 127 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6_DISCARD = new GUID
	{
		Data1 = 2303024023u,
		Data2 = 56289,
		Data3 = 17727,
		Data4 = new byte[8] { 162, 36, 19, 218, 137, 90, 243, 150 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_CONNECT_V4 = new GUID
	{
		Data1 = 3280820177u,
		Data2 = 1447,
		Data3 = 19507,
		Data4 = new byte[8] { 144, 79, 127, 188, 238, 230, 14, 130 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_CONNECT_V4_DISCARD = new GUID
	{
		Data1 = 3593644033u,
		Data2 = 62906,
		Data3 = 19158,
		Data4 = new byte[8] { 150, 227, 96, 112, 23, 217, 131, 106 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_CONNECT_V6 = new GUID
	{
		Data1 = 1248999739u,
		Data2 = 12703,
		Data3 = 17596,
		Data4 = new byte[8] { 132, 195, 186, 84, 220, 179, 182, 180 }
	};

	public static readonly GUID FWPM_LAYER_ALE_AUTH_CONNECT_V6_DISCARD = new GUID
	{
		Data1 = 3380331448u,
		Data2 = 51619,
		Data3 = 20019,
		Data4 = new byte[8] { 134, 149, 142, 23, 170, 212, 222, 9 }
	};

	public static readonly GUID FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4 = new GUID
	{
		Data1 = 2944419594u,
		Data2 = 21910,
		Data3 = 19475,
		Data4 = new byte[8] { 153, 146, 83, 158, 111, 229, 121, 103 }
	};

	public static readonly GUID FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4_DISCARD = new GUID
	{
		Data1 = 342549673u,
		Data2 = 41426,
		Data3 = 19779,
		Data4 = new byte[8] { 163, 26, 76, 66, 104, 43, 142, 79 }
	};

	public static readonly GUID FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6 = new GUID
	{
		Data1 = 1881264819u,
		Data2 = 57252,
		Data3 = 16494,
		Data4 = new byte[8] { 175, 235, 106, 250, 247, 231, 14, 253 }
	};

	public static readonly GUID FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6_DISCARD = new GUID
	{
		Data1 = 1184007734u,
		Data2 = 48074,
		Data3 = 19318,
		Data4 = new byte[8] { 148, 29, 15, 167, 245, 215, 211, 114 }
	};

	public static readonly GUID FWPM_LAYER_INBOUND_MAC_FRAME_802_3 = new GUID
	{
		Data1 = 4026236635u,
		Data2 = 85,
		Data3 = 20378,
		Data4 = new byte[8] { 162, 49, 79, 248, 19, 26, 209, 145 }
	};

	public static readonly GUID FWPM_LAYER_OUTBOUND_MAC_FRAME_802_3 = new GUID
	{
		Data1 = 1766224828u,
		Data2 = 55003,
		Data3 = 18544,
		Data4 = new byte[8] { 173, 238, 10, 205, 189, 183, 244, 178 }
	};

	public static readonly GUID FWPM_LAYER_IPSEC_KM_DEMUX_V4 = new GUID
	{
		Data1 = 4029355302u,
		Data2 = 42073,
		Data3 = 19025,
		Data4 = new byte[8] { 185, 227, 117, 157, 229, 43, 157, 44 }
	};

	public static readonly GUID FWPM_LAYER_IPSEC_KM_DEMUX_V6 = new GUID
	{
		Data1 = 796220662u,
		Data2 = 12244,
		Data3 = 20104,
		Data4 = new byte[8] { 179, 228, 169, 27, 202, 73, 82, 53 }
	};

	public static readonly GUID FWPM_LAYER_IPSEC_V4 = new GUID
	{
		Data1 = 3987102836u,
		Data2 = 24845,
		Data3 = 19397,
		Data4 = new byte[8] { 148, 143, 60, 79, 137, 85, 104, 103 }
	};

	public static readonly GUID FWPM_LAYER_IPSEC_V6 = new GUID
	{
		Data1 = 331646018u,
		Data2 = 36231,
		Data3 = 16993,
		Data4 = new byte[8] { 154, 41, 89, 210, 171, 195, 72, 180 }
	};

	public static readonly GUID FWPM_LAYER_IKEEXT_V4 = new GUID
	{
		Data1 = 2974514139u,
		Data2 = 56253,
		Data3 = 18238,
		Data4 = new byte[8] { 190, 212, 139, 71, 8, 212, 242, 112 }
	};

	public static readonly GUID FWPM_LAYER_IKEEXT_V6 = new GUID
	{
		Data1 = 3058140851u,
		Data2 = 63111,
		Data3 = 20153,
		Data4 = new byte[8] { 137, 210, 142, 243, 42, 205, 171, 226 }
	};

	public static readonly GUID FWPM_LAYER_RPC_UM = new GUID
	{
		Data1 = 1973984730u,
		Data2 = 38372,
		Data3 = 16627,
		Data4 = new byte[8] { 173, 199, 118, 136, 169, 200, 71, 225 }
	};

	public static readonly GUID FWPM_LAYER_RPC_EPMAP = new GUID
	{
		Data1 = 2454174817u,
		Data2 = 60167,
		Data3 = 18414,
		Data4 = new byte[8] { 135, 44, 191, 215, 139, 253, 22, 22 }
	};

	public static readonly GUID FWPM_LAYER_RPC_EP_ADD = new GUID
	{
		Data1 = 1636696007u,
		Data2 = 50256,
		Data3 = 18755,
		Data4 = new byte[8] { 149, 219, 153, 180, 193, 106, 85, 212 }
	};

	public static readonly GUID FWPM_LAYER_RPC_PROXY_CONN = new GUID
	{
		Data1 = 2493822219u,
		Data2 = 47708,
		Data3 = 20263,
		Data4 = new byte[8] { 144, 122, 34, 159, 172, 12, 42, 122 }
	};

	public static readonly GUID FWPM_LAYER_RPC_PROXY_IF = new GUID
	{
		Data1 = 4171466261u,
		Data2 = 57644,
		Data3 = 16812,
		Data4 = new byte[8] { 152, 223, 18, 26, 217, 129, 170, 222 }
	};

	public static readonly GUID FWPM_LAYER_KM_AUTHORIZATION = new GUID
	{
		Data1 = 1252140777u,
		Data2 = 36896,
		Data3 = 17915,
		Data4 = new byte[8] { 149, 106, 192, 36, 157, 132, 17, 149 }
	};

	public static readonly GUID FWPM_LAYER_NAME_RESOLUTION_CACHE_V4 = new GUID
	{
		Data1 = 204121729u,
		Data2 = 36955,
		Data3 = 19661,
		Data4 = new byte[8] { 164, 103, 77, 216, 17, 208, 123, 123 }
	};

	public static readonly GUID FWPM_LAYER_NAME_RESOLUTION_CACHE_V6 = new GUID
	{
		Data1 = 2463470330u,
		Data2 = 27393,
		Data3 = 17226,
		Data4 = new byte[8] { 157, 234, 209, 233, 110, 169, 125, 169 }
	};

	public static readonly GUID FWPM_LAYER_ALE_RESOURCE_RELEASE_V4 = new GUID
	{
		Data1 = 1949719758u,
		Data2 = 52400,
		Data3 = 16410,
		Data4 = new byte[8] { 191, 193, 184, 153, 52, 173, 126, 21 }
	};

	public static readonly GUID FWPM_LAYER_ALE_RESOURCE_RELEASE_V6 = new GUID
	{
		Data1 = 4108701312u,
		Data2 = 60876,
		Data3 = 19987,
		Data4 = new byte[8] { 138, 47, 185, 20, 84, 187, 5, 123 }
	};

	public static readonly GUID FWPM_LAYER_ALE_ENDPOINT_CLOSURE_V4 = new GUID
	{
		Data1 = 3027657767u,
		Data2 = 58018,
		Data3 = 18042,
		Data4 = new byte[8] { 189, 126, 219, 205, 27, 216, 90, 9 }
	};

	public static readonly GUID FWPM_LAYER_ALE_ENDPOINT_CLOSURE_V6 = new GUID
	{
		Data1 = 3142806733u,
		Data2 = 18261,
		Data3 = 19369,
		Data4 = new byte[8] { 159, 247, 249, 237, 248, 105, 156, 123 }
	};

	public static readonly GUID FWPM_LAYER_ALE_CONNECT_REDIRECT_V4 = new GUID
	{
		Data1 = 3336977548u,
		Data2 = 46980,
		Data3 = 17762,
		Data4 = new byte[8] { 170, 125, 10, 103, 207, 202, 249, 163 }
	};

	public static readonly GUID FWPM_LAYER_ALE_CONNECT_REDIRECT_V6 = new GUID
	{
		Data1 = 1484674215u,
		Data2 = 32838,
		Data3 = 17082,
		Data4 = new byte[8] { 160, 170, 183, 22, 37, 15, 199, 253 }
	};

	public static readonly GUID FWPM_LAYER_ALE_BIND_REDIRECT_V4 = new GUID
	{
		Data1 = 1721207981u,
		Data2 = 50948,
		Data3 = 17068,
		Data4 = new byte[8] { 134, 172, 124, 26, 35, 27, 210, 83 }
	};

	public static readonly GUID FWPM_LAYER_ALE_BIND_REDIRECT_V6 = new GUID
	{
		Data1 = 3203411100u,
		Data2 = 24683,
		Data3 = 17718,
		Data4 = new byte[8] { 140, 38, 28, 47, 199, 182, 49, 212 }
	};

	public static readonly GUID FWPM_LAYER_STREAM_PACKET_V4 = new GUID
	{
		Data1 = 2941442284u,
		Data2 = 52013,
		Data3 = 17637,
		Data4 = new byte[8] { 173, 146, 248, 220, 56, 210, 235, 41 }
	};

	public static readonly GUID FWPM_LAYER_STREAM_PACKET_V6 = new GUID
	{
		Data1 = 2006617251u,
		Data2 = 61593,
		Data3 = 18063,
		Data4 = new byte[8] { 181, 212, 131, 83, 92, 70, 28, 2 }
	};

	public static readonly GUID FWPM_SUBLAYER_RPC_AUDIT = new GUID
	{
		Data1 = 1972143348u,
		Data2 = 64328,
		Data3 = 19945,
		Data4 = new byte[8] { 154, 235, 62, 217, 85, 26, 177, 253 }
	};

	public static readonly GUID FWPM_SUBLAYER_IPSEC_TUNNEL = new GUID
	{
		Data1 = 2213714413u,
		Data2 = 40948,
		Data3 = 18791,
		Data4 = new byte[8] { 175, 244, 195, 9, 244, 218, 184, 39 }
	};

	public static readonly GUID FWPM_SUBLAYER_UNIVERSAL = new GUID
	{
		Data1 = 4005481475u,
		Data2 = 52948,
		Data3 = 17280,
		Data4 = new byte[8] { 129, 154, 39, 52, 57, 123, 43, 116 }
	};

	public static readonly GUID FWPM_SUBLAYER_LIPS = new GUID
	{
		Data1 = 460701902u,
		Data2 = 65376,
		Data3 = 18193,
		Data4 = new byte[8] { 167, 15, 180, 149, 140, 195, 178, 208 }
	};

	public static readonly GUID FWPM_SUBLAYER_SECURE_SOCKET = new GUID
	{
		Data1 = 363228695u,
		Data2 = 16188,
		Data3 = 20347,
		Data4 = new byte[8] { 170, 108, 129, 42, 166, 19, 221, 130 }
	};

	public static readonly GUID FWPM_SUBLAYER_TCP_CHIMNEY_OFFLOAD = new GUID
	{
		Data1 = 863373497u,
		Data2 = 47061,
		Data3 = 19807,
		Data4 = new byte[8] { 130, 249, 54, 24, 97, 139, 192, 88 }
	};

	public static readonly GUID FWPM_SUBLAYER_INSPECTION = new GUID
	{
		Data1 = 2272598497u,
		Data2 = 59049,
		Data3 = 16805,
		Data4 = new byte[8] { 129, 180, 140, 79, 17, 142, 74, 96 }
	};

	public static readonly GUID FWPM_SUBLAYER_TEREDO = new GUID
	{
		Data1 = 3127499878u,
		Data2 = 20854,
		Data3 = 18809,
		Data4 = new byte[8] { 156, 137, 38, 167, 180, 106, 131, 39 }
	};

	public static readonly GUID FWPM_SUBLAYER_IPSEC_FORWARD_OUTBOUND_TUNNEL = new GUID
	{
		Data1 = 2768776819u,
		Data2 = 36721,
		Data3 = 17753,
		Data4 = new byte[8] { 138, 154, 16, 28, 234, 4, 239, 135 }
	};

	public static readonly GUID FWPM_SUBLAYER_IPSEC_DOSP = new GUID
	{
		Data1 = 3765884274u,
		Data2 = 23869,
		Data3 = 18671,
		Data4 = new byte[8] { 128, 43, 144, 158, 221, 176, 152, 189 }
	};

	public static readonly GUID FWPM_CONDITION_ETHER_DESTINATION_ADDRESS = new GUID
	{
		Data1 = 3650742657u,
		Data2 = 31048,
		Data3 = 19587,
		Data4 = new byte[8] { 183, 66, 200, 78, 59, 103, 143, 143 }
	};

	public static readonly GUID FWPM_CONDITION_ETHER_SOURCE_ADDRESS = new GUID
	{
		Data1 = 1083125460u,
		Data2 = 14960,
		Data3 = 19277,
		Data4 = new byte[8] { 146, 166, 65, 90, 194, 14, 47, 18 }
	};

	public static readonly GUID FWPM_CONDITION_ETHER_ADDRESS_TYPE = new GUID
	{
		Data1 = 2905230864u,
		Data2 = 59881,
		Data3 = 20007,
		Data4 = new byte[8] { 156, 250, 253, 62, 93, 24, 76, 17 }
	};

	public static readonly GUID FWPM_CONDITION_ETHER_ENCAP_METHOD = new GUID
	{
		Data1 = 2744013289u,
		Data2 = 2757,
		Data3 = 17643,
		Data4 = new byte[8] { 147, 135, 161, 199, 91, 87, 110, 130 }
	};

	public static readonly GUID FWPM_CONDITION_ETHER_TYPE = new GUID
	{
		Data1 = 4245197965u,
		Data2 = 41497,
		Data3 = 19794,
		Data4 = new byte[8] { 187, 152, 26, 85, 64, 238, 123, 78 }
	};

	public static readonly GUID FWPM_CONDITION_ETHER_SNAP_CONTROL = new GUID
	{
		Data1 = 3294581633u,
		Data2 = 3247,
		Data3 = 18384,
		Data4 = new byte[8] { 185, 108, 35, 138, 203, 23, 128, 107 }
	};

	public static readonly GUID FWPM_CONDITION_ETHER_SNAP_OUI = new GUID
	{
		Data1 = 2939630382u,
		Data2 = 55260,
		Data3 = 19049,
		Data4 = new byte[8] { 159, 78, 61, 104, 58, 183, 54, 91 }
	};

	public static readonly GUID FWPM_CONDITION_ETHER_VLAN_TAG = new GUID
	{
		Data1 = 2475600673u,
		Data2 = 13848,
		Data3 = 20068,
		Data4 = new byte[8] { 156, 165, 33, 65, 235, 218, 28, 162 }
	};

	public static readonly GUID FWPM_CONDITION_IP_LOCAL_ADDRESS = new GUID
	{
		Data1 = 3656253662u,
		Data2 = 49647,
		Data3 = 17943,
		Data4 = new byte[8] { 191, 227, 255, 216, 245, 160, 137, 87 }
	};

	public static readonly GUID FWPM_CONDITION_IP_REMOTE_ADDRESS = new GUID
	{
		Data1 = 2989862554u,
		Data2 = 7524,
		Data3 = 18872,
		Data4 = new byte[8] { 164, 76, 95, 243, 217, 9, 80, 69 }
	};

	public static readonly GUID FWPM_CONDITION_IP_SOURCE_ADDRESS = new GUID
	{
		Data1 = 2929101182u,
		Data2 = 11924,
		Data3 = 19401,
		Data4 = new byte[8] { 179, 19, 178, 126, 232, 14, 87, 77 }
	};

	public static readonly GUID FWPM_CONDITION_IP_DESTINATION_ADDRESS = new GUID
	{
		Data1 = 762909499u,
		Data2 = 45968,
		Data3 = 17862,
		Data4 = new byte[8] { 134, 153, 172, 172, 234, 175, 237, 51 }
	};

	public static readonly GUID FWPM_CONDITION_IP_LOCAL_ADDRESS_TYPE = new GUID
	{
		Data1 = 1858598596u,
		Data2 = 14187,
		Data3 = 17879,
		Data4 = new byte[8] { 158, 156, 211, 55, 206, 220, 210, 55 }
	};

	public static readonly GUID FWPM_CONDITION_IP_DESTINATION_ADDRESS_TYPE = new GUID
	{
		Data1 = 516011977u,
		Data2 = 20202,
		Data3 = 20318,
		Data4 = new byte[8] { 185, 239, 118, 190, 170, 175, 23, 238 }
	};

	public static readonly GUID FWPM_CONDITION_IP_NEXTHOP_ADDRESS = new GUID
	{
		Data1 = 3938337930u,
		Data2 = 42769,
		Data3 = 19812,
		Data4 = new byte[8] { 133, 183, 63, 118, 182, 82, 153, 199 }
	};

	public static readonly GUID FWPM_CONDITION_IP_LOCAL_INTERFACE = new GUID
	{
		Data1 = 1289103945u,
		Data2 = 22979,
		Data3 = 18793,
		Data4 = new byte[8] { 183, 243, 189, 165, 211, 40, 144, 164 }
	};

	public static readonly GUID FWPM_CONDITION_IP_ARRIVAL_INTERFACE = new GUID
	{
		Data1 = 1636473709u,
		Data2 = 14443,
		Data3 = 16694,
		Data4 = new byte[8] { 173, 110, 181, 21, 135, 207, 177, 205 }
	};

	public static readonly GUID FWPM_CONDITION_ARRIVAL_INTERFACE_TYPE = new GUID
	{
		Data1 = 2314834142u,
		Data2 = 59288,
		Data3 = 20077,
		Data4 = new byte[8] { 171, 118, 124, 149, 88, 41, 46, 111 }
	};

	public static readonly GUID FWPM_CONDITION_ARRIVAL_TUNNEL_TYPE = new GUID
	{
		Data1 = 1360094940u,
		Data2 = 31372,
		Data3 = 19111,
		Data4 = new byte[8] { 181, 51, 149, 171, 89, 251, 3, 64 }
	};

	public static readonly GUID FWPM_CONDITION_ARRIVAL_INTERFACE_INDEX = new GUID
	{
		Data1 = 3423112627u,
		Data2 = 6034,
		Data3 = 19057,
		Data4 = new byte[8] { 176, 249, 3, 125, 33, 205, 130, 139 }
	};

	public static readonly GUID FWPM_CONDITION_NEXTHOP_SUB_INTERFACE_INDEX = new GUID
	{
		Data1 = 4018823458u,
		Data2 = 1399,
		Data3 = 17831,
		Data4 = new byte[8] { 154, 175, 130, 95, 190, 180, 251, 149 }
	};

	public static readonly GUID FWPM_CONDITION_IP_NEXTHOP_INTERFACE = new GUID
	{
		Data1 = 2477690715u,
		Data2 = 32623,
		Data3 = 18201,
		Data4 = new byte[8] { 152, 200, 20, 233, 116, 41, 239, 4 }
	};

	public static readonly GUID FWPM_CONDITION_NEXTHOP_INTERFACE_TYPE = new GUID
	{
		Data1 = 2538830956u,
		Data2 = 55715,
		Data3 = 18279,
		Data4 = new byte[8] { 163, 129, 233, 66, 103, 92, 217, 32 }
	};

	public static readonly GUID FWPM_CONDITION_NEXTHOP_TUNNEL_TYPE = new GUID
	{
		Data1 = 1924243729u,
		Data2 = 39035,
		Data3 = 18208,
		Data4 = new byte[8] { 153, 221, 199, 197, 118, 250, 45, 76 }
	};

	public static readonly GUID FWPM_CONDITION_NEXTHOP_INTERFACE_INDEX = new GUID
	{
		Data1 = 328099976u,
		Data2 = 31416,
		Data3 = 19813,
		Data4 = new byte[8] { 158, 232, 5, 145, 188, 246, 164, 148 }
	};

	public static readonly GUID FWPM_CONDITION_ORIGINAL_PROFILE_ID = new GUID
	{
		Data1 = 1189746001u,
		Data2 = 8789,
		Data3 = 18731,
		Data4 = new byte[8] { 128, 25, 170, 190, 238, 52, 159, 64 }
	};

	public static readonly GUID FWPM_CONDITION_CURRENT_PROFILE_ID = new GUID
	{
		Data1 = 2872062921u,
		Data2 = 49379,
		Data3 = 18265,
		Data4 = new byte[8] { 147, 125, 87, 88, 198, 93, 74, 227 }
	};

	public static readonly GUID FWPM_CONDITION_LOCAL_INTERFACE_PROFILE_ID = new GUID
	{
		Data1 = 1321170274u,
		Data2 = 40728,
		Data3 = 19718,
		Data4 = new byte[8] { 153, 65, 167, 166, 37, 116, 77, 113 }
	};

	public static readonly GUID FWPM_CONDITION_ARRIVAL_INTERFACE_PROFILE_ID = new GUID
	{
		Data1 = 3456002731u,
		Data2 = 49283,
		Data3 = 16706,
		Data4 = new byte[8] { 134, 121, 192, 143, 149, 50, 156, 97 }
	};

	public static readonly GUID FWPM_CONDITION_NEXTHOP_INTERFACE_PROFILE_ID = new GUID
	{
		Data1 = 3623852630u,
		Data2 = 52650,
		Data3 = 18219,
		Data4 = new byte[8] { 132, 219, 210, 57, 99, 193, 209, 191 }
	};

	public static readonly GUID FWPM_CONDITION_REAUTHORIZE_REASON = new GUID
	{
		Data1 = 287334028u,
		Data2 = 4526,
		Data3 = 17786,
		Data4 = new byte[8] { 138, 68, 71, 112, 38, 221, 118, 74 }
	};

	public static readonly GUID FWPM_CONDITION_ORIGINAL_ICMP_TYPE = new GUID
	{
		Data1 = 124648894u,
		Data2 = 50540,
		Data3 = 20338,
		Data4 = new byte[8] { 174, 138, 44, 254, 126, 92, 130, 134 }
	};

	public static readonly GUID FWPM_CONDITION_IP_PHYSICAL_ARRIVAL_INTERFACE = new GUID
	{
		Data1 = 3662730696u,
		Data2 = 64013,
		Data3 = 19593,
		Data4 = new byte[8] { 176, 50, 110, 98, 19, 109, 30, 150 }
	};

	public static readonly GUID FWPM_CONDITION_IP_PHYSICAL_NEXTHOP_INTERFACE = new GUID
	{
		Data1 = 4036744654u,
		Data2 = 20816,
		Data3 = 18622,
		Data4 = new byte[8] { 176, 152, 194, 81, 82, 251, 31, 146 }
	};

	public static readonly GUID FWPM_CONDITION_INTERFACE_QUARANTINE_EPOCH = new GUID
	{
		Data1 = 3437661534u,
		Data2 = 1339,
		Data3 = 17320,
		Data4 = new byte[8] { 154, 111, 51, 56, 76, 40, 228, 246 }
	};

	public static readonly GUID FWPM_CONDITION_INTERFACE_TYPE = new GUID
	{
		Data1 = 3673738516u,
		Data2 = 57502,
		Data3 = 19603,
		Data4 = new byte[8] { 165, 174, 197, 193, 59, 115, 255, 202 }
	};

	public static readonly GUID FWPM_CONDITION_TUNNEL_TYPE = new GUID
	{
		Data1 = 2007237687u,
		Data2 = 34681,
		Data3 = 18536,
		Data4 = new byte[8] { 162, 97, 245, 169, 2, 241, 192, 205 }
	};

	public static readonly GUID FWPM_CONDITION_IP_FORWARD_INTERFACE = new GUID
	{
		Data1 = 276215973u,
		Data2 = 25379,
		Data3 = 19550,
		Data4 = new byte[8] { 152, 16, 232, 211, 252, 158, 97, 54 }
	};

	public static readonly GUID FWPM_CONDITION_IP_PROTOCOL = new GUID
	{
		Data1 = 963768107u,
		Data2 = 25150,
		Data3 = 20378,
		Data4 = new byte[8] { 140, 177, 110, 121, 184, 6, 185, 167 }
	};

	public static readonly GUID FWPM_CONDITION_IP_LOCAL_PORT = new GUID
	{
		Data1 = 203137455u,
		Data2 = 22373,
		Data3 = 17727,
		Data4 = new byte[8] { 175, 34, 168, 247, 145, 172, 119, 91 }
	};

	public static readonly GUID FWPM_CONDITION_IP_REMOTE_PORT = new GUID
	{
		Data1 = 3277480013u,
		Data2 = 53803,
		Data3 = 19994,
		Data4 = new byte[8] { 145, 180, 104, 246, 116, 238, 103, 75 }
	};

	public static readonly GUID FWPM_CONDITION_EMBEDDED_LOCAL_ADDRESS_TYPE = new GUID
	{
		Data1 = 1181918312u,
		Data2 = 35338,
		Data3 = 16898,
		Data4 = new byte[8] { 171, 180, 132, 158, 146, 230, 104, 9 }
	};

	public static readonly GUID FWPM_CONDITION_EMBEDDED_REMOTE_ADDRESS = new GUID
	{
		Data1 = 2012105529u,
		Data2 = 12915,
		Data3 = 18033,
		Data4 = new byte[8] { 182, 59, 171, 111, 235, 102, 238, 182 }
	};

	public static readonly GUID FWPM_CONDITION_EMBEDDED_PROTOCOL = new GUID
	{
		Data1 = 125321479u,
		Data2 = 41630,
		Data3 = 19579,
		Data4 = new byte[8] { 158, 199, 41, 196, 74, 250, 253, 188 }
	};

	public static readonly GUID FWPM_CONDITION_EMBEDDED_LOCAL_PORT = new GUID
	{
		Data1 = 3217701197u,
		Data2 = 44251,
		Data3 = 18510,
		Data4 = new byte[8] { 184, 230, 42, 255, 121, 117, 115, 69 }
	};

	public static readonly GUID FWPM_CONDITION_EMBEDDED_REMOTE_PORT = new GUID
	{
		Data1 = 3403994785u,
		Data2 = 10600,
		Data3 = 16621,
		Data4 = new byte[8] { 164, 206, 84, 113, 96, 221, 168, 141 }
	};

	public static readonly GUID FWPM_CONDITION_FLAGS = new GUID
	{
		Data1 = 1663885883u,
		Data2 = 20839,
		Data3 = 17244,
		Data4 = new byte[8] { 134, 215, 233, 3, 104, 74, 168, 12 }
	};

	public static readonly GUID FWPM_CONDITION_DIRECTION = new GUID
	{
		Data1 = 2273624390u,
		Data2 = 51863,
		Data3 = 17622,
		Data4 = new byte[8] { 159, 209, 25, 251, 24, 64, 203, 247 }
	};

	public static readonly GUID FWPM_CONDITION_INTERFACE_INDEX = new GUID
	{
		Data1 = 1719654229u,
		Data2 = 54933,
		Data3 = 17226,
		Data4 = new byte[8] { 138, 245, 211, 131, 90, 18, 89, 188 }
	};

	public static readonly GUID FWPM_CONDITION_SUB_INTERFACE_INDEX = new GUID
	{
		Data1 = 215229555u,
		Data2 = 54817,
		Data3 = 19427,
		Data4 = new byte[8] { 174, 140, 114, 163, 72, 210, 131, 225 }
	};

	public static readonly GUID FWPM_CONDITION_SOURCE_INTERFACE_INDEX = new GUID
	{
		Data1 = 588329805u,
		Data2 = 51501,
		Data3 = 17855,
		Data4 = new byte[8] { 148, 150, 237, 244, 71, 130, 14, 45 }
	};

	public static readonly GUID FWPM_CONDITION_SOURCE_SUB_INTERFACE_INDEX = new GUID
	{
		Data1 = 90103197u,
		Data2 = 44242,
		Data3 = 17249,
		Data4 = new byte[8] { 141, 171, 249, 82, 93, 151, 102, 47 }
	};

	public static readonly GUID FWPM_CONDITION_DESTINATION_INTERFACE_INDEX = new GUID
	{
		Data1 = 902784290u,
		Data2 = 16697,
		Data3 = 17902,
		Data4 = new byte[8] { 160, 213, 103, 184, 9, 73, 216, 121 }
	};

	public static readonly GUID FWPM_CONDITION_DESTINATION_SUB_INTERFACE_INDEX = new GUID
	{
		Data1 = 729629593u,
		Data2 = 54471,
		Data3 = 18232,
		Data4 = new byte[8] { 162, 245, 233, 148, 180, 61, 163, 136 }
	};

	public static readonly GUID FWPM_CONDITION_ALE_APP_ID = new GUID
	{
		Data1 = 3616415367u,
		Data2 = 34372,
		Data3 = 20133,
		Data4 = new byte[8] { 148, 55, 216, 9, 236, 239, 201, 113 }
	};

	public static readonly GUID FWPM_CONDITION_ALE_USER_ID = new GUID
	{
		Data1 = 2936289802u,
		Data2 = 45901,
		Data3 = 20358,
		Data4 = new byte[8] { 151, 156, 201, 3, 113, 175, 110, 102 }
	};

	public static readonly GUID FWPM_CONDITION_ALE_REMOTE_USER_ID = new GUID
	{
		Data1 = 4130370487u,
		Data2 = 393,
		Data3 = 19120,
		Data4 = new byte[8] { 149, 164, 97, 35, 203, 250, 184, 98 }
	};

	public static readonly GUID FWPM_CONDITION_ALE_REMOTE_MACHINE_ID = new GUID
	{
		Data1 = 446988113u,
		Data2 = 32659,
		Data3 = 17672,
		Data4 = new byte[8] { 162, 113, 129, 171, 176, 12, 156, 171 }
	};

	public static readonly GUID FWPM_CONDITION_ALE_PROMISCUOUS_MODE = new GUID
	{
		Data1 = 479676278u,
		Data2 = 29058,
		Data3 = 18153,
		Data4 = new byte[8] { 175, 211, 176, 41, 16, 227, 3, 52 }
	};

	public static readonly GUID FWPM_CONDITION_ALE_SIO_FIREWALL_SYSTEM_PORT = new GUID
	{
		Data1 = 3119833224u,
		Data2 = 52120,
		Data3 = 20219,
		Data4 = new byte[8] { 162, 199, 173, 7, 51, 38, 67, 219 }
	};

	public static readonly GUID FWPM_CONDITION_ALE_REAUTH_REASON = new GUID
	{
		Data1 = 3028472359u,
		Data2 = 6521,
		Data3 = 19096,
		Data4 = new byte[8] { 128, 68, 24, 187, 230, 35, 117, 66 }
	};

	public static readonly GUID FWPM_CONDITION_ALE_NAP_CONTEXT = new GUID
	{
		Data1 = 1176984221u,
		Data2 = 49215,
		Data3 = 19831,
		Data4 = new byte[8] { 183, 132, 28, 87, 244, 208, 39, 83 }
	};

	public static readonly GUID FWPM_CONDITION_KM_AUTH_NAP_CONTEXT = new GUID
	{
		Data1 = 902883854u,
		Data2 = 5578,
		Data3 = 18731,
		Data4 = new byte[8] { 144, 14, 151, 253, 70, 53, 44, 206 }
	};

	public static readonly GUID FWPM_CONDITION_REMOTE_USER_TOKEN = new GUID
	{
		Data1 = 2616258150u,
		Data2 = 1737,
		Data3 = 16825,
		Data4 = new byte[8] { 132, 218, 40, 140, 180, 58, 245, 31 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_IF_UUID = new GUID
	{
		Data1 = 2090630559u,
		Data2 = 117,
		Data3 = 19765,
		Data4 = new byte[8] { 160, 209, 131, 17, 196, 207, 106, 241 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_IF_VERSION = new GUID
	{
		Data1 = 3938441655u,
		Data2 = 4706,
		Data3 = 18990,
		Data4 = new byte[8] { 173, 170, 95, 150, 246, 254, 50, 109 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_IF_FLAG = new GUID
	{
		Data1 = 596281906u,
		Data2 = 12697,
		Data3 = 18045,
		Data4 = new byte[8] { 135, 28, 39, 38, 33, 171, 56, 150 }
	};

	public static readonly GUID FWPM_CONDITION_DCOM_APP_ID = new GUID
	{
		Data1 = 4281236301u,
		Data2 = 12562,
		Data3 = 18288,
		Data4 = new byte[8] { 182, 54, 77, 36, 174, 58, 106, 242 }
	};

	public static readonly GUID FWPM_CONDITION_IMAGE_NAME = new GUID
	{
		Data1 = 3492077133u,
		Data2 = 57002,
		Data3 = 17175,
		Data4 = new byte[8] { 156, 133, 228, 14, 246, 225, 64, 195 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_PROTOCOL = new GUID
	{
		Data1 = 655866996u,
		Data2 = 14901,
		Data3 = 19687,
		Data4 = new byte[8] { 183, 239, 200, 56, 250, 189, 236, 69 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_AUTH_TYPE = new GUID
	{
		Data1 = 3669652651u,
		Data2 = 3431,
		Data3 = 17383,
		Data4 = new byte[8] { 152, 110, 117, 184, 79, 130, 245, 148 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_AUTH_LEVEL = new GUID
	{
		Data1 = 3852512981u,
		Data2 = 22956,
		Data3 = 18154,
		Data4 = new byte[8] { 190, 5, 165, 240, 94, 207, 68, 110 }
	};

	public static readonly GUID FWPM_CONDITION_SEC_ENCRYPT_ALGORITHM = new GUID
	{
		Data1 = 221277936u,
		Data2 = 59764,
		Data3 = 20340,
		Data4 = new byte[8] { 181, 199, 89, 27, 13, 167, 213, 98 }
	};

	public static readonly GUID FWPM_CONDITION_SEC_KEY_SIZE = new GUID
	{
		Data1 = 1198659643u,
		Data2 = 52472,
		Data3 = 19179,
		Data4 = new byte[8] { 188, 225, 198, 198, 22, 28, 143, 228 }
	};

	public static readonly GUID FWPM_CONDITION_IP_LOCAL_ADDRESS_V4 = new GUID
	{
		Data1 = 61221323u,
		Data2 = 28242,
		Data3 = 18936,
		Data4 = new byte[8] { 156, 65, 87, 9, 99, 60, 9, 207 }
	};

	public static readonly GUID FWPM_CONDITION_IP_LOCAL_ADDRESS_V6 = new GUID
	{
		Data1 = 595705476u,
		Data2 = 29988,
		Data3 = 17843,
		Data4 = new byte[8] { 160, 91, 30, 99, 125, 156, 122, 106 }
	};

	public static readonly GUID FWPM_CONDITION_PIPE = new GUID
	{
		Data1 = 466646045u,
		Data2 = 58335,
		Data3 = 20004,
		Data4 = new byte[8] { 134, 52, 118, 32, 70, 238, 246, 235 }
	};

	public static readonly GUID FWPM_CONDITION_IP_REMOTE_ADDRESS_V4 = new GUID
	{
		Data1 = 535541264u,
		Data2 = 15308,
		Data3 = 17889,
		Data4 = new byte[8] { 188, 54, 46, 6, 126, 44, 177, 134 }
	};

	public static readonly GUID FWPM_CONDITION_IP_REMOTE_ADDRESS_V6 = new GUID
	{
		Data1 = 611196300u,
		Data2 = 35822,
		Data3 = 16408,
		Data4 = new byte[8] { 155, 152, 49, 212, 88, 47, 51, 97 }
	};

	public static readonly GUID FWPM_CONDITION_PROCESS_WITH_RPC_IF_UUID = new GUID
	{
		Data1 = 3809575080u,
		Data2 = 48061,
		Data3 = 19732,
		Data4 = new byte[8] { 166, 94, 113, 87, 176, 98, 51, 187 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_EP_VALUE = new GUID
	{
		Data1 = 3704529081u,
		Data2 = 2182,
		Data3 = 17248,
		Data4 = new byte[8] { 156, 106, 171, 4, 58, 36, 251, 169 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_EP_FLAGS = new GUID
	{
		Data1 = 562790730u,
		Data2 = 2617,
		Data3 = 18872,
		Data4 = new byte[8] { 142, 113, 194, 12, 57, 199, 221, 46 }
	};

	public static readonly GUID FWPM_CONDITION_CLIENT_TOKEN = new GUID
	{
		Data1 = 3257465886u,
		Data2 = 16442,
		Data3 = 17528,
		Data4 = new byte[8] { 190, 5, 201, 186, 164, 192, 90, 206 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_SERVER_NAME = new GUID
	{
		Data1 = 3053822501u,
		Data2 = 50099,
		Data3 = 18631,
		Data4 = new byte[8] { 152, 51, 122, 239, 169, 82, 117, 70 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_SERVER_PORT = new GUID
	{
		Data1 = 2156983877u,
		Data2 = 39637,
		Data3 = 20027,
		Data4 = new byte[8] { 159, 159, 128, 35, 202, 9, 121, 9 }
	};

	public static readonly GUID FWPM_CONDITION_RPC_PROXY_AUTH_TYPE = new GUID
	{
		Data1 = 1083523042u,
		Data2 = 34149,
		Data3 = 18265,
		Data4 = new byte[8] { 132, 136, 23, 113, 180, 180, 181, 219 }
	};

	public static readonly GUID FWPM_CONDITION_CLIENT_CERT_KEY_LENGTH = new GUID
	{
		Data1 = 2750152903u,
		Data2 = 1524,
		Data3 = 19959,
		Data4 = new byte[8] { 145, 242, 95, 96, 217, 31, 244, 67 }
	};

	public static readonly GUID FWPM_CONDITION_CLIENT_CERT_OID = new GUID
	{
		Data1 = 3297881438u,
		Data2 = 63618,
		Data3 = 17027,
		Data4 = new byte[8] { 185, 22, 67, 107, 16, 63, 244, 173 }
	};

	public static readonly GUID FWPM_CONDITION_NET_EVENT_TYPE = new GUID
	{
		Data1 = 544119190u,
		Data2 = 18702,
		Data3 = 16591,
		Data4 = new byte[8] { 184, 49, 179, 134, 65, 235, 111, 203 }
	};

	public static readonly GUID FWPM_CONDITION_PEER_NAME = new GUID
	{
		Data1 = 2605944962u,
		Data2 = 60304,
		Data3 = 16774,
		Data4 = new byte[8] { 166, 204, 222, 91, 99, 35, 80, 22 }
	};

	public static readonly GUID FWPM_PROVIDER_IKEEXT = new GUID
	{
		Data1 = 279810582u,
		Data2 = 52446,
		Data3 = 17772,
		Data4 = new byte[8] { 139, 22, 233, 240, 78, 96, 169, 11 }
	};

	public static readonly GUID FWPM_PROVIDER_IPSEC_DOSP_CONFIG = new GUID
	{
		Data1 = 1013712297u,
		Data2 = 49244,
		Data3 = 19385,
		Data4 = new byte[8] { 131, 56, 35, 39, 129, 76, 232, 191 }
	};

	public static readonly GUID FWPM_PROVIDER_TCP_CHIMNEY_OFFLOAD = new GUID
	{
		Data1 = 2305466782u,
		Data2 = 39476,
		Data3 = 19403,
		Data4 = new byte[8] { 174, 121, 190, 185, 18, 124, 132, 185 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_INBOUND_TRANSPORT_V4 = new GUID
	{
		Data1 = 1362268173u,
		Data2 = 24196,
		Data3 = 19295,
		Data4 = new byte[8] { 128, 228, 1, 116, 30, 129, 255, 16 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_INBOUND_TRANSPORT_V6 = new GUID
	{
		Data1 = 1238609042u,
		Data2 = 10860,
		Data3 = 19919,
		Data4 = new byte[8] { 149, 95, 28, 59, 224, 9, 221, 153 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_OUTBOUND_TRANSPORT_V4 = new GUID
	{
		Data1 = 1262927626u,
		Data2 = 17699,
		Data3 = 20055,
		Data4 = new byte[8] { 170, 56, 168, 121, 135, 201, 16, 217 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_OUTBOUND_TRANSPORT_V6 = new GUID
	{
		Data1 = 953710370u,
		Data2 = 44419,
		Data3 = 20241,
		Data4 = new byte[8] { 169, 31, 223, 15, 176, 119, 34, 91 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_INBOUND_TUNNEL_V4 = new GUID
	{
		Data1 = 421169734u,
		Data2 = 3064,
		Data3 = 18127,
		Data4 = new byte[8] { 176, 69, 75, 69, 223, 166, 163, 36 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_INBOUND_TUNNEL_V6 = new GUID
	{
		Data1 = 2160280291u,
		Data2 = 7763,
		Data3 = 19823,
		Data4 = new byte[8] { 155, 68, 3, 223, 90, 238, 225, 84 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_OUTBOUND_TUNNEL_V4 = new GUID
	{
		Data1 = 1889802604u,
		Data2 = 33627,
		Data3 = 20400,
		Data4 = new byte[8] { 152, 232, 7, 95, 77, 151, 125, 70 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_OUTBOUND_TUNNEL_V6 = new GUID
	{
		Data1 = 4051915619u,
		Data2 = 42661,
		Data3 = 20066,
		Data4 = new byte[8] { 177, 128, 35, 219, 120, 157, 141, 166 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_FORWARD_INBOUND_TUNNEL_V4 = new GUID
	{
		Data1 = 679646771u,
		Data2 = 50416,
		Data3 = 20070,
		Data4 = new byte[8] { 135, 63, 132, 77, 178, 168, 153, 199 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_FORWARD_INBOUND_TUNNEL_V6 = new GUID
	{
		Data1 = 2941304514u,
		Data2 = 50822,
		Data3 = 17050,
		Data4 = new byte[8] { 136, 77, 183, 68, 67, 231, 176, 180 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_FORWARD_OUTBOUND_TUNNEL_V4 = new GUID
	{
		Data1 = 4216529206u,
		Data2 = 5579,
		Data3 = 17419,
		Data4 = new byte[8] { 147, 124, 23, 23, 202, 50, 12, 64 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_FORWARD_OUTBOUND_TUNNEL_V6 = new GUID
	{
		Data1 = 3672522956u,
		Data2 = 57377,
		Data3 = 19438,
		Data4 = new byte[8] { 158, 182, 164, 139, 39, 92, 140, 29 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_INBOUND_INITIATE_SECURE_V4 = new GUID
	{
		Data1 = 2113876123u,
		Data2 = 47741,
		Data3 = 19130,
		Data4 = new byte[8] { 145, 170, 174, 92, 102, 64, 201, 68 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_INBOUND_INITIATE_SECURE_V6 = new GUID
	{
		Data1 = 2845890265u,
		Data2 = 50572,
		Data3 = 18254,
		Data4 = new byte[8] { 138, 235, 60, 254, 153, 214, 213, 61 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_INBOUND_TUNNEL_ALE_ACCEPT_V4 = new GUID
	{
		Data1 = 1039591390u,
		Data2 = 64800,
		Data3 = 18674,
		Data4 = new byte[8] { 159, 38, 248, 84, 68, 76, 186, 121 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_INBOUND_TUNNEL_ALE_ACCEPT_V6 = new GUID
	{
		Data1 = 2716046035u,
		Data2 = 29356,
		Data3 = 18363,
		Data4 = new byte[8] { 135, 167, 1, 34, 198, 148, 52, 171 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_ALE_CONNECT_V4 = new GUID
	{
		Data1 = 1791050236u,
		Data2 = 63325,
		Data3 = 16899,
		Data4 = new byte[8] { 185, 200, 72, 230, 20, 156, 39, 18 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_ALE_CONNECT_V6 = new GUID
	{
		Data1 = 1275976197u,
		Data2 = 58143,
		Data3 = 18022,
		Data4 = new byte[8] { 144, 176, 179, 223, 173, 52, 18, 154 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_DOSP_FORWARD_V6 = new GUID
	{
		Data1 = 1829282626u,
		Data2 = 56222,
		Data3 = 20414,
		Data4 = new byte[8] { 158, 210, 87, 55, 76, 232, 159, 121 }
	};

	public static readonly GUID FWPM_CALLOUT_IPSEC_DOSP_FORWARD_V4 = new GUID
	{
		Data1 = 801855212u,
		Data2 = 52535,
		Data3 = 19279,
		Data4 = new byte[8] { 177, 8, 98, 194, 177, 133, 10, 12 }
	};

	public static readonly GUID FWPM_CALLOUT_WFP_TRANSPORT_LAYER_V4_SILENT_DROP = new GUID
	{
		Data1 = 3986720262u,
		Data2 = 9364,
		Data3 = 19832,
		Data4 = new byte[8] { 137, 188, 103, 131, 124, 3, 185, 105 }
	};

	public static readonly GUID FWPM_CALLOUT_WFP_TRANSPORT_LAYER_V6_SILENT_DROP = new GUID
	{
		Data1 = 2257833076u,
		Data2 = 41077,
		Data3 = 16726,
		Data4 = new byte[8] { 180, 118, 146, 134, 238, 206, 129, 78 }
	};

	public static readonly GUID FWPM_CALLOUT_TCP_CHIMNEY_CONNECT_LAYER_V4 = new GUID
	{
		Data1 = 4091611827u,
		Data2 = 11301,
		Data3 = 17017,
		Data4 = new byte[8] { 172, 54, 195, 15, 193, 129, 190, 196 }
	};

	public static readonly GUID FWPM_CALLOUT_TCP_CHIMNEY_CONNECT_LAYER_V6 = new GUID
	{
		Data1 = 971120773u,
		Data2 = 41793,
		Data3 = 17148,
		Data4 = new byte[8] { 162, 121, 174, 201, 78, 104, 156, 86 }
	};

	public static readonly GUID FWPM_CALLOUT_TCP_CHIMNEY_ACCEPT_LAYER_V4 = new GUID
	{
		Data1 = 3783519410u,
		Data2 = 14975,
		Data3 = 19284,
		Data4 = new byte[8] { 138, 217, 118, 5, 14, 216, 128, 202 }
	};

	public static readonly GUID FWPM_CALLOUT_TCP_CHIMNEY_ACCEPT_LAYER_V6 = new GUID
	{
		Data1 = 58249025u,
		Data2 = 49048,
		Data3 = 17923,
		Data4 = new byte[8] { 129, 242, 127, 18, 88, 96, 121, 246 }
	};

	public static readonly GUID FWPM_CALLOUT_SET_OPTIONS_AUTH_CONNECT_LAYER_V4 = new GUID
	{
		Data1 = 3159892608u,
		Data2 = 5751,
		Data3 = 16873,
		Data4 = new byte[8] { 148, 171, 194, 252, 177, 92, 46, 235 }
	};

	public static readonly GUID FWPM_CALLOUT_SET_OPTIONS_AUTH_CONNECT_LAYER_V6 = new GUID
	{
		Data1 = 2565158716u,
		Data2 = 47236,
		Data3 = 18703,
		Data4 = new byte[8] { 182, 95, 47, 106, 74, 87, 81, 149 }
	};

	public static readonly GUID FWPM_CALLOUT_TEREDO_ALE_RESOURCE_ASSIGNMENT_V6 = new GUID
	{
		Data1 = 834229138u,
		Data2 = 1646,
		Data3 = 17058,
		Data4 = new byte[8] { 183, 219, 146, 248, 172, 221, 86, 249 }
	};

	public static readonly GUID FWPM_CALLOUT_EDGE_TRAVERSAL_ALE_RESOURCE_ASSIGNMENT_V4 = new GUID
	{
		Data1 = 127602704u,
		Data2 = 61893,
		Data3 = 20429,
		Data4 = new byte[8] { 174, 5, 218, 65, 16, 122, 189, 11 }
	};

	public static readonly GUID FWPM_CALLOUT_TEREDO_ALE_LISTEN_V6 = new GUID
	{
		Data1 = 2175022311u,
		Data2 = 62988,
		Data3 = 17272,
		Data4 = new byte[8] { 186, 184, 198, 37, 163, 15, 1, 151 }
	};

	public static readonly GUID FWPM_CALLOUT_EDGE_TRAVERSAL_ALE_LISTEN_V4 = new GUID
	{
		Data1 = 860383925u,
		Data2 = 27998,
		Data3 = 20069,
		Data4 = new byte[8] { 160, 11, 167, 175, 237, 11, 169, 161 }
	};

	public static readonly GUID FWPM_PROVIDER_CONTEXT_SECURE_SOCKET_AUTHIP = new GUID
	{
		Data1 = 2992547840u,
		Data2 = 3330,
		Data3 = 18157,
		Data4 = new byte[8] { 146, 189, 127, 168, 75, 183, 62, 157 }
	};

	public static readonly GUID FWPM_PROVIDER_CONTEXT_SECURE_SOCKET_IPSEC = new GUID
	{
		Data1 = 2351776068u,
		Data2 = 63712,
		Data3 = 17088,
		Data4 = new byte[8] { 148, 206, 124, 207, 198, 59, 47, 155 }
	};

	public static readonly GUID FWPM_KEYING_MODULE_IKE = new GUID
	{
		Data1 = 2847668103u,
		Data2 = 33448,
		Data3 = 17851,
		Data4 = new byte[8] { 164, 0, 93, 126, 89, 82, 199, 169 }
	};

	public static readonly GUID FWPM_KEYING_MODULE_AUTHIP = new GUID
	{
		Data1 = 300145376u,
		Data2 = 56614,
		Data3 = 17808,
		Data4 = new byte[8] { 133, 125, 171, 75, 40, 209, 160, 149 }
	};

	public static readonly GUID FWPM_KEYING_MODULE_IKEV2 = new GUID
	{
		Data1 = 68653772u,
		Data2 = 36615,
		Data3 = 16797,
		Data4 = new byte[8] { 163, 148, 113, 105, 104, 203, 22, 71 }
	};
}
