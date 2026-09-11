namespace VpnSDK.Common.Routing;

internal enum RouteOperationResult
{
	None,
	Success,
	InterfaceNotFound,
	EmptyInterfaceName,
	LoopbackInterface
}
