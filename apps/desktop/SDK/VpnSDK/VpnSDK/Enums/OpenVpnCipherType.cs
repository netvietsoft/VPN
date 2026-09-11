using System.ComponentModel;

namespace VpnSDK.Enums;

public enum OpenVpnCipherType
{
	[Description("AES-128-CBC")]
	AES_128_CBC,
	[Description("AES-256-CBC")]
	AES_256_CBC
}
