using System.Runtime.Serialization;

namespace VpnSDK.Private.API.Common.Enums;

public enum Protocol
{
	[EnumMember(Value = "none")]
	None,
	[EnumMember(Value = "ikev2")]
	IKEv2,
	[EnumMember(Value = "openvpn")]
	OpenVPN,
	[EnumMember(Value = "wireguard")]
	Wireguard
}
