namespace VpnSDK.Private.WFP.Filtering.Conditions;

public enum InterfaceType : uint
{
	Other = 1u,
	Ethernet = 6u,
	TokenRing = 9u,
	PPP = 23u,
	Loopback = 24u,
	ATM = 37u,
	FastEthernet = 62u,
	ISDN = 63u,
	Wireless = 71u,
	Tunnel = 131u,
	L2Vlan = 145u,
	L2IPVlan = 136u,
	XboxWireless = 281u
}
