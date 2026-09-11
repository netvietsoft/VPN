namespace VpnSDK.Private.OpenVpn.Enums;

public enum ConnectionState
{
	CONNECTING,
	WAIT,
	AUTH,
	GET_CONFIG,
	ASSIGN_IP,
	ADD_ROUTES,
	CONNECTED,
	RECONNECTING,
	TCP_CONNECT,
	EXITING
}
