namespace VpnSDK.Private.API;

internal enum ApiError
{
	InvalidApiKey,
	InvalidRequest,
	InvalidCredentials,
	TooManyInvalidRequests,
	AccessTokenExpired,
	RefreshTokenExpired,
	InternalServerError,
	InvalidMetadata,
	InactiveAccount,
	InvalidEntryExitServers,
	MissingEntryExitServers,
	ServerUnhealthy,
	ServerNotFound,
	DoublehopNotAvailable,
	FeatureNotAvailable,
	AllApiEndpointsUnreachable,
	Unknown
}
